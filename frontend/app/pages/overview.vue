<script setup lang="ts">
import type { ActivitySummaryResponse, Category, Word, WeakestWordResponse } from "~/types/practice"
import dailyWordsData from "~/data/dailyWords.json"

interface DailyWordEntry {
  word: string
  meaning: string
  pos: string
  quoteEn: string
  quoteZh: string
}

useHead({
  link: [
    { rel: "preconnect", href: "https://fonts.googleapis.com" },
    {
      rel: "stylesheet",
      href: "https://fonts.googleapis.com/css2?family=Newreader:ital,wght@0,400;0,500;1,400;1,500&family=Work+Sans:wght@400;500;600&family=Caprasimo&family=Figtree:wght@400;600;700&display=swap"
    }
  ]
})

const { data: categories } = await useFetch<Category[]>(useApiUrl("/api/categories"))
const { data: words } = await useFetch<Word[]>(useApiUrl("/api/words"))

const isLoggedIn = useIsLoggedIn()
const router = useRouter()
const totalWordsDisplay = ref(0)

const { data: activitySummary } = await useAsyncData(
  "overview-activity-summary",
  async () => {
    if (!isLoggedIn.value) return { streak: { currentStreak: 0, longestStreak: 0 }, recent: [], heatmap: [] }
    return await useApiFetch<ActivitySummaryResponse>("/api/activity/summary")
  },
  {
    watch: [isLoggedIn],
    default: () => ({ streak: { currentStreak: 0, longestStreak: 0 }, recent: [], heatmap: [] }) as ActivitySummaryResponse
  }
)

const currentStreak = computed(() => activitySummary.value.streak.currentStreak)

const EF_INITIAL = 2.5
const EF_MIN = 1.3

const { data: weakestWords } = await useAsyncData(
  "overview-weakest-words",
  async () => {
    if (!isLoggedIn.value) return []
    return await useApiFetch<WeakestWordResponse[]>("/api/WordSm2/weakest?mode=Choice&count=5")
  },
  { watch: [isLoggedIn], default: () => [] as WeakestWordResponse[] }
)

const weakestWordBars = computed(() =>
  weakestWords.value.map((w) => ({
    ...w,
    pct: ((EF_INITIAL - w.ef) / (EF_INITIAL - EF_MIN)) * 100
  }))
)

onMounted(() => {
  const totalWordsTarget = words.value?.length ?? 0
  const duration = 1400
  const start = performance.now()
  const step = (now: number) => {
    const progress = Math.min(1, (now - start) / duration)
    const eased = 1 - Math.pow(1 - progress, 3)
    totalWordsDisplay.value = Math.round(eased * totalWordsTarget)
    if (progress < 1) requestAnimationFrame(step)
  }
  requestAnimationFrame(step)
})

const dailyWord = ref<DailyWordEntry | null>(null)

onMounted(() => {
  const list = dailyWordsData as DailyWordEntry[]
  dailyWord.value = list[Math.floor(Math.random() * list.length)] ?? null
})

// 未登入時的模糊預覽用假資料,不是真實數據
const weakWordsMockDataPreview = [
  { word: "haltingly", meaning: "吞吞吐吐地", pct: 90 },
  { word: "vicarious", meaning: "替代的感受", pct: 74 },
  { word: "hectic", meaning: "忙亂的", pct: 60 },
  { word: "hub", meaning: "中心樞紐", pct: 44 },
  { word: "handout", meaning: "講義;施捨", pct: 28 }
]
</script>

