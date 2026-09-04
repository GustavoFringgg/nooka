<script setup lang="ts">
import type { Word } from "~/types/practice"
import { gsap } from "gsap"

type PartOfSpeech = "形容詞" | "副詞" | "動詞" | "名詞" | "代名詞" | "介系詞" | "連接詞" | "感嘆詞"

const posColors: Record<PartOfSpeech, { bg: string; text: string }> = {
  形容詞: { bg: "rgba(193,101,59,.16)", text: "#8f4128" },
  副詞: { bg: "rgba(60,90,68,.16)", text: "#2c433a" },
  動詞: { bg: "rgba(156,74,60,.16)", text: "#78392e" },
  名詞: { bg: "rgba(62,110,142,.16)", text: "#2d5670" },
  代名詞: { bg: "rgba(107,76,110,.16)", text: "#503a52" },
  介系詞: { bg: "rgba(138,122,62,.16)", text: "#6a5e2f" },
  連接詞: { bg: "rgba(47,110,107,.16)", text: "#235350" },
  感嘆詞: { bg: "rgba(161,63,94,.16)", text: "#7a2f47" }
}
const defaultPosColor = { bg: "rgba(120,120,120,.16)", text: "#555555" }
function posColor(pos: string) {
  return posColors[pos as PartOfSpeech] ?? defaultPosColor
}

const route = useRoute()
const categoryId = route.params.categoryId as string
const mode = route.query.mode === "review" ? "review" : "new"

const { data: words, pending, error } = await useFetch<Word[]>(useApiUrl(`/api/words/category/${categoryId}`))

const progress = useFlashcardProgress(categoryId)

// 進度來自 localStorage,只存在 client,server render 時算不出正確清單,
// 所以這份清單留到 onMounted 才算,搭配 template 用 <ClientOnly> 包住這一段,避免 SSR/CSR 算出不同清單而 hydration mismatch
const sessionWords = ref<Word[]>([])
const sessionReady = ref(false)

onMounted(() => {
  if (words.value) {
    sessionWords.value = mode === "review" ? progress.getDueWords(words.value) : progress.getNewWords(words.value)
  }
  sessionReady.value = true
})

const currentIndex = ref(0)
const currentWord = computed(() => sessionWords.value[currentIndex.value])
const currentLevel = computed(() => (currentWord.value ? (progress.getProgress(currentWord.value.id)?.level ?? 1) : 1))
const progressPercent = computed(() =>
  sessionWords.value.length ? (currentIndex.value / sessionWords.value.length) * 100 : 0
)

// 翻牌動畫沿用 cardTest.vue 的 GSAP 手法
const cardRef = ref<HTMLElement | null>(null)
const isFlipped = ref(false)

function flipCard() {
  isFlipped.value = !isFlipped.value
  gsap.to(cardRef.value, { rotateY: isFlipped.value ? 180 : 0, duration: 0.6 })
}

function resetFlip() {
  isFlipped.value = false
  if (cardRef.value) gsap.set(cardRef.value, { rotateY: 0 })
}

watch(currentWord, () => resetFlip())

function advanceCard() {
  currentIndex.value++
}

function markInitial(choice: "unknown" | "familiar" | "mastered") {
  if (!currentWord.value) return
  progress.markInitialLearning(currentWord.value.id, choice)
  advanceCard()
}

const isLevel5ModalOpen = ref(false)

function handleReviewed() {
  if (!currentWord.value) return
  if (currentLevel.value === 5) {
    isLevel5ModalOpen.value = true
    return
  }
  progress.markReviewed(currentWord.value.id)
  advanceCard()
}

function resolveLevel5(action: "graduate" | "restart") {
  if (!currentWord.value) return
  progress.resolveLevel5(currentWord.value.id, action)
  isLevel5ModalOpen.value = false
  advanceCard()
}
</script>

