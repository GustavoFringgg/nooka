import type { FlashcardLevel, FlashcardProgress, Word } from "~/types/practice"

// 會員系統跟後端 WordProgress 表還沒做,先用 localStorage 模擬每本書的單字卡等級進度;
// 之後接真的後端 API 時,把讀寫換成打 API,呼叫端(practice/index.vue、flashcard.vue)不用改介面

const NEW_WORD_BATCH_LIMIT = 20
const DAILY_REVIEW_LIMIT = 30

const todayISO = (): string => {
  return new Date().toISOString().slice(0, 10)
}

const addDays = (days: number): string => {
  const date = new Date()
  date.setDate(date.getDate() + days)
  return date.toISOString().slice(0, 10)
}

export const useFlashcardProgress = (categoryId: number | string) => {
  const progressList = ref<FlashcardProgress[]>([])
  const dirtyMap = new Map<number, FlashcardProgress>()

  // 批次更新使用者單字學習記錄
  const submitBatch = async () => {
    await useApiFetch(`/api/progress/batch`, {
      method: "POST",
      body: Array.from(dirtyMap.values()) // Map 轉陣列 Array.from
    })
    dirtyMap.clear()
  }

  // 讀取使用者某書的單字學習紀錄
  const loadProgressList = async () => {
    const result = await useApiFetch<FlashcardProgress[]>(`/api/progress/category/${categoryId}`)
    progressList.value = result
  }

  // 判斷使用者在此書有沒有學習紀錄
  const isFirstTimeForBook = (): boolean => {
    return progressList.value.length === 0
  }

  // 取得這本書全部單字裡，篩選出還沒學過的新字
  const getNewWords = (allWords: Word[], limit = NEW_WORD_BATCH_LIMIT): Word[] => {
    const learned = new Set(progressList.value.map((p) => p.wordId))
    return allWords.filter((w) => !learned.has(w.id)).slice(0, limit)
  }

  // 從 progressList 篩出今天該複習的卡: 沒封存 && nextReviewAt !== null && nextReviewAt <= 今天
  const getDueWords = (allWords: Word[], limit = DAILY_REVIEW_LIMIT): Word[] => {
    const today = todayISO()
    const due = progressList.value.filter((p) => !p.isArchived && p.nextReviewAt !== null && p.nextReviewAt <= today)
    due.sort((a, b) => {
      if (a.level !== b.level) return (a.level ?? 0) - (b.level ?? 0)
      return (a.nextReviewAt ?? "").localeCompare(b.nextReviewAt ?? "")
    })

    const wordById = new Map(allWords.map((w) => [w.id, w]))
    return due
      .slice(0, limit)
      .map((p) => wordById.get(p.wordId))
      .filter((w): w is Word => w !== undefined)
  }

  const getCounts = (allWords: Word[]): { newCount: number; dueCount: number } => {
    const list = progressList.value
    const learned = new Set(list.map((p) => p.wordId))
    const today = todayISO()
    return {
      // 算出還沒出現在 progressList 的數量(還沒學過的新字有幾個)
      newCount: allWords.filter((w) => !learned.has(w.id)).length,
      // progressList 裡沒封存、且 nextReviewAt <= 今天 的數量(今天該複習幾張)
      dueCount: list.filter((p) => !p.isArchived && p.nextReviewAt !== null && p.nextReviewAt <= today).length
    }
  }

  // 給呼叫端(flashcard.vue)在畫面上查「這張卡現在是 Lv 幾、有沒有封存」,拿來決定按鈕要顯示什麼
  const getProgress = (wordId: number): FlashcardProgress | undefined => {
    return progressList.value.find((p) => p.wordId === wordId)
  }

  // 所有「改狀態」的共用底層函式
  const upsertProgress = (wordId: number, patch: Partial<FlashcardProgress>) => {
    const list = progressList.value
    const index = list.findIndex((p) => p.wordId === wordId)
    const base: FlashcardProgress =
      index >= 0 ? list[index]! : { wordId, level: null, isArchived: false, nextReviewAt: null }
    const next = { ...base, ...patch }

    if (index >= 0) list[index] = next
    else list.push(next)

    dirtyMap.set(wordId, next)
  }

  // 初學三選一:不認識 → Lv1、認識但不熟 → Lv2、非常熟悉 → 直接封存
  const markInitialLearning = (wordId: number, choice: "unknown" | "familiar" | "mastered") => {
    if (choice === "mastered") {
      upsertProgress(wordId, { level: null, isArchived: true, nextReviewAt: null })
      return
    }
    const level: FlashcardLevel = choice === "unknown" ? 1 : 2
    upsertProgress(wordId, { level, isArchived: false, nextReviewAt: addDays(1) })
  }

  // 複習「今天已複習」:Lv1~3 升一級 + 明天複習,Lv4 升 Lv5 + 後天複習(強制冷卻)
  // Lv5 不在這裡處理,呼叫端要先攔截,改走 resolveLevel5
  const markReviewed = (wordId: number) => {
    const progress = progressList.value.find((p) => p.wordId === wordId)
    if (!progress || progress.level === null || progress.level === 5) return

    if (progress.level === 4) {
      upsertProgress(wordId, { level: 5, nextReviewAt: addDays(2) })
      return
    }
    upsertProgress(wordId, { level: (progress.level + 1) as FlashcardLevel, nextReviewAt: addDays(1) })
  }

  // Lv5 滿級 Confirm 彈窗的兩個選項:畢業封存 / 打回 Lv1 重新學習
  const resolveLevel5 = (wordId: number, action: "graduate" | "restart") => {
    if (action === "graduate") {
      upsertProgress(wordId, { isArchived: true, nextReviewAt: null })
    } else {
      upsertProgress(wordId, { level: 1, nextReviewAt: addDays(1) })
    }
  }

  return {
    submitBatch,
    loadProgressList,
    isFirstTimeForBook,
    getNewWords,
    getDueWords,
    getCounts,
    getProgress,
    markInitialLearning,
    markReviewed,
    resolveLevel5
  }
}
