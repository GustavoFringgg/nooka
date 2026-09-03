import type { FlashcardLevel, FlashcardProgress, Word } from "~/types/practice"

// 會員系統跟後端 WordProgress 表還沒做,先用 localStorage 模擬每本書的單字卡等級進度;
// 之後接真的後端 API 時,把讀寫換成打 API,呼叫端(practice/index.vue、flashcard.vue)不用改介面

const NEW_WORD_BATCH_LIMIT = 20
const DAILY_REVIEW_LIMIT = 30

function storageKey(categoryId: number | string) {
  return `nooka:flashcard-progress:${categoryId}`
}

function todayISO(): string {
  return new Date().toISOString().slice(0, 10)
}

function addDays(days: number): string {
  const date = new Date()
  date.setDate(date.getDate() + days)
  return date.toISOString().slice(0, 10)
}

function loadProgressList(categoryId: number | string): FlashcardProgress[] {
  if (import.meta.server) return []
  const raw = localStorage.getItem(storageKey(categoryId))
  if (!raw) return []
  try {
    return JSON.parse(raw) as FlashcardProgress[]
  } catch {
    return []
  }
}

function saveProgressList(categoryId: number | string, list: FlashcardProgress[]) {
  if (import.meta.server) return
  localStorage.setItem(storageKey(categoryId), JSON.stringify(list))
}

export function useFlashcardProgress(categoryId: number | string) {
  function isFirstTimeForBook(): boolean {
    return loadProgressList(categoryId).length === 0
  }

  function getNewWords(allWords: Word[], limit = NEW_WORD_BATCH_LIMIT): Word[] {
    const learned = new Set(loadProgressList(categoryId).map((p) => p.wordId))
    return allWords.filter((w) => !learned.has(w.id)).slice(0, limit)
  }

  function getDueWords(allWords: Word[], limit = DAILY_REVIEW_LIMIT): Word[] {
    const today = todayISO()
    const due = loadProgressList(categoryId).filter(
      (p) => !p.isArchived && p.nextReviewAt !== null && p.nextReviewAt <= today
    )
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

  function getCounts(allWords: Word[]): { newCount: number; dueCount: number } {
    const list = loadProgressList(categoryId)
    const learned = new Set(list.map((p) => p.wordId))
    const today = todayISO()
    return {
      newCount: allWords.filter((w) => !learned.has(w.id)).length,
      dueCount: list.filter((p) => !p.isArchived && p.nextReviewAt !== null && p.nextReviewAt <= today).length
    }
  }

  function getProgress(wordId: number): FlashcardProgress | undefined {
    return loadProgressList(categoryId).find((p) => p.wordId === wordId)
  }

  function upsertProgress(wordId: number, patch: Partial<FlashcardProgress>) {
    const list = loadProgressList(categoryId)
    const index = list.findIndex((p) => p.wordId === wordId)
    const base: FlashcardProgress = index >= 0 ? list[index]! : { wordId, level: null, isArchived: false, nextReviewAt: null }
    const next = { ...base, ...patch }

    if (index >= 0) list[index] = next
    else list.push(next)

    saveProgressList(categoryId, list)
  }

  // 初學三選一:不認識 → Lv1、認識但不熟 → Lv2、非常熟悉 → 直接封存
  function markInitialLearning(wordId: number, choice: "unknown" | "familiar" | "mastered") {
    if (choice === "mastered") {
      upsertProgress(wordId, { level: null, isArchived: true, nextReviewAt: null })
      return
    }
    const level: FlashcardLevel = choice === "unknown" ? 1 : 2
    upsertProgress(wordId, { level, isArchived: false, nextReviewAt: addDays(1) })
  }

  // 複習「今天已練習」:Lv1~3 升一級 + 明天複習,Lv4 升 Lv5 + 後天複習(強制冷卻)
  // Lv5 不在這裡處理,呼叫端要先攔截,改走 resolveLevel5
  function markReviewed(wordId: number) {
    const progress = loadProgressList(categoryId).find((p) => p.wordId === wordId)
    if (!progress || progress.level === null || progress.level === 5) return

    if (progress.level === 4) {
      upsertProgress(wordId, { level: 5, nextReviewAt: addDays(2) })
      return
    }
    upsertProgress(wordId, { level: (progress.level + 1) as FlashcardLevel, nextReviewAt: addDays(1) })
  }

  // Lv5 滿級 Confirm 彈窗的兩個選項:畢業封存 / 打回 Lv1 重新學習
  function resolveLevel5(wordId: number, action: "graduate" | "restart") {
    if (action === "graduate") {
      upsertProgress(wordId, { isArchived: true, nextReviewAt: null })
    } else {
      upsertProgress(wordId, { level: 1, nextReviewAt: addDays(1) })
    }
  }

  return {
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
