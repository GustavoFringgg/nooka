<script setup lang="ts">
import type { Ref } from "vue"
import type { ActivitySummaryResponse, CategoryProgressSummaryResponse } from "~/types/practice"
type BarDay = { label: string; choice: number; typing: number }
type HeatCell = { date: Date; level: number }

const isLoggedIn = useIsLoggedIn()

const { data: categorySummaries } = await useAsyncData(
  "progress-category-summary",
  async () => {
    if (!isLoggedIn.value) return []
    return await useApiFetch<CategoryProgressSummaryResponse[]>("/api/progress/summary")
  },
  { watch: [isLoggedIn], default: () => [] as CategoryProgressSummaryResponse[] }
)

const { data: activitySummary } = await useAsyncData(
  "progress-activity-summary",
  async () => {
    if (!isLoggedIn.value) return { streak: { currentStreak: 0, longestStreak: 0 }, recent: [], heatmap: [] }
    return await useApiFetch<ActivitySummaryResponse>("/api/activity/summary")
  },
  {
    watch: [isLoggedIn],
    default: () =>
      ({ streak: { currentStreak: 0, longestStreak: 0 }, recent: [], heatmap: [] }) as ActivitySummaryResponse
  }
)

const streakBest = computed(() => activitySummary.value.streak.longestStreak)
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

const barDays = computed<BarDay[]>(() =>
  activitySummary.value.recent.map((d, i) => {
    const date = new Date(d.date)
    const isToday = i === activitySummary.value.recent.length - 1
    return {
      label: isToday ? "今天" : `${date.getMonth() + 1}/${date.getDate()}`,
      choice: d.choice,
      typing: d.typing
    }
  })
)
const streakDaysDisplay = ref(0)
const familiarWordsDisplay = ref(0)
const learningWordsDisplay = ref(0)
const dueTodayDisplay = ref(0)

const barMax = Math.max(...barDays.value.flatMap((d) => [d.choice, d.typing]), 1)

// ---- 全年練習熱力圖:選擇題或打字拼寫,當天完整跑完一輪就 +1 ----
const DAYS_PER_WEEK = 7

// 多少輪對應熱力圖的 0~4 等級,門檻是先抓的參考值,之後可依實際使用情況調整
const heatmapLevel = (count: number) => {
  if (count === 0) return 0
  if (count <= 2) return 1
  if (count <= 4) return 2
  if (count <= 6) return 3
  return 4
}

// 前後補 null(空白格),讓第一欄從真正的週日開始、最後一欄湊滿 7 天,之後每一列才會對應到正確的星期幾
const heatmapCells = computed<(HeatCell | null)[]>(() => {
  const days: HeatCell[] = activitySummary.value.heatmap.map((d) => ({
    date: new Date(d.date),
    level: heatmapLevel(d.count)
  }))
  if (days.length === 0) return []

  const firstWeekday = days[0]!.date.getDay() // 0 = 週日 ... 6 = 週六
  const leadingPadding: null[] = Array(firstWeekday).fill(null)
  const padded = [...leadingPadding, ...days]
  const trailingCount = (DAYS_PER_WEEK - (padded.length % DAYS_PER_WEEK)) % DAYS_PER_WEEK
  const trailingPadding: null[] = Array(trailingCount).fill(null)

  return [...padded, ...trailingPadding]
})

// 依「週」分欄(GitHub 熱力圖排法:一欄一週,一欄裡由上到下是週日~週六),補完 null 之後長度保證是 7 的倍數
const heatmapColumns = computed<(HeatCell | null)[][]>(() => {
  const cells = heatmapCells.value
  const totalWeeks = cells.length / DAYS_PER_WEEK
  const columns: (HeatCell | null)[][] = []
  for (let i = 0; i < totalWeeks; i++) {
    columns.push(cells.slice(i * DAYS_PER_WEEK, i * DAYS_PER_WEEK + DAYS_PER_WEEK))
  }
  return columns
})

