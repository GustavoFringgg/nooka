export interface Category {
  id: number
  name: string
  description: string | null
  createdAt: string // C# 的 DateTime 序列化成 JSON 是 ISO 8601 格式的'字串'
  updatedAt: string
}

export interface Word {
  id: number
  categoryId: number
  term: string
  definitionEN: string
  definitionCN: string
  partOfSpeech: string
  examples: string[]
}

export interface QuizOption {
  text: string // 選項顯示文字(依 direction 顯示英文或中文)
  word: Word // 這個選項背後對應的完整單字物件
}

export interface QuizQuestion {
  word: Word // 這一題的正解單字(完整物件,答對/答錯後都能拿來顯示例句、詞性)
  prompt: string // 題目顯示內容
  options: QuizOption[] // 4 個選項(已洗牌),每個選項都帶著自己的 word
}
