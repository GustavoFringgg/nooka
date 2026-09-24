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
