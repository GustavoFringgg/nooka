<script setup lang="ts">
import type { Word, QuizOption, QuizQuestion } from "~/types/practice"
import type { QuizDirection } from "~/utils/quiz"

interface AnswerLogEntry {
  question: QuizQuestion
  picked: QuizOption
  correct: boolean
}

const route = useRoute()
const currentIndex = ref(0)
const questions = ref<QuizQuestion[]>([])
const direction = (route.query.direction as QuizDirection) || "enToCn"
const categoryId = route.params.categoryId
const { data: words, pending, error } = await useFetch<Word[]>(useApiUrl(`/api/words/category/${categoryId}`))

if (words.value) questions.value = buildQuizQuestions(words.value, direction)

const currentQuestion = computed(() => questions.value[currentIndex.value])
const progressPercent = computed(() =>
  questions.value.length ? (currentIndex.value / questions.value.length) * 100 : 0
)

const score = ref(0)
const selectedOption = ref<QuizOption | null>(null)
const isAnswered = ref(false)
const isReviewOpen = ref(false)
const answerLog = ref<AnswerLogEntry[]>([])

const showSparkBurst = ref(false)

const AUTO_ADVANCE_SECONDS = 3
const autoAdvanceCountdown = ref(0)
let autoAdvanceTimer: ReturnType<typeof setInterval> | null = null

function clearAutoAdvance() {
  if (autoAdvanceTimer === null) return
  clearInterval(autoAdvanceTimer)
  autoAdvanceTimer = null
  autoAdvanceCountdown.value = 0
}

function startAutoAdvance() {
  clearAutoAdvance()
  autoAdvanceCountdown.value = AUTO_ADVANCE_SECONDS
  autoAdvanceTimer = setInterval(() => {
    autoAdvanceCountdown.value--
    if (autoAdvanceCountdown.value <= 0) {
      clearAutoAdvance()
      nextQuestion()
    }
  }, 1000)
}

onUnmounted(clearAutoAdvance)

const lastAnswerLog = computed(() => answerLog.value[answerLog.value.length - 1] ?? null)
const missed = computed(() => answerLog.value.filter((entry) => !entry.correct))

const reviewCards = computed(() => {
  const log = lastAnswerLog.value
  if (!log) return []
  return [
    { key: "picked", label: "你選的答案", tone: "wrong" as const, word: log.picked.word },
    { key: "correct", label: "正確答案", tone: "correct" as const, word: log.question.word }
  ]
})

function isCorrectOption(option: QuizOption) {
  return option.word.id === currentQuestion.value?.word.id
}

function selectOption(option: QuizOption) {
  if (isAnswered.value || !currentQuestion.value) return
  selectedOption.value = option
  isAnswered.value = true

  const correct = isCorrectOption(option)
  if (correct) {
    score.value++
    showSparkBurst.value = true
    startAutoAdvance()
  }
  answerLog.value.push({ question: currentQuestion.value, picked: option, correct })
  if (!correct) isReviewOpen.value = true
}

function optionClasses(option: QuizOption) {
  if (!isAnswered.value) return "liquid-glass-hover cursor-pointer"
  if (isCorrectOption(option)) {
    const base = "border-emerald-400/50 bg-emerald-400/8"
    return showSparkBurst.value ? `${base} overflow-visible` : base
  }
  if (option === selectedOption.value) return "border-rose-400/50 bg-rose-400/8"
  return "opacity-40"
}

function letterClasses(option: QuizOption) {
  if (isAnswered.value && isCorrectOption(option)) return "border-emerald-400/60 text-emerald-300"
  if (isAnswered.value && option === selectedOption.value) return "border-rose-400/60 text-rose-300"
  return "border-white/20 text-night-muted"
}

function nextQuestion() {
  clearAutoAdvance()
  isReviewOpen.value = false
  currentIndex.value++
  selectedOption.value = null
  isAnswered.value = false
  showSparkBurst.value = false
}

function restartQuiz() {
  clearAutoAdvance()
  if (words.value) questions.value = buildQuizQuestions(words.value, direction)
  currentIndex.value = 0
  score.value = 0
  selectedOption.value = null
  isAnswered.value = false
  answerLog.value = []
  isReviewOpen.value = false
  showSparkBurst.value = false
}
</script>

