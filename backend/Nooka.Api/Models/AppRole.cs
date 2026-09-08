using Microsoft.AspNetCore.Identity;
namespace Nooka.Api.Models;

public class AppRole : IdentityRole<int>
// AppRole 繼承 IdentityRole 所有屬性
{
}

// public class IdentityRole<TKey>
// {
//     public TKey Id { get; set; }
//     public string Name { get; set; }
//     public string NormalizedName { get; set; }
//     public string ConcurrencyStamp { get; set; }
// }