// google-login 這個 action 裡,依序要呼叫:

// 1. GoogleJsonWebSignature.ValidateAsync(request.IdToken, validationSettings) — 驗證前端傳來的 ID Token,validationSettings.Audience 要設成 appsettings 裡的 Google:ClientId,驗證失敗會丟例外
// 2. 從回傳的 payload.Email 呼叫 UserManager<AppUser>.FindByEmailAsync(email) 查帳號
// 3. 查不到的話:UserManager.CreateAsync(user)(不帶密碼參數)新建 AppUser,接著 UserManager.AddToRoleAsync(user, "User") 加預設角色
// 4. UserManager.GetRolesAsync(user) 撈這個使用者的角色清單(給/建都要撈,因為要放進 JWT claims)
// 5. 呼叫 GenerateJwtToken(private method,claims 帶 NameIdentifier/Email/Role)產生 JWT 字串
// 6. Response.Cookies.Append("access_token", token, new CookieOptions { HttpOnly = true, Secure = false, SameSite = SameSiteMode.Lax, Expires = ... }) 把 token 寫進 cookie


using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text; // for Encoding

using Google.Apis.Auth;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;

using Nooka.Api.Models;
using Nooka.Api.Models.Dtos;


[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IConfiguration _configuration;

    public AuthController(UserManager<AppUser> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }
    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin(GoogleLoginRequest request)
    {
        var validationSettings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = new[] { _configuration["Google:ClientId"] }
        };
        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, validationSettings);
        }
        catch (InvalidJwtException)
        // ValidateAsync 驗證失敗(token 過期、簽章不對、audience 不符)會丟 InvalidJwtException,這邊接住轉成 401
        {
            return Unauthorized();
        }

        var user = await _userManager.FindByEmailAsync(payload.Email);
        if (user is null)
        {
            user = new AppUser
            {
                UserName = payload.Email,
                Email = payload.Email,
                EmailConfirmed = true
            };
            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                return BadRequest(createResult.Errors);
            }
            await _userManager.AddToRoleAsync(user, "User");
        }

        var roles = await _userManager.GetRolesAsync(user); // GetRolesAsync 回傳的是 IList<string>
        var token = GenerateJwtToken(user, roles);

        Response.Cookies.Append("access_token", token, new CookieOptions
        // 後端把 token 寫進一個 cookie,叫 access_token
        {
            HttpOnly = true,
            // 這個 cookie 瀏覽器會保管,但前端 JavaScript(比如 document.cookie)讀不到它
            // 只有瀏覽器本身在發送 HTTP 請求時會自動把它夾帶上去
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddHours(7)
        });

        return Ok();
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("access_token");
        // Cookies.Delete 會送一個過期的 Set-Cookie 回去,瀏覽器收到後就會清掉這個 cookie
        return Ok();
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value);
        return Ok(new { email, roles });
    }





    private string GenerateJwtToken(AppUser user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
             new(ClaimTypes.Email, user.Email!)
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
        issuer: _configuration["Jwt:Issuer"],
        audience: _configuration["Jwt:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddHours(7),
        signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}