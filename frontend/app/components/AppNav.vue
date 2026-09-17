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
const links = [
  { label: "首頁", to: "/" },
  { label: "總覽", to: "/overview" },
  { label: "練習", to: "/practice" },
  { label: "學習紀錄", to: "#" }
]

const ctaLabel = computed(() => (loggedIn.value ? "登出" : "登入 / 註冊"))
const handleCtaClick = async () => {
  if (loggedIn.value) {
    openLogoutModal()
  } else {
    router.push("/login")
  }
}
</script>

<template>
  <nav
    class="relative z-10 flex items-center justify-between px-8 py-5 font-body bg-paper-bg border-b border-paper-fg/15"
  >
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

    <button
      class="rounded-full bg-paper-primary text-paper-bg font-body px-6 py-2.5 text-sm font-medium border-0 cursor-pointer transition-[transform,background-color] duration-250 ease-out hover:scale-[1.03] hover:bg-paper-accent"
      @click="handleCtaClick"
    >
      {{ ctaLabel }}
    </button>
  </nav>

  <UModal
    v-model:open="isLogoutModalOpen"
    title="要先離開了嗎？"
    :ui="{
      content: 'bg-paper-bg text-paper-fg ring-paper-fg/10 divide-paper-fg/10',
      header: 'border-paper-fg/10',
      footer: 'border-paper-fg/10',
      title: 'text-paper-fg font-display text-2xl font-normal',
      close: 'text-paper-muted hover:bg-paper-fg/10 hover:text-paper-fg',
      overlay: 'bg-paper-fg/40'
    }"
  >
    <template #body>
      <div class="flex items-start gap-4">
        <span
          class="flex w-11 h-11 shrink-0 items-center justify-center rounded-full bg-paper-accent/12 text-paper-accent"
        >
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
        <p class="text-paper-muted text-[15px] leading-relaxed pt-1.5">
          登出後, 你的學習進度都會好好留著, 下次用 Google 帳號登入就能繼續
        </p>
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
        <UButton
          label="登出"
          class="flex-1 justify-center bg-paper-accent text-paper-bg hover:bg-[#a8552f]"
          @click="confirmLogout"
        />
      </div>
    </template>
  </UModal>
</template>
