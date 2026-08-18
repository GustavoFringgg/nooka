<script setup lang="ts">
import type { Category } from "~/types/practice"

// TODO: mock 資料,之後換成真的 API
// const { data: categories } = await useFetch<Category[]>(useApiUrl("/api/categories"))
const categories: Category[] = [
  {
    id: 1,
    name: "多益(600)",
    description: "這是多益600分以下常出現高頻率的單字",
    createdAt: "2026-08-14T00:00:00Z",
    updatedAt: "2026-08-18T00:00:00Z"
  },
  {
    id: 2,
    name: "航空用",
    description: "航空業常見的專業詞彙,適合空服員、地勤人員練習",
    createdAt: "2026-08-14T00:00:00Z",
    updatedAt: "2026-08-16T00:00:00Z"
  },
  {
    id: 3,
    name: "資安用",
    description: "資訊安全領域常見術語",
    createdAt: "2026-08-10T00:00:00Z",
    updatedAt: "2026-08-10T00:00:00Z"
  }
]

const formatDate = (iso: string) =>
  new Date(iso).toLocaleDateString("zh-TW", { year: "numeric", month: "2-digit", day: "2-digit" })
</script>

<template>
  <div class="practice">
    <AppNav />

    <header class="practice__header">
      <h1 class="practice__title">今天想練習哪本書</h1>
    </header>

    <div class="practice__grid">
      <NuxtLink
        v-for="category in categories"
        :key="category.id"
        :to="`/practice/${category.id}/choice`"
        class="book-card"
      >
        <h2 class="book-card__name">{{ category.name }}</h2>
        <p class="book-card__description">{{ category.description }}</p>
        <span class="book-card__updated">最後更新 {{ formatDate(category.updatedAt) }}</span>
      </NuxtLink>
    </div>
  </div>
</template>

<style lang="scss" scoped>
@use "../../assets/scss/tokens" as *;
@use "../../assets/scss/liquid-glass" as *;

.practice {
  min-height: 100vh;
  background: $night-bg;
  font-family: $font-body;
  color: $night-foreground;
  padding-bottom: 64px;

  &__header {
    max-width: 1280px;
    margin: 48px auto 48px;
    padding: 0 24px;
    text-align: center;
  }

  &__title {
    font-family: $font-display;
    font-weight: 400;
    font-size: 44px;
    margin: 0 0 16px;
  }

  &__subtitle {
    color: $night-muted-foreground;
    font-size: 16px;
  }

  &__grid {
    max-width: 1280px;
    margin: 0 auto;
    padding: 0 24px 32px;
    display: flex;
    flex-wrap: wrap;
    align-items: flex-end;
    gap: 40px;
    border-bottom: 3px solid rgba(255, 255, 255, 0.08);
  }
}

.book-card {
  @include liquid-glass;
  @include liquid-glass-hover;
  position: relative;
  display: flex;
  flex-direction: column;
  width: 160px;
  aspect-ratio: 3 / 4.3;
  border-radius: 4px 14px 14px 4px;
  padding: 28px 22px 24px 34px;
  text-decoration: none;
  color: inherit;
  background-color: $night-bg-panel;
  box-shadow:
    4px 4px 0 rgba(0, 0, 0, 0.3),
    8px 8px 0 rgba(0, 0, 0, 0.18);

  &::before {
    content: "";
    position: absolute;
    inset: 0 auto 0 0;
    width: 10px;
    background: $night-accent;
  }

  &__name {
    font-family: $font-display;
    font-weight: 400;
    font-size: 24px;
    line-height: 1.25;
    margin: 0 0 12px;
    color: $night-foreground;
  }

  &__description {
    font-size: 13px;
    color: $night-muted-foreground;
    line-height: 1.6;
    margin: 0;
    flex-grow: 1;
  }

  &__updated {
    font-size: 11px;
    color: $night-accent;
    margin-top: 16px;
  }
}
</style>
