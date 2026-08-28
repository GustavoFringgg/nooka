<script setup lang="ts">
import type { Word, QuizOption, QuizQuestion } from "~/types/practice"
import type { QuizDirection } from "~/utils/quiz"

interface AnswerLogEntry {
  question: QuizQuestion
  picked: QuizOption
  correct: boolean
}

// TODO: 接上真的後端(mock 書本 categoryId 有真實資料)後,整段 mockWords + effectiveWords fallback 都要刪掉,
// 改回直接用 words.value(目前是因為前端還沒接後端,先讓沒 API 也能測選擇題畫面)
const mockWords: Word[] = [
  {
    id: -1,
    categoryId: -1,
    term: "vicarious",
    definitionCN: "替代的感受",
    definitionEN: "experienced through someone else's actions or feelings",
    partOfSpeech: "形容詞",
    examples: ["She felt a vicarious thrill watching her daughter perform."]
  },
  {
    id: -2,
    categoryId: -1,
    term: "haltingly",
    definitionCN: "吞吞吐吐地",
    definitionEN: "in a hesitant or uncertain manner",
    partOfSpeech: "副詞",
    examples: ["He spoke haltingly, unsure of the right words."]
  },
  {
    id: -3,
    categoryId: -1,
    term: "hectic",
    definitionCN: "忙亂的",
    definitionEN: "full of frantic activity",
    partOfSpeech: "形容詞",
    examples: ["It was a hectic day at the office."]
  },
  {
    id: -4,
    categoryId: -1,
    term: "hub",
    definitionCN: "中心樞紐",
    definitionEN: "the effective center of an activity or region",
    partOfSpeech: "名詞",
    examples: ["The airport is a major hub for international flights."]
  },
  {
    id: -5,
    categoryId: -1,
    term: "hysterical",
    definitionCN: "歇斯底里的",
    definitionEN: "affected by wildly uncontrolled emotion",
    partOfSpeech: "形容詞",
    examples: ["The crowd became hysterical when the band appeared."]
  },
  {
    id: -6,
    categoryId: -1,
    term: "handout",
    definitionCN: "講義;施捨",
    definitionEN: "a document given free to people at a meeting or class",
    partOfSpeech: "名詞",
    examples: ["The teacher gave each student a handout."]
  },
  {
    id: -7,
    categoryId: -1,
    term: "ambiguous",
    definitionCN: "模稜兩可的",
    definitionEN: "open to more than one interpretation",
    partOfSpeech: "形容詞",
    examples: ["The instructions were ambiguous and confused everyone."]
  },
  {
    id: -8,
    categoryId: -1,
    term: "candid",
    definitionCN: "坦率的",
    definitionEN: "truthful and straightforward",
    partOfSpeech: "形容詞",
    examples: ["She gave a candid answer to the reporter's question."]
  }
]

const route = useRoute()
const currentIndex = ref(0)
const questions = ref<QuizQuestion[]>([])
const direction = (route.query.direction as QuizDirection) || "enToCn"
const questionCount = Number(route.query.count) || undefined
const categoryId = route.params.categoryId
// TODO: mock fallback 刪除後,要把 error 加回來,失敗時顯示「載入失敗」而不是靜默 fallback
const { data: words, pending } = await useFetch<Word[]>(useApiUrl(`/api/words/category/${categoryId}`))

const effectiveWords = computed(() => (words.value && words.value.length > 0 ? words.value : mockWords))

if (effectiveWords.value.length) questions.value = buildQuizQuestions(effectiveWords.value, direction, questionCount)

const currentQuestion = computed(() => questions.value[currentIndex.value])
const progressPercent = computed(() =>
  questions.value.length ? (currentIndex.value / questions.value.length) * 100 : 0
)

const score = ref(0)
const selectedOption = ref<QuizOption | null>(null)
const isAnswered = ref(false)
const isReviewOpen = ref(false)
const answerLog = ref<AnswerLogEntry[]>([])
const highlightedIndex = ref(0)

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

const OPTION_COLS = 2

function moveHighlight(deltaRow: number, deltaCol: number) {
  const len = currentQuestion.value?.options.length
  if (!len) return
  const row = Math.floor(highlightedIndex.value / OPTION_COLS)
  const col = highlightedIndex.value % OPTION_COLS
  const maxRow = Math.floor((len - 1) / OPTION_COLS)
  const newRow = Math.min(Math.max(row + deltaRow, 0), maxRow)
  const newCol = Math.min(Math.max(col + deltaCol, 0), OPTION_COLS - 1)
  const newIndex = newRow * OPTION_COLS + newCol
  if (newIndex < len) highlightedIndex.value = newIndex
}

