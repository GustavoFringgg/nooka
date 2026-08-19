<script setup lang="ts">
import type { QuizDirection } from "~/utils/quiz"

const route = useRoute()
const direction = (route.query.direction as QuizDirection) || "enToCn"
</script>

<template>
  <div class="min-h-screen bg-night-bg font-body text-night-fg pb-16">
    <AppNav />

    <div class="max-w-7xl mx-auto px-6 pt-12 text-center">
      <p class="text-night-muted">
        測驗方向:{{ direction === "enToCn" ? "看英文選中文" : "看中文選英文" }}
      </p>
      <p class="text-night-muted mt-2">(選擇題測驗畫面之後再做)</p>
    </div>
  </div>
</template>
