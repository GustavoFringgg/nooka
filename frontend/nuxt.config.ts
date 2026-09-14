// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: "2025-07-15",
  devtools: { enabled: true },
  modules: ["@nuxt/ui"],
  css: ["~/assets/css/main.css"],
  runtimeConfig: {
    public: {
      apiBase: "http://localhost:5016",
      googleClientId: ""
      // Nuxt 有個命名慣例: 環境變數只要叫 NUXT_PUBLIC_<KEY的大寫底線版>，啟動時就會自動覆蓋掉 runtimeConfig.public.<key> 對應的值——所以 NUXT_PUBLIC_GOOGLE_CLIENT_ID 會自動對應到 googleClientId,不用自己寫程式碼去讀 .env。
    }
  }
})
