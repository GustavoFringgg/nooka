import type { AuthUser } from "~/types/auth"

export const useAuthUser = () => useState<AuthUser | null>("authUser", () => null)

export const fetchMe = async () => {
  const user = useAuthUser()
  try {
    user.value = await useApiFetch<AuthUser>("/api/auth/me")
  } catch {
    user.value = null
  }
}

export const loginWithGoogle = async (idToken: string) => {
  await useApiFetch("/api/auth/google-login", {
    method: "POST",
    body: { idToken }
  })
  await fetchMe()
}

export const logout = async () => {
  await useApiFetch("/api/auth/logout", {
    method: "POST"
  })
  const user = useAuthUser()
  user.value = null
}