function confirmHighlighted() {
  const option = currentQuestion.value?.options[highlightedIndex.value]
  if (option) selectOption(option)
}

function handleKeydown(e: KeyboardEvent) {
  if (!currentQuestion.value) return

  if (isAnswered.value) {
    if (e.key === "Enter") {
      e.preventDefault()
      nextQuestion()
    }
    return
  }

  if (e.key === "ArrowRight") {
    e.preventDefault()
    moveHighlight(0, 1)
  } else if (e.key === "ArrowLeft") {
    e.preventDefault()
    moveHighlight(0, -1)
  } else if (e.key === "ArrowDown") {
    e.preventDefault()
    moveHighlight(1, 0)
  } else if (e.key === "ArrowUp") {
    e.preventDefault()
    moveHighlight(-1, 0)
  } else if (e.key === "Enter") {
    e.preventDefault()
    confirmHighlighted()
  }
}

onMounted(() => window.addEventListener("keydown", handleKeydown))
onUnmounted(() => window.removeEventListener("keydown", handleKeydown))

function optionClasses(option: QuizOption, index: number) {
  if (!isAnswered.value) {
    const base = "bg-paper-bg-alt hover:border-paper-primary/40 hover:-translate-y-0.5 cursor-pointer"
    if (index === highlightedIndex.value) return `border-paper-primary/60 ring-2 ring-paper-primary/25 ${base}`
    return `border-paper-fg/15 ${base}`
  }
  if (isCorrectOption(option)) {
    const base = "border-emerald-500/50 bg-emerald-500/8"
    return showSparkBurst.value ? `${base} overflow-visible` : base
  }
  if (option === selectedOption.value) return "border-rose-500/50 bg-rose-500/8"
  return "border-paper-fg/10 bg-paper-bg-alt opacity-40"
}

function letterClasses(option: QuizOption, index: number) {
  if (isAnswered.value && isCorrectOption(option)) return "border-emerald-500/60 text-emerald-600"
  if (isAnswered.value && option === selectedOption.value) return "border-rose-500/60 text-rose-600"
  if (!isAnswered.value && index === highlightedIndex.value) return "border-paper-primary/60 bg-paper-primary/10 text-paper-primary"
  return "border-paper-fg/20 text-paper-muted"
}

function nextQuestion() {
  clearAutoAdvance()
  isReviewOpen.value = false
  currentIndex.value++
  selectedOption.value = null
  isAnswered.value = false
  showSparkBurst.value = false
  highlightedIndex.value = 0
}

function restartQuiz() {
  clearAutoAdvance()
  questions.value = buildQuizQuestions(effectiveWords.value, direction, questionCount)
  currentIndex.value = 0
  score.value = 0
  selectedOption.value = null
  isAnswered.value = false
  answerLog.value = []
  isReviewOpen.value = false
  showSparkBurst.value = false
  highlightedIndex.value = 0
}
</script>

