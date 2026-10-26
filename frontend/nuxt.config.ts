// https://nuxt.com/docs/api/configuration/nuxt-config

// NUXT_API_PROXY_TARGET 是後端真正的網址(本機 http://localhost:5016,正式環境是 Cloud Run 網址)
// 不放進 runtimeConfig.public,因為這個網址只在伺服器端(routeRules 的 proxy)用得到,不需要暴露給瀏覽器
const apiProxyTarget = process.env.NUXT_API_PROXY_TARGET || "http://localhost:5016"

export default defineNuxtConfig({
  compatibilityDate: "2025-07-15",
  devtools: { enabled: true },
  modules: ["@nuxt/ui"],
  ui: { colorMode: false },
  css: ["~/assets/css/main.css"],
  routeRules: {
    // 瀏覽器打 /api/** 都是打「自己的網域」,Nitro 在伺服器端把它代理轉發到真正的後端
    // 這樣後端回的 Set-Cookie 對瀏覽器來說永遠是同網域(第一方)設定的,不會被 iOS Safari/Chrome(WebKit)的 ITP 擋掉跨網域 cookie
    "/api/**": { proxy: `${apiProxyTarget}/api/**` }
  },
  runtimeConfig: {
    public: {
      apiBase: "",
      googleClientId: ""
      // Nuxt 有個命名慣例: 環境變數只要叫 NUXT_PUBLIC_<KEY的大寫底線版>，啟動時就會自動覆蓋掉 runtimeConfig.public.<key> 對應的值——所以 NUXT_PUBLIC_GOOGLE_CLIENT_ID 會自動對應到 googleClientId,不用自己寫程式碼去讀 .env。
    }
  }
})
