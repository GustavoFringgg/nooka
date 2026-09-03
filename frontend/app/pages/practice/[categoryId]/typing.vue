<script setup lang="ts">
import type { Word, TypingQuestion } from "~/types/practice"

type PartOfSpeech = "形容詞" | "副詞" | "動詞" | "名詞" | "代名詞" | "介系詞" | "連接詞" | "感嘆詞"

const posColors: Record<PartOfSpeech, { bg: string; text: string }> = {
  形容詞: { bg: "rgba(193,101,59,.16)", text: "#8f4128" },
  副詞: { bg: "rgba(60,90,68,.16)", text: "#2c433a" },
  動詞: { bg: "rgba(156,74,60,.16)", text: "#78392e" },
  名詞: { bg: "rgba(62,110,142,.16)", text: "#2d5670" },
  代名詞: { bg: "rgba(107,76,110,.16)", text: "#503a52" },
  介系詞: { bg: "rgba(138,122,62,.16)", text: "#6a5e2f" },
  連接詞: { bg: "rgba(47,110,107,.16)", text: "#235350" },
  感嘆詞: { bg: "rgba(161,63,94,.16)", text: "#7a2f47" }
}

const defaultPosColor = { bg: "rgba(120,120,120,.16)", text: "#555555" }

function posColor(pos: string) {
  return posColors[pos as PartOfSpeech] ?? defaultPosColor
}

const route = useRoute()
const categoryId = route.params.categoryId
const questionCount = Number(route.query.count) || undefined
const { data: words, pending, error } = await useFetch<Word[]>(useApiUrl(`/api/words/category/${categoryId}`))

const questions = ref<TypingQuestion[]>([])
if (words.value?.length) questions.value = buildTypingQuestions(words.value, questionCount)

const currentIndex = ref(0)
const currentQuestion = computed(() => questions.value[currentIndex.value])
const isLastQuestion = computed(() => currentIndex.value + 1 >= questions.value.length)
const progressPercent = computed(() =>
  questions.value.length ? (currentIndex.value / questions.value.length) * 100 : 0
)

const score = ref(0)
const letters = ref<string[]>([])
const resultMode = ref<"correct" | "wrongRetry" | "skipped" | null>(null)
const reviewWordIds = ref(new Set<number>())
const missedWords = computed(() => words.value?.filter((w) => reviewWordIds.value.has(w.id)) ?? [])

const letterInputs = ref<(HTMLInputElement | null)[]>([])
function setLetterRef(el: Element | null, idx: number) {
  letterInputs.value[idx] = (el as HTMLInputElement) ?? null
}
function focusLetter(idx: number) {
  nextTick(() => letterInputs.value[idx]?.focus())
}

function speak(text: string) {
  if (typeof window === "undefined" || !window.speechSynthesis) return
  const utterance = new SpeechSynthesisUtterance(text)
  utterance.lang = "en-US"
  speechSynthesis.speak(utterance)
}

const AUTO_ADVANCE_SECONDS = 3
const autoAdvanceCountdown = ref(0)
let autoAdvanceTimer: ReturnType<typeof setInterval> | null = null
let revealTimer: ReturnType<typeof setTimeout> | null = null

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

function clearRevealTimer() {
  if (revealTimer === null) return
  clearTimeout(revealTimer)
  revealTimer = null
}

function resetLetters() {
  const term = currentQuestion.value?.word.term ?? ""
  letters.value = new Array(term.length).fill("")
  resultMode.value = null
}

watch(
  currentQuestion,
  (q) => {
    clearAutoAdvance()
    clearRevealTimer()
    resetLetters()
    if (q) {
      speak(q.word.term)
      focusLetter(0)
    }
  },
  { immediate: true }
)

function handleLetterInput(e: Event, idx: number) {
  if (resultMode.value) return
  const value = (e.target as HTMLInputElement).value.slice(-1)
  letters.value[idx] = value
  if (value && idx < letters.value.length - 1) focusLetter(idx + 1)
  if (letters.value.every((l) => l)) checkAnswer()
}

