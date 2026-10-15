<script setup lang="ts">
const route = useRoute()
const router = useRouter()
const loggedIn = useAuthUser()

const isLogoutModalOpen = ref(false)

const openLogoutModal = () => {
  isLogoutModalOpen.value = true
}

const confirmLogout = async () => {
  isLogoutModalOpen.value = false
  await logout()
}
const links = computed(() => {
  const base = [
    { label: "首頁", to: "/" },
    { label: "總覽", to: "/overview" },
    { label: "書架", to: "/practice" },
    { label: "學習紀錄", to: "/progress" }
  ]
  if (loggedIn.value?.roles.includes("Admin")) {
    base.push({ label: "資料管理", to: "/admin/categories" })
  }
  return base
})

const ctaLabel = computed(() => (loggedIn.value ? "登出" : "登入"))
const handleCtaClick = async () => {
  if (loggedIn.value) {
    openLogoutModal()
  } else {
    router.push("/login")
  }
}

const isMobileMenuOpen = ref(false)
watch(
  () => route.path,
  () => {
    isMobileMenuOpen.value = false
  }
)
</script>

<template>
  <nav class="sticky top-0 z-50 flex items-center justify-between px-6 md:px-8 py-5 font-body bg-paper-bg border-b border-paper-fg/15">
    <div class="font-display italic text-2xl tracking-tight text-paper-fg">
      Nooka
      <sup class="text-xs not-italic">&reg;</sup>
    </div>

    <div class="hidden md:flex items-center gap-8">
      <NuxtLink
        v-for="link in links"
        :key="link.label"
        :to="link.to"
        class="text-sm text-paper-muted transition-colors hover:text-paper-fg"
        :class="{ 'text-paper-fg border-b-2 border-paper-primary pb-0.5': route.path === link.to }"
      >
        {{ link.label }}
      </NuxtLink>
    </div>

    <div class="flex items-center gap-3 md:gap-0">
      <button
        class="hidden md:inline-block rounded-full bg-paper-primary text-paper-bg font-body px-6 py-2.5 text-sm font-medium border-0 cursor-pointer transition-[transform,background-color] duration-250 ease-out hover:scale-[1.03] hover:bg-paper-accent"
        @click="handleCtaClick"
      >
        {{ ctaLabel }}
      </button>

      <button
        class="md:hidden flex items-center justify-center w-9 h-9 rounded-full border-0 bg-transparent cursor-pointer text-paper-fg"
        :aria-expanded="isMobileMenuOpen"
        aria-label="開啟選單"
        @click="isMobileMenuOpen = !isMobileMenuOpen"
      >
        <svg v-if="!isMobileMenuOpen" width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round">
          <path d="M3 6h18M3 12h18M3 18h18" />
        </svg>
        <svg v-else width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round">
          <path d="M6 6l12 12M18 6L6 18" />
        </svg>
      </button>
    </div>

    <div
      v-if="isMobileMenuOpen"
      class="md:hidden absolute top-full right-0 w-44 rounded-b-2xl bg-paper-bg border border-t-0 border-paper-fg/15 shadow-[0_18px_30px_-20px_rgba(43,42,37,0.3)] px-4 py-4 flex flex-col items-center gap-3 text-center"
    >
      <NuxtLink
        v-for="link in links"
        :key="link.label"
        :to="link.to"
        class="text-[15px] text-paper-muted transition-colors hover:text-paper-fg"
        :class="{ 'text-paper-fg font-medium': route.path === link.to }"
      >
        {{ link.label }}
      </NuxtLink>
      <button
        class="mt-1 rounded-full bg-paper-primary text-paper-bg font-body px-6 py-2.5 text-sm font-medium border-0 cursor-pointer w-full"
        @click="handleCtaClick"
      >
        {{ ctaLabel }}
      </button>
    </div>
  </nav>

  <UModal v-model:open="isLogoutModalOpen" title="要先離開了嗎？">
    <template #body>
      <div class="flex items-start gap-4">
        <span class="flex w-11 h-11 shrink-0 items-center justify-center rounded-full bg-paper-accent/12 text-paper-accent">
          <svg
            width="20"
            height="20"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            stroke-width="2.5"
            stroke-linecap="round"
            stroke-linejoin="round"
          >
            <path d="M6 3h12a1 1 0 0 1 1 1v16l-7-4-7 4V4a1 1 0 0 1 1-1Z" />
          </svg>
        </span>
        <p class="text-paper-muted text-[15px] leading-relaxed pt-1.5">登出後, 學習進度都會好好留著, 下次登入就能繼續練習</p>
      </div>
    </template>
    <template #footer>
      <div class="flex gap-3 w-full">
        <UButton
          label="取消"
          color="neutral"
          variant="ghost"
          class="flex-1 justify-center text-paper-muted hover:bg-paper-fg/5 hover:text-paper-fg"
          @click="isLogoutModalOpen = false"
        />
        <UButton label="登出" class="flex-1 justify-center bg-paper-accent text-paper-bg hover:bg-[#a8552f]" @click="confirmLogout" />
      </div>
    </template>
  </UModal>
</template>
