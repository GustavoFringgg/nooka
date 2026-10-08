<script setup lang="ts">
const route = useRoute()

const links = [
  { label: "分類管理", to: "/admin/categories" },
  { label: "單字管理", to: "/admin/words" }
]
</script>

<template>
  <div class="flex min-h-screen bg-paper-bg-alt font-body text-paper-fg">
    <aside
      class="w-55 shrink-0 bg-paper-bg border-r border-paper-fg/12 px-4.5 py-7 flex flex-col gap-6"
    >
      <div>
        <div class="font-display italic text-[22px]">Nooka</div>
        <div class="mt-0.5 text-[11px] uppercase tracking-[0.08em] text-paper-muted">
          Admin 後台
        </div>
      </div>

      <nav class="flex flex-col gap-1">
        <NuxtLink
          v-for="link in links"
          :key="link.to"
          :to="link.to"
          class="flex items-center gap-2.5 rounded-[10px] px-3.5 py-2.5 text-sm text-paper-muted transition-colors hover:text-paper-fg"
          :class="{ 'bg-paper-primary/10 text-paper-primary font-semibold': route.path === link.to }"
        >
          {{ link.label }}
        </NuxtLink>
      </nav>

      <NuxtLink
        to="/"
        class="mt-auto px-3.5 text-[13px] text-paper-muted transition-colors hover:text-paper-fg"
      >
        ← 回到網站
      </NuxtLink>
    </aside>

    <main class="flex-1 min-w-0 px-11 py-10">
      <slot />
    </main>
  </div>
</template>
