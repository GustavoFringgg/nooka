<script setup lang="ts">
import type { Category, Word } from "~/types/practice"
import type { QuizDirection } from "~/utils/quiz"
import { gsap } from "gsap"
const router = useRouter()

// ========== 共用工具 ==========
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

const posColor = (pos: string) => {
  return posColors[pos as PartOfSpeech]
}

const darken = (hex: string, amount = 0.25) => {
  const num = parseInt(hex.replace("#", ""), 16)
  const r = Math.round(((num >> 16) & 255) * (1 - amount))
  const g = Math.round(((num >> 8) & 255) * (1 - amount))
  const b = Math.round((num & 255) * (1 - amount))
  return `rgb(${r}, ${g}, ${b})`
}

// ========== 書架/選書 ==========
const selectedId = ref<number | null>(null)
const selectedBook = computed(() => categories.value?.find((c) => c.id === selectedId.value) ?? null)

const { data: categories, pending: categoriesPending } = await useFetch<Category[]>(useApiUrl("/api/categories"))

watch(
  categories,
  (val) => {
    if (val?.length && selectedId.value === null) selectedId.value = val[0]!.id
  },
  { immediate: true } // immediate 設定 watch 當下先跑一次
)
const { data: words, pending: wordsPending } = await useFetch<Word[]>(
  () => useApiUrl(`/api/words/category/${selectedId.value}`),
  { watch: [selectedId] }
)

const selectBook = (id: number) => {
  selectedId.value = id
}

// ========== 練習模式切換 ==========
type PracticeMode = "flashcard" | "choice" | "typing"
const selectedMode = ref<PracticeMode | null>(null)

const selectMode = (mode: PracticeMode) => {
  selectedMode.value = mode
  if (mode === "choice") openChoiceModal()
  if (mode === "typing") openTypingModal()
  if (mode === "flashcard") openFlashcardModal()
}

// ========== 共用:題數設定(選擇題、打字拼寫都用得到) ==========
const questionCount = ref(1)
const minCount = ref(5) //computed(() => Math.min(5, selectedBook.value?.count ?? 5))
const maxCount = computed(() => words.value?.length ?? 5)

// ========== 選擇題 Modal ==========
const isChoiceModalOpen = ref(false)
const direction = ref<QuizDirection>("enToCn")

const directionOptions: { label: string; value: QuizDirection }[] = [
  { label: "看英文,選中文答案", value: "enToCn" },
  { label: "看中文,選英文答案", value: "cnToEn" }
]

const openChoiceModal = () => {
  if (!selectedBook.value) return
  questionCount.value = maxCount.value
  isChoiceModalOpen.value = true
}

const startChoiceQuiz = () => {
  if (!selectedBook.value) return
  isChoiceModalOpen.value = false
  const targetId = selectedBook.value.id
  router.push(`/practice/${targetId}/choice?direction=${direction.value}&count=${questionCount.value}`)
}

// ========== 打字拼寫 Modal ==========
const isTypingModalOpen = ref(false)

const openTypingModal = () => {
  if (!selectedBook.value) return
  questionCount.value = maxCount.value
  isTypingModalOpen.value = true
}

const startTypingQuiz = () => {
  if (!selectedBook.value) return
  isTypingModalOpen.value = false
  const targetId = selectedBook.value.id
  router.push(`/practice/${targetId}/typing?count=${questionCount.value}`)
}

// ========== 單字卡 Modal ==========
const isFlashcardModalOpen = ref(false)
const flashcardStep = ref<"choose" | "intro">("choose")
const flashcardModeChoice = ref<"new" | "review" | null>(null)

const flashcardProgress = computed(() => {
  return selectedId.value !== null ? useFlashcardProgress(selectedId.value) : null
})

