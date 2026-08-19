<script setup lang="ts">
import type { Category } from "~/types/practice"
import { useApiUrl } from "#imports"

const result = await useFetch<Category[]>(useApiUrl("/api/categories"))
const categories = result.data

const formatDate = (iso: string) =>
  new Date(iso).toLocaleDateString("zh-TW", { year: "numeric", month: "2-digit", day: "2-digit" })
</script>

<template>
  <div class="min-h-screen bg-night-bg font-body text-night-fg pb-16">
    <AppNav />

    <header class="max-w-7xl mx-auto mt-12 mb-12 px-6 text-center">
      <h1 class="font-display font-normal text-[44px] m-0 mb-4">今天想練習哪本書</h1>
    </header>

    <div class="max-w-7xl mx-auto px-6 pb-8 flex flex-wrap items-end gap-10 border-b-[3px] border-white/8">
      <NuxtLink
        v-for="category in categories"
        :key="category.id"
        :to="`/practice/${category.id}`"
        class="liquid-glass liquid-glass-hover relative flex flex-col w-40 aspect-[3/4.3] rounded-l-[4px] rounded-r-[14px] pt-7 pr-[22px] pb-6 pl-[34px] no-underline text-inherit bg-night-panel shadow-[4px_4px_0_rgba(0,0,0,0.3),8px_8px_0_rgba(0,0,0,0.18)] before:content-[''] before:absolute before:inset-y-0 before:left-0 before:w-2.5 before:bg-night-accent"
      >
        <h2 class="font-display font-normal text-2xl leading-tight m-0 mb-3 text-night-fg">{{ category.name }}</h2>
        <p class="text-[13px] text-night-muted leading-relaxed m-0 grow">{{ category.description }}</p>
        <span class="text-[11px] text-night-accent mt-4">最後更新 {{ formatDate(category.updatedAt) }}</span>
      </NuxtLink>
    </div>
  </div>
</template>