<template>
  <div class="min-h-screen bg-paper-bg font-body text-paper-fg pb-20" style="color-scheme: light">
    <AppNav />

    <p v-if="pending" class="text-center text-paper-muted py-24">載入中...</p>

    <template v-else>
      <div v-if="currentQuestion" class="max-w-2xl mx-auto px-6 pt-8">
        <div class="mb-10">
          <div class="flex items-center justify-between mb-2 text-sm text-paper-muted">
            <span>第 {{ currentIndex + 1 }} / {{ questions.length }} 題</span>
            <span class="text-paper-accent">{{ score }} 分</span>
          </div>
          <div class="h-1.5 rounded-full bg-paper-fg/10 overflow-hidden">
            <div
              class="h-full rounded-full bg-paper-primary transition-[width] duration-500 ease-out"
              :style="{ width: `${progressPercent}%` }"
            />
          </div>
        </div>

        <div
          :key="currentIndex"
          class="rounded-3xl px-8 py-16 text-center mb-8 bg-paper-bg-alt border-2 border-paper-fg/25 shadow-[0_18px_40px_-20px_rgba(43,42,37,0.3)] animate-fade-rise"
        >
          <h1 class="font-display font-normal text-4xl sm:text-5xl leading-snug m-0 text-paper-fg">
            {{ currentQuestion.prompt }}
          </h1>
        </div>

        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4 mb-8">
          <button
            v-for="(option, i) in currentQuestion.options"
            :key="option.word.id"
            class="relative rounded-2xl border px-5 py-5 pl-14 text-left transition-all duration-200"
            :class="optionClasses(option, i)"
            :disabled="isAnswered"
            @click="selectOption(option)"
            @mouseenter="highlightedIndex = i"
          >
            <span
              class="absolute top-1/2 left-5 flex size-6 -translate-y-1/2 items-center justify-center rounded-full border text-[11px] font-medium transition-colors duration-200"
              :class="letterClasses(option, i)"
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
            class="text-sm text-paper-muted underline decoration-paper-fg/20 underline-offset-4 transition-colors hover:text-paper-fg cursor-pointer"
            @click="isReviewOpen = true"
          >
            再看一次詳解
          </button>
          <UButton
            :label="currentIndex + 1 < questions.length ? '下一題' : '查看結果'"
            size="xl"
            class="px-12 bg-paper-primary text-paper-bg hover:bg-paper-accent"
            @click="nextQuestion"
          >
            <template v-if="lastAnswerLog?.correct" #trailing>
              <span :key="autoAdvanceCountdown" class="countdown-badge">{{ autoAdvanceCountdown }}</span>
            </template>
          </UButton>
        </div>
      </div>

      <div v-else class="max-w-2xl mx-auto px-6 pt-16 text-center">
        <h1 class="font-display font-normal text-4xl mb-2 text-paper-fg">測驗結束</h1>
        <p class="text-paper-muted mb-10">
          你答對了
          <span class="text-paper-accent">{{ score }}</span>
          / {{ questions.length }} 題
        </p>

        <div v-if="missed.length" class="text-left mb-12">
          <h2 class="font-display font-normal text-xl mb-4 text-paper-fg">這幾個單字再複習一下</h2>
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <article
              v-for="entry in missed"
              :key="entry.question.word.id"
              class="rounded-2xl p-5 bg-paper-bg-alt border border-paper-fg/10"
            >
              <div class="flex items-baseline justify-between mb-1.5">
                <h3 class="font-display font-normal text-lg m-0 text-paper-fg">{{ entry.question.word.term }}</h3>
                <span class="text-xs text-paper-accent shrink-0">{{ entry.question.word.partOfSpeech }}</span>
              </div>
              <p class="text-sm text-paper-muted m-0">{{ entry.question.word.definitionCN }}</p>
            </article>
          </div>
        </div>

        <div class="flex justify-center gap-4">
          <UButton label="再練習一次" class="bg-paper-primary text-paper-bg hover:bg-paper-accent" @click="restartQuiz" />
          <UButton
            label="返回選擇分類"
            color="neutral"
            variant="outline"
            class="bg-transparent border-paper-fg/25 text-paper-fg hover:bg-paper-fg/5"
            @click="navigateTo('/practice')"
          />
        </div>
      </div>
    </template>

    <UModal
      v-model:open="isReviewOpen"
      title="答錯了,來看看正確答案"
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
        <div class="space-y-4">
          <div
            v-for="card in reviewCards"
            :key="card.key"
            class="rounded-2xl border p-5"
            :class="
              card.tone === 'correct'
                ? 'bg-paper-primary/6 border-paper-primary/25'
                : 'bg-rose-500/6 border-rose-500/20'
            "
          >
            <div
              class="inline-flex items-center gap-1.5 mb-3 rounded-full px-2.5 py-1 text-xs"
              :class="
                card.tone === 'correct'
                  ? 'bg-paper-primary/15 text-paper-primary'
                  : 'bg-rose-500/15 text-rose-600'
              "
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
              <h3 class="font-display font-normal text-xl m-0 text-paper-fg">{{ card.word.term }}</h3>
              <span class="text-xs text-paper-muted shrink-0">{{ card.word.partOfSpeech }}</span>
            </div>
            <p class="text-[15px] mb-1 text-paper-fg">{{ card.word.definitionCN }}</p>
            <p class="text-[13px] text-paper-muted mb-2">{{ card.word.definitionEN }}</p>
            <ul
              v-if="card.word.examples?.length"
              class="m-0 pl-[18px] text-xs text-paper-muted leading-relaxed space-y-0.5"
            >
              <li v-for="(example, i) in card.word.examples" :key="i">{{ example }}</li>
            </ul>
          </div>
        </div>
      </template>

      <template #footer>
        <UButton
          label="繼續下一題"
          block
          class="bg-paper-primary text-paper-bg hover:bg-paper-accent"
          @click="nextQuestion"
        />
      </template>
    </UModal>
  </div>
</template>