<template>
  <div class="min-h-screen bg-night-bg font-body text-night-fg pb-20">
    <AppNav />

    <p v-if="pending" class="text-center text-night-muted py-24">載入中...</p>
    <p v-else-if="error" class="text-center text-night-muted py-24">載入失敗,請稍後再試</p>

    <template v-else>
      <div v-if="currentQuestion" class="max-w-2xl mx-auto px-6 pt-8">
        <div class="mb-10">
          <div class="flex items-center justify-between mb-2 text-sm text-night-muted">
            <span>第 {{ currentIndex + 1 }} / {{ questions.length }} 題</span>
            <span class="text-night-accent">{{ score }} 分</span>
          </div>
          <div class="h-1.5 rounded-full bg-white/8 overflow-hidden">
            <div
              class="h-full rounded-full bg-night-accent transition-[width] duration-500 ease-out"
              :style="{ width: `${progressPercent}%` }"
            />
          </div>
        </div>

        <div
          :key="currentIndex"
          class="liquid-glass rounded-3xl px-8 py-16 text-center mb-8 shadow-[0_24px_60px_-20px_rgba(0,0,0,0.6)] animate-fade-rise"
        >
          <h1 class="font-display font-normal text-4xl sm:text-5xl leading-snug m-0">
            {{ currentQuestion.prompt }}
          </h1>
        </div>

        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4 mb-8">
          <button
            v-for="(option, i) in currentQuestion.options"
            :key="option.word.id"
            class="liquid-glass relative rounded-2xl px-5 py-5 pl-14 text-left transition-all duration-200"
            :class="optionClasses(option)"
            :disabled="isAnswered"
            @click="selectOption(option)"
          >
            <span
              class="absolute top-1/2 left-5 flex size-6 -translate-y-1/2 items-center justify-center rounded-full border text-[11px] font-medium transition-colors duration-200"
              :class="letterClasses(option)"
            >
              <svg
                v-if="isAnswered && isCorrectOption(option)"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                stroke-width="2.25"
                stroke-linecap="round"
                stroke-linejoin="round"
                class="size-3.5"
              >
                <path d="M5 13l4 4L19 7" />
              </svg>
              <svg
                v-else-if="isAnswered && option === selectedOption"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                stroke-width="2.25"
                stroke-linecap="round"
                stroke-linejoin="round"
                class="size-3.5"
              >
                <path d="M6 6l12 12M18 6L6 18" />
              </svg>
              <template v-else>{{ ["A", "B", "C", "D"][i] }}</template>
            </span>
            <SparkBurst
              v-if="showSparkBurst && isCorrectOption(option)"
              class="absolute top-1/2 left-5 -translate-y-1/2"
            />
            <span class="block text-base leading-relaxed">{{ option.text }}</span>
          </button>
        </div>

        <div v-if="isAnswered" class="flex items-center justify-center gap-6">
          <button
            v-if="lastAnswerLog && !lastAnswerLog.correct"
            class="text-sm text-night-muted underline decoration-white/20 underline-offset-4 transition-colors hover:text-night-fg cursor-pointer"
            @click="isReviewOpen = true"
          >
            再看一次詳解
          </button>
          <UButton
            :label="currentIndex + 1 < questions.length ? '下一題' : '查看結果'"
            size="xl"
            class="px-12"
            @click="nextQuestion"
          >
            <template v-if="lastAnswerLog?.correct" #trailing>
              <span :key="autoAdvanceCountdown" class="countdown-badge">{{ autoAdvanceCountdown }}</span>
            </template>
          </UButton>
        </div>
      </div>

      <div v-else class="max-w-2xl mx-auto px-6 pt-16 text-center">
        <h1 class="font-display font-normal text-4xl mb-2">測驗結束</h1>
        <p class="text-night-muted mb-10">
          你答對了
          <span class="text-night-accent">{{ score }}</span>
          / {{ questions.length }} 題
        </p>

        <div v-if="missed.length" class="text-left mb-12">
          <h2 class="font-display font-normal text-xl mb-4">這幾個單字再複習一下</h2>
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <article
              v-for="entry in missed"
              :key="entry.question.word.id"
              class="liquid-glass rounded-2xl p-5 bg-night-panel"
            >
              <div class="flex items-baseline justify-between mb-1.5">
                <h3 class="font-display font-normal text-lg m-0">{{ entry.question.word.term }}</h3>
                <span class="text-xs text-night-accent shrink-0">{{ entry.question.word.partOfSpeech }}</span>
              </div>
              <p class="text-sm text-night-muted m-0">{{ entry.question.word.definitionCN }}</p>
            </article>
          </div>
        </div>

        <div class="flex justify-center gap-4">
          <UButton label="再練習一次" @click="restartQuiz" />
          <UButton label="返回選擇分類" color="neutral" variant="outline" @click="navigateTo('/practice')" />
        </div>
      </div>
    </template>

    <UModal v-model:open="isReviewOpen" title="答錯了,來看看正確答案">
      <template #body>
        <div class="space-y-4">
          <div
            v-for="card in reviewCards"
            :key="card.key"
            class="rounded-2xl border p-5"
            :class="
              card.tone === 'correct' ? 'bg-night-accent/6 border-night-accent/25' : 'bg-rose-500/6 border-rose-500/20'
            "
          >
            <div
              class="inline-flex items-center gap-1.5 mb-3 rounded-full px-2.5 py-1 text-xs"
              :class="card.tone === 'correct' ? 'bg-night-accent/15 text-night-accent' : 'bg-rose-500/15 text-rose-300'"
            >
              <svg
                v-if="card.tone === 'correct'"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                stroke-width="2.25"
                stroke-linecap="round"
                stroke-linejoin="round"
                class="size-3"
              >
                <path d="M5 13l4 4L19 7" />
              </svg>
              <svg
                v-else
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                stroke-width="2.25"
                stroke-linecap="round"
                stroke-linejoin="round"
                class="size-3"
              >
                <path d="M6 6l12 12M18 6L6 18" />
              </svg>
              {{ card.label }}
            </div>
            <div class="flex items-baseline justify-between gap-3 mb-1.5">
              <h3 class="font-display font-normal text-xl m-0">{{ card.word.term }}</h3>
              <span class="text-xs text-night-muted shrink-0">{{ card.word.partOfSpeech }}</span>
            </div>
            <p class="text-[15px] mb-1">{{ card.word.definitionCN }}</p>
            <p class="text-[13px] text-night-muted mb-2">{{ card.word.definitionEN }}</p>
            <ul
              v-if="card.word.examples?.length"
              class="m-0 pl-[18px] text-xs text-night-muted leading-relaxed space-y-0.5"
            >
              <li v-for="(example, i) in card.word.examples" :key="i">{{ example }}</li>
            </ul>
          </div>
        </div>
      </template>

      <template #footer>
        <UButton label="繼續下一題" block @click="nextQuestion" />
      </template>
    </UModal>
  </div>
</template>
