<script setup lang="ts">
import type { Ref } from "vue"
import { onMounted, ref } from "vue"

// Stage 8(學習紀錄頁面):目前是純假資料版面,對應 learningRecord.md 2026/09/28 定案(9/28 後續補充:統計列改回這頁,代表跨全部書的總計)。
// 統計列(連續天數/已精熟/學習中/今天待複習)之後改真資料時,可以直接打現有的 GET /api/progress/summary(回傳每本書的數字)再前端加總,不用等後端補新功能;
// 14 天長條圖 / 全年熱力圖只算「選擇題 + 打字拼寫」,不含單字卡,這兩個要串真資料則需要新增 DailyActivity 表才能算,還沒排。
// 這次先把版面全部用假資料撐起來,之後輪到串 API 再逐一替換。

const isLoggedIn = useIsLoggedIn()

// 統計列數字從 0 開始滾到目標值的動畫,手法跟 overview.vue 的 totalWordsDisplay 一樣(requestAnimationFrame + ease-out)
const animateCount = (target: number, display: Ref<number>, duration = 1000) => {
  const start = performance.now()
  const step = (now: number) => {
    const progress = Math.min(1, (now - start) / duration)
    const eased = 1 - Math.pow(1 - progress, 3)
    display.value = Math.round(eased * target)
    if (progress < 1) requestAnimationFrame(step)
  }
  requestAnimationFrame(step)
}

const today = new Date()
today.setHours(0, 0, 0, 0)

// 固定種子的簡易 PRNG,避免用 Math.random() 造成 SSR/CSR 算出不同的假資料(hydration mismatch)
function mulberry32(seed: number) {
  return () => {
    seed |= 0
    seed = (seed + 0x6d2b79f5) | 0
    let t = Math.imul(seed ^ (seed >>> 15), 1 | seed)
    t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296
  }
}

// ---- 統計列(假資料,跨全部書的總計;之後改真資料會串 GET /api/progress/summary 並把各本書加總)----
const streakDays = 12
const streakBest = 27
const familiarWords = 184
const familiarWeeklyDelta = 14
const learningWords = 96
const learningBookCount = 3
const dueToday = 23

const streakDaysDisplay = ref(0)
const familiarWordsDisplay = ref(0)
const learningWordsDisplay = ref(0)
const dueTodayDisplay = ref(0)

onMounted(() => {
  animateCount(streakDays, streakDaysDisplay)
  animateCount(familiarWords, familiarWordsDisplay)
  animateCount(learningWords, learningWordsDisplay)
  animateCount(dueToday, dueTodayDisplay)
})

// ---- 近 14 天練習量:選擇題 / 打字拼寫(假資料)----
type BarDay = { label: string; choice: number; typing: number }

const barDays: BarDay[] = (() => {
  const rand = mulberry32(925)
  const days: BarDay[] = []
  for (let i = 13; i >= 0; i--) {
    const date = new Date(today)
    date.setDate(date.getDate() - i)
    const label = i === 0 ? "今天" : `${date.getMonth() + 1}/${date.getDate()}`
    days.push({
      label,
      choice: Math.round(rand() * 8) + 1,
      typing: Math.round(rand() * 6)
    })
  }
  return days
})()

const barMax = Math.max(...barDays.flatMap((d) => [d.choice, d.typing]), 1)
const totalRounds = barDays.reduce((sum, d) => sum + d.choice + d.typing, 0)

// ---- 全年練習熱力圖:選擇題或打字拼寫,當天完整跑完一輪就 +1(假資料)----
const WEEKS = 53
const DAYS_PER_WEEK = 7
const TOTAL_CELLS = WEEKS * DAYS_PER_WEEK

type HeatCell = { date: Date; level: number }

const heatmapCells: HeatCell[] = (() => {
  const rand = mulberry32(20260928)
  const cells: HeatCell[] = []
  const startOffset = TOTAL_CELLS - 1
  for (let i = 0; i < TOTAL_CELLS; i++) {
    const date = new Date(today)
    date.setDate(date.getDate() - (startOffset - i))
    const r = rand()
    let level = 0
    if (r > 0.82) level = 4
    else if (r > 0.65) level = 3
    else if (r > 0.48) level = 2
    else if (r > 0.32) level = 1
    cells.push({ date, level })
  }
  return cells
})()

// 依「週」分欄(GitHub 熱力圖排法:一欄一週,一欄裡由上到下是週日~週六)
const heatmapColumns: HeatCell[][] = []
for (let i = 0; i < WEEKS; i++) {
  heatmapColumns.push(heatmapCells.slice(i * DAYS_PER_WEEK, i * DAYS_PER_WEEK + DAYS_PER_WEEK))
}

