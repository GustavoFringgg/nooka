import { describe, it, expect } from "vitest"
import { buildQuizQuestions, buildTypingQuestions } from "./quiz"
import type { Word } from "~/types/practice"

const words: Word[] = [
  { id: 1, categoryId: 1, term: "apple", ipa: null, definitionEN: "a fruit", definitionCN: "蘋果", partOfSpeech: "n.", examples: [] },
  { id: 2, categoryId: 1, term: "banana", ipa: null, definitionEN: "a fruit", definitionCN: "香蕉", partOfSpeech: "n.", examples: [] },
  { id: 3, categoryId: 1, term: "cherry", ipa: null, definitionEN: "a fruit", definitionCN: "櫻桃", partOfSpeech: "n.", examples: [] },
  { id: 4, categoryId: 1, term: "date", ipa: null, definitionEN: "a fruit", definitionCN: "椰棗", partOfSpeech: "n.", examples: [] },
  { id: 5, categoryId: 1, term: "elderberry", ipa: null, definitionEN: "a fruit", definitionCN: "接骨木莓", partOfSpeech: "n.", examples: [] }
]

describe("buildQuizQuestions", () => {
  it("每一題的選項數量都是 4 個", () => {
    const questions = buildQuizQuestions(words, "enToCn")
    questions.forEach((q) => {
      expect(q.options).toHaveLength(4)
    })
  })

  it("正確答案一定存在於選項裡", () => {
    const questions = buildQuizQuestions(words, "enToCn")
    questions.forEach((q) => {
      const ids = q.options.map((o) => o.word.id)
      expect(ids).toContain(q.word.id)
    })
  })

  it("遵守 count 參數的限制", () => {
    const questions = buildQuizQuestions(words, "enToCn", 3)
    expect(questions).toHaveLength(3)
  })
})

describe("buildTypingQuestions", () => {
  it("提示字(prompt)正確對應到中文定義(definitionCN)", () => {
    const questions = buildTypingQuestions(words)
    questions.forEach((q) => {
      expect(q.prompt).toBe(q.word.definitionCN)
    })
  })

  it("遵守 count 參數的限制", () => {
    const questions = buildTypingQuestions(words, 2)
    expect(questions).toHaveLength(2) // == expect(q.options.length).toBe(4)
  })
})

// describe("分組標籤") => 多取名跟被測試fn名稱相同
// it("測試案例描述字串")
