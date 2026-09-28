export const useApiUrl = (path: string) => {
  const config = useRuntimeConfig()
  // 呼叫 Nuxt 內建的 useRuntimeConfig(),拿到你在 nuxt.config.ts 裡設定的那個 runtimeConfig 物件,存進 config 這個變數。
  return `${config.public.apiBase}${path}`
}

// 呼叫 useApiUrl("/api/categories"),會回傳 "http://localhost:5016/api/categories"

export const useApiFetch = <T>(path: string, options: Record<string, unknown> = {}) => {
  const headers = import.meta.server ? useRequestHeaders(["cookie"]) : undefined
  return $fetch<T>(useApiUrl(path), { credentials: "include", headers, ...options })
}

// 1. import.meta.server:Nuxt 提供的判斷式,true 代表這段程式碼現在是在伺服器端執行(不是瀏覽器)
// 2. useRequestHeaders(["cookie"]):Nuxt 內建的 function,讀「這次進來的請求」帶了哪些 header,這裡只挑 cookie 這個欄位出來
// 3. 瀏覽器端(import.meta.server 是 false)的話 headers 是 undefined,不影響原本的行為(因為瀏覽器本來就會靠 credentials: "include" 自動帶 cookie,不需要手動塞)
