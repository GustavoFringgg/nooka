<script setup lang="ts">
const route = useRoute()

const links = [
  { label: "首頁", to: "/" },
  { label: "分類", to: "#" },
  { label: "練習", to: "/practice" },
  { label: "學習紀錄", to: "#" },
  { label: "登入", to: "#" },
]
</script>

<template>
  <nav class="app-nav">
    <div class="app-nav__logo">Nooka<sup>&reg;</sup></div>

    <div class="app-nav__links">
      <NuxtLink
        v-for="link in links"
        :key="link.label"
        :to="link.to"
        class="app-nav__link"
        :class="{ 'app-nav__link--active': route.path === link.to }"
      >
        {{ link.label }}
      </NuxtLink>
    </div>

    <button class="app-nav__cta">開始學習</button>
  </nav>
</template>

<style lang="scss" scoped>
@use "../assets/scss/tokens" as *;
@use "../assets/scss/liquid-glass" as *;

.app-nav {
  position: relative;
  z-index: 10;
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 24px 32px;
  max-width: 1280px;
  margin: 0 auto;
  font-family: $font-body;

  &__logo {
    font-family: $font-display;
    font-size: 30px;
    letter-spacing: -0.02em;
    color: $color-foreground;

    sup {
      font-size: 12px;
    }
  }

  &__links {
    display: none;
    gap: 32px;
    align-items: center;

    @media (min-width: 768px) {
      display: flex;
    }
  }

  &__link {
    font-size: 14px;
    color: $color-muted-foreground;
    text-decoration: none;
    transition: color 0.2s ease;

    &:hover {
      color: $color-foreground;
    }

    &--active {
      color: $color-foreground;
    }
  }

  &__cta {
    @include liquid-glass(0.01, 0.1, 0.3);
    @include liquid-glass-hover;
    border-radius: 999px;
    color: $color-foreground;
    background-color: transparent;
    font-family: inherit;
    padding: 10px 24px;
    font-size: 14px;
    border: none;
  }
}
</style>