const { data: flashcardCounts } = await useAsyncData(
  "flashcard-counts", // key:唯一的名字，Nuxt 用它做 SSR/CSR 之間的快取比對
  async () => {
    // 執行非同步
    if (!selectedBook.value || !words.value || !flashcardProgress.value) return { newCount: 0, dueCount: 0 }
    await flashcardProgress.value.loadProgressList()
    return flashcardProgress.value.getCounts(words.value)
  },
  { watch: [selectedId, words], default: () => ({ newCount: 0, dueCount: 0 }) }
)

watch(isFlashcardModalOpen, (open) => {
  if (!open) flashcardStep.value = "choose"
})

const flashcardModalUi = computed(() => ({
  content: `bg-paper-bg text-paper-fg ring-paper-fg/10 divide-paper-fg/10 ${flashcardStep.value === "intro" ? "sm:max-w-xl" : ""}`,
  header: "border-paper-fg/10",
  footer: "border-paper-fg/10",
  title: "text-paper-fg font-display text-2xl font-normal",
  description: "text-paper-muted",
  close: "text-paper-muted hover:bg-paper-fg/10 hover:text-paper-fg",
  overlay: "bg-paper-fg/40"
}))

const introDemoRef = ref<HTMLElement | null>(null)
const introFlipped = ref(false)
const introChoicesRevealed = ref(false)

const prefersReducedMotion = () => window.matchMedia("(prefers-reduced-motion: reduce)").matches

const flipIntroDemo = () => {
  const root = introDemoRef.value
  const card = root?.querySelector<HTMLElement>(".intro-demo-card")
  if (!root || !card) return

  introFlipped.value = !introFlipped.value
  const rotateY = introFlipped.value ? 180 : 0
  if (prefersReducedMotion()) {
    gsap.set(card, { rotateY })
  } else {
    gsap.to(card, { rotateY, duration: 0.6, ease: "power2.inOut" })
  }

  if (introFlipped.value && !introChoicesRevealed.value) {
    introChoicesRevealed.value = true
    const choices = root.querySelectorAll<HTMLElement>(".intro-choice")
    if (prefersReducedMotion()) {
      gsap.set(choices, { opacity: 1, y: 0 })
    } else {
      gsap.fromTo(choices, { opacity: 0, y: 10 }, { opacity: 1, y: 0, duration: 0.35, stagger: 0.12, ease: "power2.out" })
    }
  }
}

watch(flashcardStep, (step) => {
  if (step !== "intro") return
  introFlipped.value = false
  introChoicesRevealed.value = false
  nextTick(() => {
    const root = introDemoRef.value
    if (!root) return
    gsap.set(root.querySelector(".intro-demo-card"), { rotateY: 0 })
    gsap.set(root.querySelectorAll(".intro-choice"), { opacity: 0, y: 10 })
  })
})

const openFlashcardModal = () => {
  if (!selectedBook.value) return
  flashcardStep.value = "choose"
  isFlashcardModalOpen.value = true
}

const chooseFlashcardMode = (mode: "new" | "review") => {
  if (!selectedBook.value || !flashcardProgress.value) return
  flashcardModeChoice.value = mode

  if (flashcardProgress.value.isFirstTimeForBook()) {
    flashcardStep.value = "intro"
  } else {
    goToFlashcard()
  }
}

const backToFlashcardChoose = () => {
  flashcardStep.value = "choose"
}

const goToFlashcard = () => {
  if (!selectedBook.value || !flashcardModeChoice.value) return
  isFlashcardModalOpen.value = false
  const targetId = selectedBook.value.id
  router.push(`/practice/${targetId}/flashcard?mode=${flashcardModeChoice.value}`)
}
</script>