// 每欄第一個「非空白」格的月份若跟前一欄不同,才顯示月份文字,避免每欄都重複標
const monthLabels = computed<string[]>(() => {
  const firstRealCell = (col: (HeatCell | null)[]) => col.find((c) => c !== null) ?? null
  return heatmapColumns.value.map((col, i) => {
    const current = firstRealCell(col)
    if (!current) return ""
    const prev = i > 0 ? firstRealCell(heatmapColumns.value[i - 1]!) : null
    const month = current.date.getMonth()
    const prevMonth = prev ? prev.date.getMonth() : -1
    return month !== prevMonth ? `${month + 1}月` : ""
  })
})

// 把「一欄一週」轉置成「一列一個星期幾」,改用 flex-1 橫向排列才能撐滿卡片寬度
const heatmapRows = computed<(HeatCell | null)[][]>(() =>
  Array.from({ length: DAYS_PER_WEEK }, (_, row) => heatmapColumns.value.map((col) => col[row] ?? null))
)

const cellLevelClass = [
  "bg-paper-fg/8",
  "bg-paper-primary/28",
  "bg-paper-primary/52",
  "bg-paper-primary/78",
  "bg-paper-primary"
]

useHead({
  link: [
    { rel: "preconnect", href: "https://fonts.googleapis.com" },
    {
      rel: "stylesheet",
      href: "https://fonts.googleapis.com/css2?family=Newreader:ital,opsz,wght@0,6..72,400;0,6..72,500;1,6..72,400&family=Work+Sans:wght@400;500;600&display=swap"
    }
  ]
})

const familiarWords = computed(() => categorySummaries.value.reduce((sum, s) => sum + s.familiar, 0))

const learningWords = computed(() => categorySummaries.value.reduce((sum, s) => sum + s.learning, 0))

const learningBookCount = computed(() => categorySummaries.value.filter((s) => s.familiar + s.learning > 0).length)

const dueToday = computed(() => categorySummaries.value.reduce((sum, s) => sum + s.dueToday, 0))

onMounted(() => {
  animateCount(activitySummary.value.streak.currentStreak, streakDaysDisplay)
  animateCount(familiarWords.value, familiarWordsDisplay)
  animateCount(learningWords.value, learningWordsDisplay)
  animateCount(dueToday.value, dueTodayDisplay)
})
</script>

