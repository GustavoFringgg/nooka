<script setup lang="ts">
// TODO: 目前書架 / 單字預覽全部是 mock 資料(色票、KK 音標後端都還沒有欄位),
// 之後補上 Category 顏色欄位 + Word 音標欄位後,換成真的 GET /api/categories、GET /api/words/category/{id}
import type { Category } from "~/types/practice"
import type { QuizDirection } from "~/utils/quiz"
const { data: categories, pending } = await useFetch<Category[]>(useApiUrl("/api/categories"))

const selectedId = ref<number | null>(null)
const selectedBook = computed(() => categories.value?.find((c) => c.id === selectedId.value) ?? null)

watch(
  categories,
  (val) => {
    if (val?.length && selectedId.value === null) selectedId.value = val[0]!.id
  },
  { immediate: true } // immediate 設定 watch 當下先跑一次
)

function darken(hex: string, amount = 0.25) {
  const num = parseInt(hex.replace("#", ""), 16)
  const r = Math.round(((num >> 16) & 255) * (1 - amount))
  const g = Math.round(((num >> 8) & 255) * (1 - amount))
  const b = Math.round((num & 255) * (1 - amount))
  return `rgb(${r}, ${g}, ${b})`
}
// interface MockBook {
//   id: string
//   categoryId?: number // 對應真實後端 categoryId,mock 書本沒有就先留空
//   name: string
//   color: string
//   spineColor: string
//   count: number
// }

// interface MockWord {
//   en: string
//   zh: string
//   pos: "形容詞" | "副詞" | "動詞" | "名詞"
//   ipa: string
// }

// const posColors: Record<MockWord["pos"], { bg: string; text: string }> = {
//   形容詞: { bg: "rgba(193,101,59,.16)", text: "#8f4128" },
//   副詞: { bg: "rgba(60,90,68,.16)", text: "#2c433a" },
//   動詞: { bg: "rgba(156,74,60,.16)", text: "#78392e" },
//   名詞: { bg: "rgba(62,110,142,.16)", text: "#2d5670" }
// }

// const books: MockBook[] = [
//   { id: "toeic", categoryId: 1, name: "多益(600)", color: "#b08434", spineColor: "#8a6425", count: 6 },
//   { id: "security", name: "資安用", color: "#9c4a3c", spineColor: "#78392e", count: 40 },
//   { id: "aviation", name: "航空用", color: "#3e6e8e", spineColor: "#2d5670", count: 35 },
//   { id: "biz", name: "商務", color: "#6b4c6e", spineColor: "#503a52", count: 28 },
//   { id: "eng", name: "工程", color: "#6e7a3e", spineColor: "#535c2f", count: 32 },
//   { id: "travel", name: "旅遊", color: "#2f6e6b", spineColor: "#235350", count: 20 },
//   { id: "medical", name: "醫療用", color: "#a13f5e", spineColor: "#7a2f47", count: 30 },
//   { id: "finance", name: "金融", color: "#4a5a8a", spineColor: "#37436a", count: 26 },
//   { id: "daily", name: "生活會話", color: "#8a7a3e", spineColor: "#6a5e2f", count: 24 }
// ]

// const demoWords: MockWord[] = [
//   { en: "vicarious", zh: "替代的感受", pos: "形容詞", ipa: "vaɪˈkɛrɪəs" },
//   { en: "haltingly", zh: "吞吞吐吐地", pos: "副詞", ipa: "ˈhɔltɪŋli" },
//   { en: "hectic", zh: "忙亂的", pos: "形容詞", ipa: "ˈhɛktɪk" },
//   { en: "hub", zh: "中心樞紐", pos: "名詞", ipa: "hʌb" },
//   { en: "hysterical", zh: "歇斯底里的", pos: "形容詞", ipa: "hɪˈstɛrɪkl" },
//   { en: "handout", zh: "講義;施捨", pos: "名詞", ipa: "ˈhændaʊt" },
//   { en: "ambiguous", zh: "模稜兩可的", pos: "形容詞", ipa: "æmˈbɪgjuəs" },
//   { en: "candid", zh: "坦率的", pos: "形容詞", ipa: "ˈkændɪd" },
//   { en: "diligent", zh: "勤奮的", pos: "形容詞", ipa: "ˈdɪlədʒənt" },
//   { en: "eloquent", zh: "雄辯的", pos: "形容詞", ipa: "ˈɛləkwənt" },
//   { en: "frugal", zh: "節儉的", pos: "形容詞", ipa: "ˈfrugl̩" },
//   { en: "gregarious", zh: "善於社交的", pos: "形容詞", ipa: "grɪˈgɛriəs" },
//   { en: "hinder", zh: "阻礙", pos: "動詞", ipa: "ˈhɪndɚ" },
//   { en: "intricate", zh: "複雜精細的", pos: "形容詞", ipa: "ˈɪntrɪkɪt" },
//   { en: "jubilant", zh: "歡欣鼓舞的", pos: "形容詞", ipa: "ˈdʒublənt" },
//   { en: "keen", zh: "敏銳的;渴望的", pos: "形容詞", ipa: "kin" },
//   { en: "lucid", zh: "清晰易懂的", pos: "形容詞", ipa: "ˈlusɪd" },
//   { en: "meticulous", zh: "一絲不苟的", pos: "形容詞", ipa: "məˈtɪkjələs" },
//   { en: "novice", zh: "新手", pos: "名詞", ipa: "ˈnɑvɪs" },
//   { en: "obsolete", zh: "過時的", pos: "形容詞", ipa: "ˌɑbsəˈlit" }
// ]