<template>
  <div class="h-screen overflow-hidden flex flex-col bg-paper-bg font-body text-paper-fg" style="color-scheme: light">
    <AppNav />

    <div class="flex-1 min-h-0 bg-paper-bg-alt flex flex-col">
      <div class="max-w-[1440px] mx-auto px-12 w-full flex-1 min-h-0 flex flex-col">
        <div class="max-w-[1040px] mx-auto pt-8 w-full shrink-0">
          <div class="mb-6 flex flex-wrap gap-4">
            <button
              type="button"
              class="flex-1 min-w-40 rounded-full px-6 py-5 text-center text-base font-semibold transition-transform duration-250 ease-out cursor-pointer hover:-translate-y-0.5"
              :class="
                selectedMode === 'flashcard'
                  ? 'bg-paper-primary text-paper-bg'
                  : 'bg-paper-fg/8 text-paper-muted/70 hover:bg-paper-primary hover:text-paper-bg'
              "
              @click="selectMode('flashcard')"
            >
              單字卡
            </button>
            <button
              type="button"
              class="flex-1 min-w-40 rounded-full px-6 py-5 text-center text-base font-semibold transition-transform duration-250 ease-out"
              :class="
                selectedMode === 'choice'
                  ? 'bg-paper-primary text-paper-bg cursor-pointer hover:-translate-y-0.5'
                  : 'bg-paper-fg/8 text-paper-muted/70 cursor-pointer hover:-translate-y-0.5 hover:bg-paper-primary hover:text-paper-bg'
              "
              :disabled="!selectedBook"
              @click="selectMode('choice')"
            >
              選擇題
            </button>
            <button
              type="button"
              class="flex-1 min-w-40 rounded-full px-6 py-5 text-center text-base font-semibold transition-transform duration-250 ease-out cursor-pointer hover:-translate-y-0.5"
              :class="
                selectedMode === 'typing'
                  ? 'bg-paper-primary text-paper-bg'
                  : 'bg-paper-fg/8 text-paper-muted/70 hover:bg-paper-primary hover:text-paper-bg'
              "
              @click="selectMode('typing')"
            >
              打字拼寫
            </button>
          </div>
        </div>

        <div class="flex-1 min-h-0 pb-8 flex flex-col lg:flex-row gap-12">
          <div
            class="styled-scrollbar lg:flex-[1.15] h-full min-h-0 grid grid-cols-2 gap-6 box-border overflow-y-auto pt-7 pr-1"
          >
            <div v-for="book in categories" :key="book.id" class="cursor-pointer" @click="selectBook(book.id)">
              <div
                class="flex rounded-l-[3px] rounded-r-[10px] transition-transform duration-250 ease-out hover:-translate-y-2.5 hover:-rotate-[1.5deg] hover:shadow-[0_26px_34px_-18px_rgba(43,42,37,0.4)]"
                :class="
                  selectedId === book.id
                    ? '-translate-y-2.5 -rotate-[1.5deg] shadow-[0_26px_34px_-18px_rgba(43,42,37,0.4)]'
                    : 'shadow-[0_10px_18px_-14px_rgba(43,42,37,0.3)]'
                "
              >
                <div class="w-3.5 rounded-l-[3px]" :style="{ background: darken(book.color ?? '#8a7a3e') }" />
                <div
                  class="relative flex-1 flex flex-col justify-between px-5 py-5.5 min-h-[190px] shadow-[inset_-10px_0_14px_-12px_rgba(0,0,0,0.35)]"
                  :style="{ background: book.color ?? '#8a7a3e' }"
                >
                  <div
                    class="absolute right-0 top-0.5 bottom-0.5 w-1.5 rounded-r-md"
                    style="
                      background: repeating-linear-gradient(
                        180deg,
                        rgba(255, 255, 255, 0.5) 0px,
                        rgba(255, 255, 255, 0.5) 1px,
                        transparent 1px,
                        transparent 3px
                      );
                    "
                  />
                  <span
                    v-if="selectedId === book.id"
                    class="absolute -top-0.5 right-4.5 w-6.5 h-9.5 bg-paper-primary shadow-[0_4px_8px_rgba(43,42,37,0.3)] z-10"
                    style="clip-path: polygon(0 0, 100% 0, 100% 100%, 50% 78%, 0 100%)"
                  />
                  <span class="text-[11px] tracking-[0.1em] text-white/75 uppercase">單字書</span>
                  <div>
                    <div
                      class="font-display text-[26px] text-white leading-tight"
                      style="text-shadow: 0 1px 2px rgba(0, 0, 0, 0.15)"
                    >
                      {{ book.name }}
                    </div>
                    <!-- <div class="text-white/85 text-[13px] mt-1.5">{{ book.count }} 個單字</div> -->
                    <!-- TODO: 之後透過 api 獲取真正書量 -->
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div
            class="lg:flex-[0.85] h-full min-h-0 bg-paper-bg rounded-[20px] p-7 shadow-[0_20px_40px_-26px_rgba(43,42,37,0.3)] box-border flex flex-col"
          >
            <div v-if="selectedBook" class="flex items-center gap-3 mb-1.5">
              <div class="w-3.5 h-3.5 rounded-[4px]" :style="{ background: selectedBook.color ?? '#8a7a3e' }" />
              <span class="font-display text-[26px] text-paper-fg">{{ selectedBook.name }}</span>
            </div>
            <!-- <p v-if="selectedBook" class="mb-5 text-paper-muted text-[13.5px]">{{ selectedBook.count }} 個單字</p> -->
            <!-- TODO: 之後改用api  words.value.length 來補資料 -->
            <div
              class="styled-scrollbar grid grid-cols-2 gap-2 flex-1 min-h-0 overflow-y-auto overflow-x-hidden pr-1.5 content-start"
            >
              <div v-for="w in words" :key="w.id" class="py-3 px-1 border-b border-paper-fg/12 box-border">
                <div class="flex items-baseline justify-between gap-2">
                  <span class="font-display text-[17px] text-paper-fg">{{ w.term }}</span>
                  <span class="text-paper-muted text-[12.5px] text-right">{{ w.definitionCN }}</span>
                </div>
                <div class="flex items-center justify-between gap-2 mt-1">
                  <div class="flex items-center gap-1.5">
                    <span
                      class="text-[#9c9384] text-[11.5px]"
                      style="font-family: ui-monospace, &quot;SF Mono&quot;, monospace"
                    >
                      /{{ w.ipa ?? "" }}/
                    </span>
                    <span
                      class="w-6.5 h-6.5 rounded-full flex items-center justify-center text-[#9c9384] cursor-pointer transition-colors duration-150 hover:bg-paper-fg/8 hover:text-paper-primary"
                    >
                      <svg width="15" height="15" viewBox="0 0 24 24" fill="currentColor">
                        <path d="M3 9v6h4l5 5V4L7 9H3z" />
                        <path d="M16.5 12c0-1.77-.77-3.29-2-4.24v8.48c1.23-.95 2-2.47 2-4.24z" opacity=".7" />
                      </svg>
                    </span>
                  </div>
                  <span
                    class="text-[11px] px-1.5 py-0.5 rounded-full shrink-0"
                    :style="{ color: posColor(w.partOfSpeech).text, background: posColor(w.partOfSpeech).bg }"
                  >
                    {{ w.partOfSpeech }}
                  </span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <UModal
      v-model:open="isChoiceModalOpen"
      title="練習方向"
      description="選擇練習方向與題數,設定好就可以開始測驗"
      :ui="{
        content: 'bg-paper-bg text-paper-fg ring-paper-fg/10 divide-paper-fg/10',
        header: 'border-paper-fg/10',
        footer: 'border-paper-fg/10',
        title: 'text-paper-fg font-display text-2xl font-normal',
        description: 'text-paper-muted',
        close: 'text-paper-muted hover:bg-paper-fg/10 hover:text-paper-fg',
        overlay: 'bg-paper-fg/40'
      }"
    >
      <template #body>
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-7">
          <div class="flex flex-col gap-2.5">
            <button
              v-for="opt in directionOptions"
              :key="opt.value"
              type="button"
              class="flex items-center gap-3 rounded-2xl border px-4.5 py-3.5 text-left cursor-pointer transition-colors"
              :class="direction === opt.value ? 'border-paper-primary bg-paper-primary/8' : 'border-paper-fg/15'"
              @click="direction = opt.value"
            >
              <span
                class="flex w-4.5 h-4.5 shrink-0 items-center justify-center rounded-full border-2 border-paper-primary"
              >
                <span
                  class="w-2 h-2 rounded-full"
                  :class="direction === opt.value ? 'bg-paper-primary' : 'bg-transparent'"
                />
              </span>
              <span class="text-paper-fg text-[15px]">{{ opt.label }}</span>
            </button>
          </div>

          <div class="bg-paper-bg-alt rounded-2xl p-5 min-h-[112px] flex items-center justify-center gap-2.5">
            <div class="flex flex-col items-center gap-1.5 preview-pulse">
              <div
                class="flex w-[74px] h-14 items-center justify-center rounded-[10px] border-[1.5px] border-paper-primary bg-white font-display text-[15px] text-paper-fg"
              >
                {{ direction === "enToCn" ? "word" : "中文" }}
              </div>
              <span class="text-[10px] text-paper-muted">{{ direction === "enToCn" ? "EN" : "中" }}</span>
            </div>
            <div class="text-paper-accent text-xl preview-arrow">→</div>
            <div class="flex flex-col gap-1.5">
              <div class="w-15 h-4 rounded-md bg-paper-fg/10" />
              <div class="w-15 h-4 rounded-md bg-paper-primary" />
              <div class="w-15 h-4 rounded-md bg-paper-fg/10" />
            </div>
          </div>
        </div>

        <div class="mt-7 pt-6 border-t border-paper-fg/15">
          <div class="flex items-center justify-between">
            <span class="text-paper-fg text-[15px] font-medium">題目數量</span>
            <span class="text-paper-accent font-display text-[22px]">{{ questionCount }}</span>
          </div>
          <USlider
            v-model="questionCount"
            :min="minCount"
            :max="maxCount"
            :step="1"
            class="mt-3.5"
            :ui="{
              track: 'bg-paper-fg/12',
              range: 'bg-paper-primary',
              thumb: 'bg-white border-2 border-paper-primary ring-0 focus-visible:outline-paper-primary'
            }"
          />
          <div class="flex justify-between text-[#9c9384] text-xs mt-1">
            <span>{{ minCount }}</span>
            <span>{{ maxCount }}</span>
          </div>
        </div>
      </template>

      <template #footer>
        <div class="flex gap-3 w-full">
          <UButton
            label="取消"
            color="neutral"
            variant="outline"
            class="flex-1 justify-center bg-transparent border-paper-fg/25 text-paper-fg hover:bg-paper-fg/5"
            @click="isChoiceModalOpen = false"
          />
          <UButton
            label="開始練習"
            class="flex-[2] justify-center bg-paper-primary text-paper-bg hover:bg-paper-accent"
            @click="startChoiceQuiz"
          />
        </div>
      </template>
    </UModal>

    <UModal
      v-model:open="isTypingModalOpen"
      title="練習題數"
      description="選擇題數,設定好就可以開始測驗"
      :ui="{
        content: 'bg-paper-bg text-paper-fg ring-paper-fg/10 divide-paper-fg/10',
        header: 'border-paper-fg/10',
        footer: 'border-paper-fg/10',
        title: 'text-paper-fg font-display text-2xl font-normal',
        description: 'text-paper-muted',
        close: 'text-paper-muted hover:bg-paper-fg/10 hover:text-paper-fg',
        overlay: 'bg-paper-fg/40'
      }"
    >
      <template #body>
        <div class="mt-7 pt-6">
          <div class="flex items-center justify-between">
            <span class="text-paper-fg text-[15px] font-medium">題目數量</span>
            <span class="text-paper-accent font-display text-[22px]">{{ questionCount }}</span>
          </div>
          <USlider
            v-model="questionCount"
            :min="minCount"
            :max="maxCount"
            :step="1"
            class="mt-3.5"
            :ui="{
              track: 'bg-paper-fg/12',
              range: 'bg-paper-primary',
              thumb: 'bg-white border-2 border-paper-primary ring-0 focus-visible:outline-paper-primary'
            }"
          />
          <div class="flex justify-between text-[#9c9384] text-xs mt-1">
            <span>{{ minCount }}</span>
            <span>{{ maxCount }}</span>
          </div>
        </div>
      </template>

      <template #footer>
        <div class="flex gap-3 w-full">
          <UButton
            label="取消"
            color="neutral"
            variant="outline"
            class="flex-1 justify-center bg-transparent border-paper-fg/25 text-paper-fg hover:bg-paper-fg/5"
            @click="isTypingModalOpen = false"
          />
          <UButton
            label="開始練習"
            class="flex-[2] justify-center bg-paper-primary text-paper-bg hover:bg-paper-accent"
            @click="startTypingQuiz"
          />
        </div>
      </template>
    </UModal>

    <UModal
      v-model:open="isFlashcardModalOpen"
      :title="flashcardStep === 'choose' ? '今天想怎麼練？' : '開始之前'"
      :description="
        flashcardStep === 'choose' ? '選學新字,或複習已經標記過的單字' : '第一次玩這本書的單字卡,先看一下規則'
      "
      :ui="flashcardModalUi"
    >
      <template #body>
        <div v-if="flashcardStep === 'choose'" class="grid grid-cols-1 sm:grid-cols-2 gap-5">
          <button
            type="button"
            class="text-left rounded-2xl border-2 p-5 transition-colors"
            :class="
              flashcardCounts.newCount > 0
                ? 'border-paper-primary/40 hover:bg-paper-primary/8 cursor-pointer'
                : 'border-paper-fg/10 opacity-60 cursor-not-allowed'
            "
            :disabled="flashcardCounts.newCount === 0"
            @click="chooseFlashcardMode('new')"
          >
            <div class="font-display text-xl text-paper-fg mb-1">學習新單字</div>
            <p class="text-paper-muted text-sm m-0">
              <template v-if="flashcardCounts.newCount > 0">
                還有
                <span class="text-red-700">{{ flashcardCounts.newCount }}</span>
                個字沒學過
              </template>
              <template v-else>這本書都學過了</template>
            </p>
          </button>

          <button
            v-if="flashcardCounts.dueCount > 0"
            type="button"
            class="text-left rounded-2xl border-2 border-paper-fg/15 p-5 cursor-pointer transition-colors hover:bg-paper-fg/5"
            @click="chooseFlashcardMode('review')"
          >
            <div class="font-display text-xl text-paper-fg mb-1">複習已學過的單字</div>
            <p class="text-paper-muted text-sm m-0">
              今天有
              <span class="text-paper-accent font-medium text-red-500">{{ flashcardCounts.dueCount }}</span>
              張待複習
            </p>
          </button>
          <div v-else class="text-left rounded-2xl border-2 border-paper-fg/10 p-5 opacity-70">
            <div class="font-display text-xl text-paper-muted mb-1">複習單字</div>
            <p class="text-paper-muted text-sm m-0">今天的複習都完成了 🎉</p>
            <p class="text-paper-muted text-xs mt-2 mb-0">明天會有新的複習排程</p>
            <p class="text-paper-muted text-xs mt-2 mb-0">現在可以先點左邊「學習新單字」</p>
          </div>
        </div>

        <div v-else ref="introDemoRef" class="flex flex-col items-center gap-6 py-2">
          <p class="text-paper-fg text-[15px] leading-relaxed text-center max-w-sm">
            每張卡片點一下會翻面看意思,看完誠實選你有多熟悉這個字
          </p>

          <div style="perspective: 1200px; cursor: pointer" @click="flipIntroDemo">
            <div class="intro-demo-card" style="position: relative; width: 220px; height: 160px; transform-style: preserve-3d">
              <div
                class="rounded-2xl bg-paper-bg-alt border-2 border-paper-fg/25 shadow-[0_14px_30px_-18px_rgba(43,42,37,0.3)] flex flex-col items-center justify-center gap-1.5"
                style="position: absolute; inset: 0; backface-visibility: hidden"
              >
                <div class="font-display text-2xl text-paper-fg">nostalgia</div>
                <div class="text-paper-muted text-xs" style="font-family: ui-monospace, &quot;SF Mono&quot;, monospace">
                  /nɒˈstældʒə/
                </div>
                <p class="text-[11px] text-paper-muted mt-1 m-0">點卡片看意思</p>
              </div>
              <div
                class="rounded-2xl bg-paper-primary/10 flex flex-col items-center justify-center gap-1 px-4 text-center"
                style="position: absolute; inset: 0; backface-visibility: hidden; transform: rotateY(180deg)"
              >
                <div class="font-display text-xl text-paper-primary">懷舊之情</div>
                <div class="text-xs text-paper-fg">a wistful longing for the past</div>
              </div>
            </div>
          </div>

          <div class="grid grid-cols-3 gap-2 w-full max-w-sm">
            <div class="intro-choice rounded-xl border-1.5 border-paper-fg/20 text-paper-fg text-[11.5px] py-2.5 text-center">
              不認識
            </div>
            <div
              class="intro-choice rounded-xl border-1.5 border-paper-primary/50 text-paper-primary text-[11.5px] py-2.5 text-center"
            >
              認識但不熟
            </div>
            <div class="intro-choice rounded-xl bg-paper-primary text-paper-bg text-[11.5px] py-2.5 text-center">
              非常熟悉
            </div>
          </div>

          <p class="text-paper-muted text-xs text-center max-w-sm">
            之後複習只要點「今天已複習」,系統會自動安排下次什麼時候再看到這張卡
          </p>
        </div>
      </template>

      <template #footer>
        <div v-if="flashcardStep === 'choose'" class="flex gap-3 w-full">
          <UButton
            label="取消"
            color="neutral"
            variant="outline"
            class="flex-1 justify-center bg-transparent border-paper-fg/25 text-paper-fg hover:bg-paper-fg/5"
            @click="isFlashcardModalOpen = false"
          />
        </div>
        <div v-else class="flex gap-3 w-full">
          <UButton
            label="返回"
            color="neutral"
            variant="outline"
            class="flex-1 justify-center bg-transparent border-paper-fg/25 text-paper-fg hover:bg-paper-fg/5"
            @click="backToFlashcardChoose"
          />
          <button
            type="button"
            class="flex-[2] inline-flex items-center justify-center rounded-md px-4 py-2.5 text-sm font-medium bg-paper-primary text-paper-bg hover:bg-paper-accent cursor-pointer transition-colors"
            @click="goToFlashcard"
          >
            立即開始
          </button>
        </div>
      </template>
    </UModal>
  </div>
</template>

<style scoped>
@keyframes preview-pulse {
  0%,
  100% {
    transform: translateY(0);
  }
  50% {
    transform: translateY(-6px);
  }
}

@keyframes preview-arrow {
  0%,
  100% {
    opacity: 0.35;
    transform: translateX(0);
  }
  50% {
    opacity: 1;
    transform: translateX(6px);
  }
}

.preview-pulse {
  animation: preview-pulse 1.8s ease-in-out infinite;
}

.preview-arrow {
  animation: preview-arrow 1.8s ease-in-out infinite;
}

.styled-scrollbar {
  scrollbar-width: thin;
  scrollbar-color: rgba(43, 42, 37, 0.25) transparent;
}

.styled-scrollbar::-webkit-scrollbar {
  width: 8px;
}

.styled-scrollbar::-webkit-scrollbar-track {
  background: transparent;
}

.styled-scrollbar::-webkit-scrollbar-thumb {
  background-color: rgba(43, 42, 37, 0.25);
  border-radius: 999px;
}

.styled-scrollbar::-webkit-scrollbar-thumb:hover {
  background-color: rgba(43, 42, 37, 0.4);
}
</style>
