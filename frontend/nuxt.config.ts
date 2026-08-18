// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: "2025-07-15",
  devtools: { enabled: true },
  css: ["~/assets/scss/_reset.scss"],
  runtimeConfig: {
    public: {
      apiBase: "http://localhost:5016"
    }
  }
})
