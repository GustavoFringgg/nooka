<script setup lang="ts">
import type { Category, Word } from "~/types/practice"
import type { QuizDirection } from "~/utils/quiz"

const route = useRoute()
const router = useRouter()
const categoryId = route.params.categoryId

const { data: category } = await useFetch<Category>(useApiUrl(`/api/categories/${categoryId}`))

const { data: words, pending, error } = await useFetch<Word[]>(useApiUrl(`/api/words/category/${categoryId}`))

const isChoiceSettingsOpen = ref(false)
const direction = ref<QuizDirection>("enToCn")

const directionOptions = [
  { label: "看英文選中文", value: "enToCn" },
  { label: "看中文選英文", value: "cnToEn" }
]

function startChoiceQuiz() {
  isChoiceSettingsOpen.value = false
  router.push(`/practice/${categoryId}/choice?direction=${direction.value}`)
}
</script>

<template>
  <div class="min-h-screen bg-night-bg font-body text-night-fg pb-16">
    <AppNav />

    <header class="max-w-7xl mx-auto mt-12 mb-8 px-6 text-center">
      <h1 class="font-display font-normal text-4xl m-0">{{ category?.name }}</h1>
    </header>

    <div class="max-w-7xl mx-auto mb-10 px-6 flex flex-wrap gap-4">
      <button
        class="liquid-glass liquid-glass-hover px-6 py-3 font-body text-sm rounded-full cursor-pointer border-night-accent/50 text-night-accent"
        @click="isChoiceSettingsOpen = true"
      >
        選擇題
      </button>

      <UModal v-model:open="isChoiceSettingsOpen" title="設定你的測驗" description="選擇練習方向後開始測驗">
        <template #body>
          <URadioGroup v-model="direction" :items="directionOptions" />
        </template>

        <template #footer>
          <UButton label="開始測試" block @click="startChoiceQuiz" />
        </template>
      </UModal>
      <button
        class="liquid-glass px-6 py-3 font-body text-sm text-night-fg rounded-full cursor-not-allowed opacity-50"
        disabled
      >
        消消樂 · 即將推出
      </button>
      <button
        class="liquid-glass px-6 py-3 font-body text-sm text-night-fg rounded-full cursor-not-allowed opacity-50"
        disabled
      >
        打字拼寫 · 即將推出
      </button>
    </div>

    <p v-if="pending" class="text-center text-night-muted py-10">載入中...</p>
    <p v-else-if="error" class="text-center text-night-muted py-10">載入失敗,請稍後再試</p>

    <div v-else class="max-w-7xl mx-auto px-6 grid grid-cols-[repeat(auto-fill,minmax(260px,1fr))] gap-5">
      <article v-for="word in words" :key="word.id" class="liquid-glass p-5 bg-night-panel">
        <div class="flex items-baseline justify-between mb-2">
          <h2 class="font-display font-normal text-[22px] m-0 text-night-fg">{{ word.term }}</h2>
          <span class="text-xs text-night-accent">{{ word.partOfSpeech }}</span>
        </div>
        <p class="mb-1 text-[15px]">{{ word.definitionCN }}</p>
        <p class="mb-3 text-[13px] text-night-muted">{{ word.definitionEN }}</p>
        <ul v-if="word.examples?.length" class="m-0 pl-[18px] text-xs text-night-muted leading-relaxed">
          <li v-for="(example, i) in word.examples" :key="i">{{ example }}</li>
        </ul>
      </article>
    </div>
  </div>
</template>