function handleLetterKeydown(e: KeyboardEvent, idx: number) {
  if (e.key === "Backspace" && !letters.value[idx] && idx > 0) focusLetter(idx - 1)
}

function checkAnswer() {
  if (!currentQuestion.value || resultMode.value) return
  const guess = letters.value.join("").trim().toLowerCase()
  const answer = currentQuestion.value.word.term.trim().toLowerCase()

  if (guess === answer) {
    score.value++
    resultMode.value = "correct"
    startAutoAdvance()
    return
  }

  reviewWordIds.value.add(currentQuestion.value.word.id)
  resultMode.value = "wrongRetry"
  speak(currentQuestion.value.word.term)
  revealTimer = setTimeout(() => {
    resetLetters()
    focusLetter(0)
  }, 2500)
}

function skipQuestion() {
  if (!currentQuestion.value || resultMode.value) return
  reviewWordIds.value.add(currentQuestion.value.word.id)
  letters.value = currentQuestion.value.word.term.split("")
  resultMode.value = "skipped"
  speak(currentQuestion.value.word.term)
}

function nextQuestion() {
  clearAutoAdvance()
  currentIndex.value++
}

function restartQuiz() {
  clearAutoAdvance()
  clearRevealTimer()
  questions.value = buildTypingQuestions(words.value!, questionCount)
  currentIndex.value = 0
  score.value = 0
  reviewWordIds.value = new Set()
}

onUnmounted(() => {
  clearAutoAdvance()
  clearRevealTimer()
  if (typeof window !== "undefined" && window.speechSynthesis) speechSynthesis.cancel()
})

const letterBoxClasses = computed(() => {
  if (resultMode.value === "correct") return "border-paper-primary bg-paper-primary/8 text-paper-primary"
  if (resultMode.value === "wrongRetry" || resultMode.value === "skipped")
    return "border-rose-500/50 bg-rose-500/8 text-rose-600"
  return "border-paper-fg/20 focus:border-paper-primary"
})
</script>