<template>
  <div class="min-h-screen bg-paper-bg font-body text-paper-fg pb-20" style="color-scheme: light">
    <AppNav />

    <p v-if="pending" class="text-center text-paper-muted py-24">載入中...</p>
    <p v-else-if="error" class="text-center text-paper-muted py-24">載入失敗...</p>

    <template v-else>
      <ClientOnly>
        <template #fallback>
          <p class="text-center text-paper-muted py-24">載入中...</p>
        </template>

      <p v-if="!sessionReady" class="text-center text-paper-muted py-24">載入中...</p>

      <div v-else-if="currentWord" class="max-w-2xl mx-auto px-6 pt-8">
        <div class="mb-8">
          <div class="flex items-center justify-between mb-2 text-sm text-paper-muted">
            <span>第 {{ currentIndex + 1 }} / {{ sessionWords.length }} 張</span>
            <span class="text-paper-accent">{{ mode === "review" ? "複習" : "學習新單字" }}</span>
          </div>
          <div class="h-1.5 rounded-full bg-paper-fg/10 overflow-hidden">
            <div
              class="h-full rounded-full bg-paper-primary transition-[width] duration-500 ease-out"
              :style="{ width: `${progressPercent}%` }"
            />
          </div>
        </div>

        <div style="perspective: 1400px" class="mb-8" @click="flipCard">
          <div
            ref="cardRef"
            style="position: relative; width: 100%; height: 320px; transform-style: preserve-3d; cursor: pointer"
          >
            <div
              class="rounded-3xl bg-paper-bg-alt border-2 border-paper-fg/25 shadow-[0_18px_40px_-20px_rgba(43,42,37,0.3)] flex flex-col items-center justify-center gap-2 px-8 text-center"
              style="position: absolute; inset: 0; backface-visibility: hidden"
            >
              <div class="font-display text-5xl text-paper-fg">{{ currentWord.term }}</div>
              <div class="text-paper-muted text-sm" style="font-family: ui-monospace, &quot;SF Mono&quot;, monospace">
                /{{ currentWord.ipa ?? "" }}/
              </div>
              <span
                class="text-[11px] px-1.5 py-0.5 rounded-full mt-2"
                :style="{ color: posColor(currentWord.partOfSpeech).text, background: posColor(currentWord.partOfSpeech).bg }"
              >
                {{ currentWord.partOfSpeech }}
              </span>

              <div v-if="mode === 'review'" class="flex gap-1.5 mt-3">
                <div
                  v-for="n in 5"
                  :key="n"
                  class="w-6.5 h-1.5 rounded-full"
                  :class="n <= currentLevel ? 'bg-paper-accent' : 'bg-paper-fg/10'"
                />
              </div>

              <p class="text-xs text-paper-muted mt-4 m-0">點卡片看意思</p>
            </div>

            <div
              class="rounded-3xl bg-paper-primary/10 flex flex-col items-center justify-center gap-3 px-8 text-center"
              style="position: absolute; inset: 0; backface-visibility: hidden; transform: rotateY(180deg)"
            >
              <div class="font-display text-3xl text-paper-primary">{{ currentWord.definitionCN }}</div>
              <div class="text-sm text-paper-fg max-w-sm">{{ currentWord.definitionEN }}</div>
              <div v-if="currentWord.examples?.length" class="text-xs text-paper-muted italic">
                "{{ currentWord.examples[0] }}"
              </div>
            </div>
          </div>
        </div>

        <div v-if="mode === 'new'" class="grid grid-cols-3 gap-3">
          <button
            type="button"
            class="rounded-2xl border-1.5 border-paper-fg/20 text-paper-fg h-18 flex flex-col items-center justify-center gap-1.5 text-[13.5px] cursor-pointer transition-colors hover:bg-paper-fg/5"
            @click="markInitial('unknown')"
          >
            不認識
          </button>
          <button
            type="button"
            class="rounded-2xl border-1.5 border-paper-primary/50 text-paper-primary h-18 flex flex-col items-center justify-center gap-1.5 text-[13.5px] cursor-pointer transition-colors hover:bg-paper-primary/8"
            @click="markInitial('familiar')"
          >
            認識但不熟
          </button>
          <button
            type="button"
            class="rounded-2xl bg-paper-primary text-paper-bg h-18 flex flex-col items-center justify-center gap-1.5 text-[13.5px] cursor-pointer transition-colors hover:bg-paper-accent"
            @click="markInitial('mastered')"
          >
            非常熟悉
          </button>
        </div>

        <UButton
          v-else
          label="今天已練習"
          size="xl"
          class="w-full justify-center bg-paper-primary text-paper-bg hover:bg-paper-accent"
          style="height: 60px"
          @click="handleReviewed"
        />
      </div>

      <div v-else class="max-w-2xl mx-auto px-6 pt-16 text-center">
        <h1 class="font-display font-normal text-4xl mb-2 text-paper-fg">
          {{ mode === "review" ? "今天的複習都完成了 🎉" : "這批新字都學完了" }}
        </h1>
        <p class="text-paper-muted mb-10">可以回書架看看別的練習方式,或明天再回來複習。</p>
        <UButton
          label="返回書架"
          class="bg-paper-primary text-paper-bg hover:bg-paper-accent"
          @click="navigateTo('/practice')"
        />
      </div>
      </ClientOnly>
    </template>

    <UModal
      v-model:open="isLevel5ModalOpen"
      title="這張卡你已經很熟了！"
      :description="`「${currentWord?.term ?? ''}」已經連續答對很多次,代表你已經記得很熟了。要把它畢業封存,還是重新從頭學一次？`"
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
      <template #footer>
        <div class="flex flex-col gap-2 w-full">
          <UButton
            label="不再顯示(畢業封存)"
            size="xl"
            class="justify-center bg-paper-primary text-paper-bg hover:bg-paper-accent"
            @click="resolveLevel5('graduate')"
          />
          <UButton
            label="重新學習(打回 Lv1)"
            color="neutral"
            variant="ghost"
            class="justify-center text-paper-muted"
            @click="resolveLevel5('restart')"
          />
        </div>
      </template>
    </UModal>
  </div>
</template>