<template>
  <div class="min-h-screen bg-[#f5ead8] font-body text-paper-fg pb-24">
    <AppNav />

    <div class="overview-content max-w-6xl mx-auto px-6 md:px-12 text-[#201e1d]">
      <div class="mt-5 mb-7">
        <h1 class="font-heading-organic m-0 text-[clamp(32px,4vw,48px)] text-[#201e1d]">今天也背一點單字吧</h1>
      </div>

      <div class="bento-grid grid gap-4">
        <div class="area-words relative overflow-hidden rounded-[32px] p-6.5 bg-[#fff2eb] shadow-[0_3px_10px_rgba(46,43,37,.16)]">
          <div class="absolute -right-10 -top-10 w-[140px] h-[140px] rounded-full bg-[#ffe1d0] pointer-events-none" />
          <div class="relative">
            <div class="text-[10px] uppercase tracking-[.1em] text-[#c67139]">目前 Nooka 收錄了</div>
            <div class="font-heading-organic text-[64px] leading-none text-[#8c491a] mt-2">
              {{ totalWordsDisplay.toLocaleString() }}
            </div>
            <div class="text-[13px] text-[#201e1d]/80 mt-2">個在所有主題書架裡的單字</div>
          </div>
        </div>

        <div
          class="area-books rounded-[32px] p-6.5 flex flex-col items-center justify-center text-center bg-[#f0fae1] shadow-[0_1px_2px_rgba(46,43,37,.14)]"
        >
          <div class="w-16 h-16 rounded-full bg-[#8fa073] text-[#f5ead8] flex items-center justify-center font-heading-organic text-[28px] mb-3">
            {{ categories?.length ?? 0 }}
          </div>
          <div class="text-[10px] uppercase tracking-[.1em] text-[#c67139]">主題書架</div>
          <div class="text-[13px] text-[#201e1d]/80 mt-1">多益</div>
        </div>

        <div class="area-wordcard rounded-[32px] p-6.5 bg-[#ffcb8a] text-[#201e1d] shadow-[0_3px_10px_rgba(46,43,37,.16)]">
          <div class="text-[11px] uppercase tracking-[.1em] text-[#8c491a]">每日一字</div>
          <div class="font-heading-organic text-[38px] mt-2.5">{{ dailyWord?.word }}</div>
          <div class="text-[#201e1d]/80 text-[15px] mt-1.5">
            {{ dailyWord?.meaning }}
            <span class="opacity-60 text-[13px]">· {{ dailyWord?.pos }}</span>
          </div>
        </div>

        <div class="area-quote rounded-[32px] p-6.5 flex flex-col justify-center bg-[#ebddc5] shadow-[0_1px_2px_rgba(46,43,37,.14)]">
          <div class="text-[10px] uppercase tracking-[.1em] text-[#c67139]">每日一句</div>
          <p class="font-heading-organic text-[22px] leading-[1.4] mt-2.5 mb-0">"{{ dailyWord?.quoteEn }}"</p>
          <p class="text-[#645c50] text-sm mt-2.5">{{ dailyWord?.quoteZh }}</p>
        </div>

        <div
          v-if="!isLoggedIn"
          class="area-status rounded-[32px] p-6.5 flex flex-col items-center justify-center text-center bg-[#ebddc5] shadow-[0_1px_2px_rgba(46,43,37,.14)]"
        >
          <h3 class="font-heading-organic text-[22px] m-0">看看你的學習狀況</h3>
          <p class="text-[13px] text-[#201e1d]/80 mt-2 mb-4">登入後可看已學單字量與最容易忘記的字。</p>
          <button
            class="font-heading-organic px-4 py-2 rounded-full bg-[#c67139] text-[#f5ead8] text-sm cursor-pointer transition-colors duration-200 ease-out hover:bg-[#b2622d]"
            @click="router.push('/login')"
          >
            登入 / 註冊
          </button>
        </div>
        <div
          v-else
          class="area-status rounded-[32px] p-6.5 flex flex-col items-center justify-center text-center bg-[#3a1310] shadow-[0_3px_10px_rgba(46,43,37,.16)]"
        >
          <div class="text-[20px] uppercase tracking-[.1em] text-[#ffb27a]">連續學習</div>
          <div class="streak-flame relative mt-1.5">
            <span class="streak-flame-glyph" aria-hidden="true">🔥</span>
            <span class="font-heading-organic streak-flame-number">{{ currentStreak }}</span>
          </div>
          <div class="text-[20px] text-[#ffe1d0]/80 mt-1.5">天,保持下去</div>
        </div>

        <div
          v-if="!isLoggedIn"
          class="area-weak rounded-[32px] p-6.5 bg-[#ebddc5] shadow-[0_1px_2px_rgba(46,43,37,.14)] opacity-50 blur-[1.5px] pointer-events-none"
        >
          <div class="text-[10px] uppercase tracking-[.1em] text-[#c67139]">最容易忘記的 5 個字</div>
          <div class="flex flex-col gap-2.5 mt-3">
            <div v-for="w in weakWordsMockDataPreview" :key="w.word" class="flex items-center gap-3.5">
              <span class="font-heading-organic text-lg w-[110px]">{{ w.word }}</span>
              <span class="text-[#645c50] text-sm w-[90px]">{{ w.meaning }}</span>
              <div class="flex-1 h-1.5 rounded-full bg-[#eee7db]">
                <div class="h-full rounded-full bg-[#c67139]" :style="{ width: w.pct + '%' }" />
              </div>
            </div>
          </div>
        </div>
        <div v-else class="area-weak rounded-[32px] p-6.5 bg-[#ebddc5] shadow-[0_1px_2px_rgba(46,43,37,.14)]">
          <div class="text-[10px] uppercase tracking-[.1em] text-[#c67139]">最容易忘記的 5 個字</div>
          <div v-if="weakestWordBars.length === 0" class="text-[13px] text-[#201e1d]/60 mt-3">還沒有足夠的作答紀錄</div>
          <div v-else class="flex flex-col gap-2.5 mt-3">
            <div v-for="w in weakestWordBars" :key="w.wordId" class="flex items-center gap-3.5">
              <span class="font-heading-organic text-lg w-[110px]">{{ w.term }}</span>
              <span class="text-[#645c50] text-[13px] w-[90px]">{{ w.definitionCN }}</span>
              <div class="flex-1 h-1.5 rounded-full bg-[#eee7db]">
                <div class="h-full rounded-full bg-[#c67139]" :style="{ width: w.pct + '%' }" />
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.overview-content {
  font-family: "Figtree", system-ui, sans-serif;
}
.font-heading-organic {
  font-family: "Caprasimo", system-ui, sans-serif;
  font-weight: 400;
}

