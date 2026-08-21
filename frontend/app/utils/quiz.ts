import type { Word, QuizOption, QuizQuestion } from "~/types/practice"

export type QuizDirection = "enToCn" | "cnToEn"

export function buildQuizQuestions(words: Word[], direction: QuizDirection, count?: number): QuizQuestion[] {
  const answerOption = (word: Word) => (direction === "enToCn" ? word.definitionCN : word.term)

  const questions = words.map((word) => {
    const filtered = words.filter((w) => w.id !== word.id)
    const shuffledFiltered = shuffle(filtered)
    const distractors = shuffledFiltered.slice(0, 3)

    const combined = [word, ...distractors]
    const shuffledCombined = shuffle(combined)

    const options: QuizOption[] = shuffledCombined.map((w) => {
      const text = answerOption(w)
      return { text, word: w }
    })
    const prompt = direction === "enToCn" ? word.term : word.definitionCN
    return { word, prompt, options }
  })

  const shuffledQuestions = shuffle(questions)
  return count ? shuffledQuestions.slice(0, count) : shuffledQuestions
}

export function shuffle<T>(array: T[]): T[] {
  const result = [...array]

  for (let i = result.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1))

    const temp = result[i]
    result[i] = result[j]!
    result[j] = temp!
  }
  return result
}

// Math.random() ,回傳一個 0 到 1 之間的浮點數(不包含 1,包含 0)
// J 拿來當作陣列的隨機索引 只會有0-4