<template>
  <div class="min-h-screen bg-paper-bg font-body text-paper-fg pb-20" style="color-scheme: light">
    <AppNav />

    <p v-if="pending" class="text-center text-paper-muted py-24">載入中...</p>
    <p v-else-if="error" class="text-center text-paper-muted py-24">載入失敗...</p>

    <template v-else>
      <div v-if="currentQuestion" class="max-w-3xl mx-auto px-6 pt-8">
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
          class="rounded-3xl px-8 py-12 text-center mb-8 bg-paper-bg-alt border-2 border-paper-fg/25 shadow-[0_18px_40px_-20px_rgba(43,42,37,0.3)] animate-fade-rise"
        >
          <div class="flex items-center justify-center gap-2 mb-1.5">
            <h1 class="font-display font-normal text-4xl m-0 text-paper-fg">
              {{ currentQuestion.prompt }}
            </h1>
            <button
              type="button"
              class="w-10 h-10 rounded-full flex items-center justify-center text-paper-muted cursor-pointer transition-colors duration-150 hover:bg-paper-fg/8 hover:text-paper-primary"
              @click="speak(currentQuestion.word.term)"
            >
              <svg width="30" height="30" viewBox="0 0 24 24" fill="currentColor">
                <path d="M3 9v6h4l5 5V4L7 9H3z" />
                <path d="M16.5 12c0-1.77-.77-3.29-2-4.24v8.48c1.23-.95 2-2.47 2-4.24z" opacity=".7" />
              </svg>
            </button>
          </div>

          <div class="flex items-center justify-center gap-2 mb-9">
            <span
              class="text-[11px] px-1.5 py-0.5 rounded-full shrink-0"
              :style="{
                color: posColor(currentQuestion.word.partOfSpeech).text,
                background: posColor(currentQuestion.word.partOfSpeech).bg
              }"
            >
              {{ currentQuestion.word.partOfSpeech }}
            </span>
            <span
              class="text-[#9c9384] text-[12.5px]"
              style="font-family: ui-monospace, &quot;SF Mono&quot;, monospace"
            >
              /{{ currentQuestion.word.ipa ?? "" }}/
            </span>
          </div>

          <div class="relative flex justify-center">
            <SparkBurst
              v-if="resultMode === 'correct'"
              class="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2"
            />
            <div class="flex justify-center gap-2 flex-wrap">
              <input
                v-for="(letter, idx) in letters"
                :key="idx"
                :ref="(el) => setLetterRef(el, idx)"
                type="text"
                maxlength="1"
                autocomplete="off"
                :value="letter"
                :disabled="resultMode !== null"
                class="w-11 h-14 rounded-[10px] border-2 text-center font-display text-2xl lowercase bg-white outline-none transition-colors"
                :class="letterBoxClasses"
                @input="handleLetterInput($event, idx)"
                @keydown="handleLetterKeydown($event, idx)"
              />
            </div>
          </div>

          <div
            v-if="resultMode"
            class="mt-7 rounded-2xl px-6 py-4 animate-fade-rise"
            :class="resultMode === 'correct' ? 'bg-paper-primary/8' : 'bg-rose-500/8'"
          >
            <p v-if="resultMode === 'correct'" class="text-paper-primary text-lg m-0">太棒了，拼對了🎉</p>
            <p v-else class="text-rose-600 text-sm m-0">
              正確答案是
              <span class="font-display text-xl text-paper-fg">{{ currentQuestion.word.term }}</span>
            </p>
          </div>
        </div>

        <div class="flex items-center justify-center gap-3">
          <button
            type="button"
            class="flex-1 max-w-40 rounded-full border border-paper-fg/20 px-6 py-3.5 text-center text-[15px] text-paper-fg transition-colors"
            :class="resultMode ? 'opacity-40 cursor-not-allowed' : 'cursor-pointer hover:bg-paper-fg/5'"
            :disabled="!!resultMode"
            @click="skipQuestion"
          >
            跳過
          </button>
          <UButton
            v-if="resultMode === 'correct' || resultMode === 'skipped'"
            :label="isLastQuestion ? '查看結果' : '下一題'"
            size="xl"
            class="flex-[2] max-w-64 justify-center bg-paper-primary text-paper-bg hover:bg-paper-accent"
            @click="nextQuestion"
          >
            <template v-if="resultMode === 'correct'" #trailing>
              <span :key="autoAdvanceCountdown" class="countdown-badge">{{ autoAdvanceCountdown }}</span>
            </template>
          </UButton>
        </div>
      </div>

      <div v-else class="max-w-3xl mx-auto px-6 pt-16 text-center">
        <h1 class="font-display font-normal text-4xl mb-2 text-paper-fg">練習結束</h1>
        <p class="text-paper-muted mb-10">
          你答對了
          <span class="text-paper-accent">{{ score }}</span>
          / {{ questions.length }} 題
        </p>

        <div v-if="missedWords.length" class="text-left mb-12">
          <h2 class="font-display font-normal text-xl mb-4 text-paper-fg">這幾個單字再複習一下</h2>
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <article
              v-for="w in missedWords"
              :key="w.id"
              class="rounded-2xl p-5 bg-paper-bg-alt border border-paper-fg/10"
            >
              <div class="flex items-baseline justify-between mb-1.5">
                <h3 class="font-display font-normal text-lg m-0 text-paper-fg">{{ w.term }}</h3>
                <span class="text-xs text-paper-accent shrink-0">{{ w.partOfSpeech }}</span>
              </div>
              <p class="text-sm text-paper-muted m-0">{{ w.definitionCN }}</p>
            </article>
          </div>
        </div>

        <div class="flex justify-center gap-4">
          <UButton
            label="再練習一次"
            class="bg-paper-primary text-paper-bg hover:bg-paper-accent"
            @click="restartQuiz"
          />
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
  </div>
</template>