const router = useRouter()

const previewWords = computed(() => [])
const isChoiceModalOpen = ref(false)
const direction = ref<QuizDirection>("enToCn")
const questionCount = ref(1)

const directionOptions: { label: string; value: QuizDirection }[] = [
  { label: "看英文,選中文答案", value: "enToCn" },
  { label: "看中文,選英文答案", value: "cnToEn" }
]

const minCount = ref(1) //computed(() => Math.min(5, selectedBook.value?.count ?? 5))
const maxCount = ref(20) //computed(() => selectedBook.value?.count ?? 30)

function openChoiceModal() {
  if (!selectedBook.value) return
  questionCount.value = maxCount.value
  isChoiceModalOpen.value = true
}

function selectBook(id: number) {
  selectedId.value = id
}

function startChoiceQuiz() {
  if (!selectedBook.value) return
  isChoiceModalOpen.value = false
  const targetId = selectedBook.value.id
  router.push(`/practice/${targetId}/choice?direction=${direction.value}&count=${questionCount.value}`)
}
</script>

<template>
  <div class="h-screen overflow-hidden flex flex-col bg-paper-bg font-body text-paper-fg" style="color-scheme: light">
    <AppNav />

    <div class="flex-1 min-h-0 bg-paper-bg-alt flex flex-col">
      <div class="max-w-[1440px] mx-auto px-12 w-full flex-1 min-h-0 flex flex-col">
        <div class="max-w-[1040px] mx-auto pt-8 w-full shrink-0">
          <div class="mb-6 flex flex-wrap gap-4">
            <div
              class="flex-1 min-w-40 rounded-full bg-paper-fg/8 px-6 py-5 text-center text-base font-semibold text-paper-muted/70 cursor-not-allowed"
            >
              單字卡
              <span class="text-xs">· 即將推出</span>
            </div>
            <button
              type="button"
              class="flex-1 min-w-40 rounded-full px-6 py-5 text-center text-base font-semibold transition-transform duration-250 ease-out"
              :class="
                selectedBook
                  ? 'bg-paper-primary text-paper-bg cursor-pointer hover:-translate-y-0.5'
                  : 'bg-paper-fg/15 text-paper-muted/70 cursor-not-allowed'
              "
              :disabled="!selectedBook"
              @click="openChoiceModal"
            >
              選擇題
            </button>
            <div
              class="flex-1 min-w-40 rounded-full bg-paper-fg/8 px-6 py-5 text-center text-base font-semibold text-paper-muted/70 cursor-not-allowed"
            >
              打字拼寫
              <span class="text-xs">· 即將推出</span>
            </div>
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
              <div class="w-3.5 h-3.5 rounded-[4px]" :style="{ background: selectedBook.color }" />
              <span class="font-display text-[26px] text-paper-fg">{{ selectedBook.name }}</span>
            </div>
            <!-- <p v-if="selectedBook" class="mb-5 text-paper-muted text-[13.5px]">{{ selectedBook.count }} 個單字</p> -->
            <!-- TODO: 之後改用api  words.value.length 來補資料 -->
            <div
              class="styled-scrollbar grid grid-cols-2 gap-2 flex-1 min-h-0 overflow-y-auto overflow-x-hidden pr-1.5 content-start"
            >
              <div v-for="w in previewWords" :key="w.en" class="py-3 px-1 border-b border-paper-fg/12 box-border">
                <div class="flex items-baseline justify-between gap-2">
                  <span class="font-display text-[17px] text-paper-fg">{{ w.en }}</span>
                  <span class="text-paper-muted text-[12.5px] text-right">{{ w.zh }}</span>
                </div>
                <div class="flex items-center justify-between gap-2 mt-1">
                  <div class="flex items-center gap-1.5">
                    <span
                      class="text-[#9c9384] text-[11.5px]"
                      style="font-family: ui-monospace, &quot;SF Mono&quot;, monospace"
                    >
                      /{{ w.ipa }}/
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
                    :style="{ color: posColors[w.pos].text, background: posColors[w.pos].bg }"
                  >
                    {{ w.pos }}
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