<template>
  <div class="min-h-screen bg-paper-bg-alt font-body text-paper-fg">
    <AppNav />

    <div class="max-w-[1080px] mx-auto px-6 md:px-8 pt-11 pb-20">
      <div v-if="isLoggedIn">
        <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-5">
          <div
            class="bg-paper-bg rounded-[18px] p-5 shadow-[0_20px_40px_-30px_rgba(43,42,37,0.35)] flex flex-col justify-center"
          >
            <div class="flex items-center justify-between mb-2.5">
              <span class="text-[11.5px] text-paper-muted uppercase tracking-[0.08em]">連續練習</span>
              <span class="text-xs text-paper-primary">▲ 最長紀錄 {{ streakBest }} 天</span>
            </div>
            <div class="font-display text-[32px] leading-none">
              {{ streakDaysDisplay }}
              <span class="font-body text-[13px] text-paper-muted ml-1">天</span>
            </div>
          </div>

          <div
            class="bg-paper-bg rounded-[18px] p-5 shadow-[0_20px_40px_-30px_rgba(43,42,37,0.35)] flex flex-col justify-center"
          >
            <div class="text-[11.5px] text-paper-muted uppercase tracking-[0.08em] mb-2.5">已熟悉</div>
            <div class="font-display text-[32px] leading-none">
              {{ familiarWordsDisplay }}
              <span class="font-body text-[13px] text-paper-muted ml-1">個字</span>
            </div>
          </div>

          <div
            class="bg-paper-bg rounded-[18px] p-5 shadow-[0_20px_40px_-30px_rgba(43,42,37,0.35)] flex flex-col justify-center"
          >
            <div class="flex items-center justify-between mb-2.5">
              <span class="text-[11.5px] text-paper-muted uppercase tracking-[0.08em]">學習中</span>
              <span class="text-xs text-paper-muted">你讀了 {{ learningBookCount }} 本書</span>
            </div>
            <div class="font-display text-[32px] leading-none">
              {{ learningWordsDisplay }}
              <span class="font-body text-[13px] text-paper-muted ml-1">個字</span>
            </div>
          </div>

          <div
            class="bg-paper-bg rounded-[18px] p-5 shadow-[0_20px_40px_-30px_rgba(43,42,37,0.35)] flex flex-col justify-center"
          >
            <div class="flex items-center justify-between mb-2.5">
              <span class="text-[11.5px] text-paper-muted uppercase tracking-[0.08em]">今天待複習</span>
              <NuxtLink to="/practice" class="text-xs text-paper-primary no-underline hover:text-paper-accent">
                → 去練習
              </NuxtLink>
            </div>
            <div class="font-display text-[32px] leading-none text-paper-accent">
              {{ dueTodayDisplay }}
              <span class="font-body text-[13px] text-paper-muted ml-1">張</span>
            </div>
          </div>
        </div>

        <div class="bg-paper-bg rounded-[20px] p-6 mb-5 shadow-[0_20px_40px_-30px_rgba(43,42,37,0.35)]">
          <div class="flex items-baseline justify-between mb-4.5">
            <h2 class="font-display text-[19px] font-normal m-0">近 14 天練習量</h2>
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
              <span
                class="text-[10.5px] text-paper-muted"
                :class="{ 'text-paper-fg font-medium': day.label === '今天' }"
              >
                {{ day.label }}
              </span>
            </div>
          </div>

          <div class="flex items-center gap-5 mt-4 justify-end text-[11.5px] text-paper-muted">
            <span class="flex items-center gap-1.5">
              <span class="w-2.5 h-2.5 rounded-sm bg-paper-primary/85" />
              選擇題
            </span>
            <span class="flex items-center gap-1.5">
              <span class="w-2.5 h-2.5 rounded-sm bg-paper-accent" />
              打字拼寫
            </span>
          </div>
        </div>

        <div class="bg-paper-bg rounded-[20px] p-6 mb-5 shadow-[0_20px_40px_-30px_rgba(43,42,37,0.35)]">
          <div class="flex flex-col gap-[3px]">
            <div class="flex gap-[3px] ml-[25px] mb-1">
              <div
                v-for="(label, i) in monthLabels"
                :key="i"
                class="flex-1 text-[10px] text-paper-muted whitespace-nowrap overflow-visible"
              >
                {{ label }}
              </div>
            </div>

            <div v-for="(row, r) in heatmapRows" :key="r" class="flex gap-[3px] items-stretch">
              <div class="w-[22px] shrink-0 flex items-center text-[10px] text-paper-muted">
                {{ ["", "一", "", "三", "", "五", ""][r] }}
              </div>
              <div
                v-for="(cell, i) in row"
                :key="i"
                class="flex-1 aspect-square rounded-[3px]"
                :class="cell ? cellLevelClass[cell.level] : 'invisible'"
                :title="cell ? `${cell.date.getMonth() + 1}/${cell.date.getDate()}` : undefined"
              />
            </div>
          </div>

          <div class="flex items-center gap-1.5 mt-3 justify-start text-[11px] text-paper-muted">
            少
            <span v-for="(cls, i) in cellLevelClass" :key="i" class="w-[11px] h-[11px] rounded-[3px]" :class="cls" />
            多
          </div>
        </div>

        <p class="text-center text-sm text-paper-muted mt-6">
          想看每本書各自的進度?去
          <NuxtLink to="/practice" class="text-paper-primary">書架</NuxtLink>
          看單字書上的標示。
        </p>
      </div>

      <div v-else class="flex items-center justify-center px-6 py-24">
        <div
          class="bg-paper-bg rounded-[20px] px-10 py-8 text-center shadow-[0_25px_60px_-20px_rgba(43,42,37,0.45)] max-w-sm"
        >
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
</template>
