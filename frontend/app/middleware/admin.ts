export default defineNuxtRouteMiddleware(() => {
  const user = useAuthUser()
  if (!user.value?.roles.includes("Admin")) {
    return navigateTo("/")
  }
})
