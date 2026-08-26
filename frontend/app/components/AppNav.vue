<script setup lang="ts">
const route = useRoute()
const loggedIn = useDemoLoggedIn()

const links = [
  { label: "首頁", to: "/" },
  { label: "總覽", to: "/overview" },
  { label: "練習", to: "/practice" },
  { label: "學習紀錄", to: "#" },
  { label: "登入", to: "#" },
]

const ctaLabel = computed(() => (loggedIn.value ? "登出" : "登入 / 註冊"))
const toggleLoggedIn = () => {
  loggedIn.value = !loggedIn.value
}
</script>

<template>
  <nav
    class="relative z-10 flex items-center justify-between px-8 py-5 font-body bg-paper-bg border-b border-paper-fg/15"
  >
    <div class="font-display italic text-2xl tracking-tight text-paper-fg">
      Nooka<sup class="text-xs not-italic">&reg;</sup>
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
      @click="toggleLoggedIn"
    >
      {{ ctaLabel }}
    </button>
  </nav>
</template>
