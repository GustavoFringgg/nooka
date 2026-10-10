export interface AdminCategory {
  id: number
  name: string
  description: string | null
  color: string | null
  updatedAt: string
  wordCount: number
}

export interface AdminWord {
  id: number
  term: string
  definitionCN: string
  definitionEN: string
  partOfSpeech: string
  examples: string[]
  ipa: string | null
  categoryIds: number[]
}

export interface PagedResponse<T> {
  items: T[]
  totalCount: number
}