// 每欄第一格的月份若跟前一欄不同,才顯示月份文字,避免每欄都重複標
const monthLabels: string[] = heatmapColumns.map((col, i) => {
  const month = col[0]!.date.getMonth()
  const prevMonth = i > 0 ? heatmapColumns[i - 1]![0]!.date.getMonth() : -1
  return month !== prevMonth ? `${month + 1}月` : ""
})

const cellLevelClass = ["bg-paper-fg/8", "bg-paper-primary/28", "bg-paper-primary/52", "bg-paper-primary/78", "bg-paper-primary"]

useHead({
  link: [
    { rel: "preconnect", href: "https://fonts.googleapis.com" },
    {
      rel: "stylesheet",
      href: "https://fonts.googleapis.com/css2?family=Newreader:ital,opsz,wght@0,6..72,400;0,6..72,500;1,6..72,400&family=Work+Sans:wght@400;500;600&display=swap"
    }
  ]
})
</script>

<template>
  <div class="min-h-screen bg-paper-bg-alt font-body text-paper-fg">
    <AppNav />

    <div class="max-w-[1080px] mx-auto px-6 md:px-8 pt-11 pb-20">
      <div class="flex items-end justify-between mb-7">
        <div>
          <h1 class="font-display font-normal text-[34px] m-0 mb-1.5">學習紀錄</h1>
          <p class="text-paper-muted text-sm m-0">你的學習旅程,一眼看完</p>
        </div>
      </div>

      <div class="relative">
        <div
          class="transition-[filter,opacity] duration-200"
          :class="!isLoggedIn ? 'blur-[6px] opacity-70 pointer-events-none select-none' : ''"
        >
        <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-5">
          <div class="bg-paper-bg rounded-[18px] p-5 shadow-[0_20px_40px_-30px_rgba(43,42,37,0.35)]">
            <div class="text-[11.5px] text-paper-muted uppercase tracking-[0.08em] mb-2.5">連續練習</div>
            <div class="font-display text-[32px] leading-none">
              {{ streakDaysDisplay }}<span class="font-body text-[13px] text-paper-muted ml-1">天</span>
            </div>
            <div class="mt-2.5 text-xs text-paper-primary">▲ 最長紀錄 {{ streakBest }} 天</div>
          </div>

          <div class="bg-paper-bg rounded-[18px] p-5 shadow-[0_20px_40px_-30px_rgba(43,42,37,0.35)]">
            <div class="text-[11.5px] text-paper-muted uppercase tracking-[0.08em] mb-2.5">已精熟單字</div>
            <div class="font-display text-[32px] leading-none">
              {{ familiarWordsDisplay }}<span class="font-body text-[13px] text-paper-muted ml-1">個字</span>
            </div>
            <div class="mt-2.5 text-xs text-paper-primary">▲ 本週 +{{ familiarWeeklyDelta }}</div>
          </div>

          <div class="bg-paper-bg rounded-[18px] p-5 shadow-[0_20px_40px_-30px_rgba(43,42,37,0.35)]">
            <div class="text-[11.5px] text-paper-muted uppercase tracking-[0.08em] mb-2.5">學習中</div>
            <div class="font-display text-[32px] leading-none">
              {{ learningWordsDisplay }}<span class="font-body text-[13px] text-paper-muted ml-1">個字</span>
            </div>
            <div class="mt-2.5 text-xs text-paper-muted">跨 {{ learningBookCount }} 本書</div>
          </div>

          <div class="bg-paper-bg rounded-[18px] p-5 shadow-[0_20px_40px_-30px_rgba(43,42,37,0.35)]">
            <div class="text-[11.5px] text-paper-muted uppercase tracking-[0.08em] mb-2.5">今天待複習</div>
            <div class="font-display text-[32px] leading-none text-paper-accent">
              {{ dueTodayDisplay }}<span class="font-body text-[13px] text-paper-muted ml-1">張</span>
            </div>
            <NuxtLink
              to="/practice"
              class="inline-block mt-3 rounded-[10px] bg-paper-primary text-paper-bg text-xs px-3.5 py-1.5 no-underline"
            >
              去複習
            </NuxtLink>
          </div>
        </div>

        <div class="bg-paper-bg rounded-[20px] p-6 mb-5 shadow-[0_20px_40px_-30px_rgba(43,42,37,0.35)]">
          <div class="flex items-baseline justify-between mb-4.5">
            <h2 class="font-display text-[19px] font-normal m-0">近 14 天練習量</h2>
            <span class="text-xs text-paper-muted">總計 {{ totalRounds }} 輪</span>
          </div>

          <div class="flex gap-2 px-1">
            <div v-for="day in barDays" :key="day.label" class="flex-1 flex flex-col items-center gap-2">
              <div class="w-full h-40 flex items-end justify-center gap-1">
                <div
                  class="w-full max-w-[11px] rounded-t-[5px] rounded-b-[2px] bg-paper-primary/85"
                  :style="{ height: `${(day.choice / barMax) * 100}%` }"
                />
                <div
                  class="w-full max-w-[11px] rounded-t-[5px] rounded-b-[2px] bg-paper-accent"
                  :style="{ height: `${(day.typing / barMax) * 100}%` }"
                />
              </div>
              <span class="text-[10.5px] text-paper-muted" :class="{ 'text-paper-fg font-medium': day.label === '今天' }">
                {{ day.label }}
              </span>
            </div>
          </div>

          <div class="flex items-center gap-5 mt-4 justify-end text-[11.5px] text-paper-muted">
            <span class="flex items-center gap-1.5"><span class="w-2.5 h-2.5 rounded-sm bg-paper-primary/85" />選擇題</span>
            <span class="flex items-center gap-1.5"><span class="w-2.5 h-2.5 rounded-sm bg-paper-accent" />打字拼寫</span>
          </div>
        </div>

        <div class="bg-paper-bg rounded-[20px] p-6 mb-5 shadow-[0_20px_40px_-30px_rgba(43,42,37,0.35)]">
          <div class="flex items-baseline justify-between mb-4.5">
            <h2 class="font-display text-[19px] font-normal m-0">全年練習熱力圖</h2>
            <span class="text-xs text-paper-muted">過去 12 個月・選擇題 + 打字拼寫</span>
          </div>

          <div class="overflow-x-auto pb-1">
            <div class="inline-flex flex-col">
              <div class="grid ml-[26px] mb-1" :style="{ gridTemplateColumns: `repeat(${WEEKS}, 12px)`, gap: '3px' }">
                <div v-for="(label, i) in monthLabels" :key="i" class="text-[10px] text-paper-muted whitespace-nowrap overflow-visible">{{ label }}</div>
              </div>
              <div class="flex">
                <div class="grid mr-1 w-[22px]" :style="{ gridTemplateRows: 'repeat(7, 12px)', gap: '3px' }">
                  <span
                    v-for="(d, i) in ['', '一', '', '三', '', '五', '']"
                    :key="i"
                    class="flex items-center h-3 text-[10px] text-paper-muted"
                  >{{ d }}</span>
                </div>
                <div class="grid" :style="{ gridTemplateColumns: `repeat(${WEEKS}, 12px)`, gridTemplateRows: 'repeat(7, 12px)', gridAutoFlow: 'column', gap: '3px' }">
                  <div
                    v-for="(cell, i) in heatmapCells"
                    :key="i"
                    class="w-3 h-3 rounded-[3px]"
                    :class="cellLevelClass[cell.level]"
                    :title="`${cell.date.getMonth() + 1}/${cell.date.getDate()}`"
                  />
                </div>
              </div>
            </div>
          </div>

          <div class="flex items-center gap-1.5 mt-3 justify-end text-[11px] text-paper-muted">
            少
            <span v-for="(cls, i) in cellLevelClass" :key="i" class="w-[11px] h-[11px] rounded-[3px]" :class="cls" />
            多
          </div>
        </div>

        <p class="text-center text-sm text-paper-muted mt-6">
          想看每本書各自的進度?去 <NuxtLink to="/practice" class="text-paper-primary">書架</NuxtLink> 看單字書上的標示。
        </p>
        </div>

        <div v-if="!isLoggedIn" class="absolute inset-0 flex items-center justify-center px-6">
          <div class="bg-paper-bg rounded-[20px] px-10 py-8 text-center shadow-[0_25px_60px_-20px_rgba(43,42,37,0.45)] max-w-sm">
            <p class="font-display text-xl m-0 mb-2">登入後查看你的學習進度</p>
            <p class="text-paper-muted text-sm m-0 mb-5">練習紀錄會跟你的帳號綁在一起,登入後才會開始累積。</p>
            <NuxtLink
              to="/login"
              class="inline-block rounded-full bg-paper-primary text-paper-bg font-body px-6 py-2.5 text-sm font-medium no-underline transition-colors hover:bg-paper-accent"
            >
              去登入
            </NuxtLink>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