.bento-grid {
  grid-template-columns: 1.1fr 1fr 1fr;
  grid-template-areas:
    "words words books"
    "wordcard quote quote"
    "status weak weak";
}
.area-words {
  grid-area: words;
}
.area-books {
  grid-area: books;
}
.area-wordcard {
  grid-area: wordcard;
}
.area-quote {
  grid-area: quote;
}
.area-status {
  grid-area: status;
}
.area-weak {
  grid-area: weak;
}

.streak-flame {
  width: 96px;
  height: 96px;
  display: flex;
  align-items: center;
  justify-content: center;
}
.streak-flame-glyph {
  position: absolute;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 92px;
  line-height: 1;
  filter: drop-shadow(0 0 12px rgba(255, 123, 56, 0.65));
  animation: streak-flicker 1.8s ease-in-out infinite;
}
.streak-flame-number {
  position: relative;
  margin-top: 30px;
  font-size: 30px;
  line-height: 1;
  color: #fff6ec;
  text-shadow: 0 1px 2px rgba(0, 0, 0, 0.35);
}
@keyframes streak-flicker {
  0%,
  100% {
    opacity: 0.85;
    transform: scale(0.97);
  }
  50% {
    opacity: 1;
    transform: scale(1.05);
  }
}

@media (max-width: 760px) {
  .bento-grid {
    grid-template-columns: 1fr;
    grid-template-areas:
      "words"
      "books"
      "wordcard"
      "quote"
      "status"
      "weak";
  }
}
</style>
