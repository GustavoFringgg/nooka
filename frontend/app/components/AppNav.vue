<script setup lang="ts">
const route = useRoute()

const links = [
  { label: "首頁", to: "/" },
  { label: "練習", to: "/practice" },
  { label: "學習紀錄", to: "#" },
  { label: "登入", to: "#" },
]
</script>

<template>
  <nav class="relative z-10 flex items-center justify-between max-w-7xl mx-auto px-8 py-6 font-body">
    <div class="font-display text-3xl tracking-tight text-hero-fg">
      Nooka<sup class="text-xs">&reg;</sup>
    </div>

    <div class="hidden md:flex items-center gap-8">
      <NuxtLink
        v-for="link in links"
        :key="link.label"
        :to="link.to"
        class="text-sm text-hero-muted transition-colors hover:text-hero-fg"
        :class="{ 'text-hero-fg': route.path === link.to }"
      >
        {{ link.label }}
      </NuxtLink>
    </div>

    <button
      class="liquid-glass-cta liquid-glass-hover rounded-full bg-transparent text-hero-fg font-body px-6 py-2.5 text-sm border-0"
    >
      開始學習
    </button>
  </nav>
</template>
