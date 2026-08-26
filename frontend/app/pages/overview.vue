<script setup lang="ts">
import { onMounted, ref } from "vue"

// 版面規格取自設計稿(Organic 設計系統:Caprasimo/Figtree + 暖橘/sage 綠色票),數值已直接寫死在下方,
// 僅供這頁的儀表板內容使用;nav 沿用首頁那份 Newreader/paper 色票
useHead({
  link: [
    { rel: "preconnect", href: "https://fonts.googleapis.com" },
    {
      rel: "stylesheet",
      href: "https://fonts.googleapis.com/css2?family=Newreader:ital,wght@0,400;0,500;1,400;1,500&family=Work+Sans:wght@400;500;600&family=Caprasimo&family=Figtree:wght@400;600;700&display=swap"
    }
  ]
})

const loggedIn = useDemoLoggedIn()

const totalWordsTarget = 1240
const totalWordsDisplay = ref(0)

onMounted(() => {
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

const totalBooks = 6

const dailyWord = "haltingly"
const dailyMeaning = "吞吞吐吐地"
const dailyPos = "adv."
const dailyQuoteEn = "He explained the accident haltingly, still shaken."
const dailyQuoteZh = "他吞吞吐吐地說明了這場事故,還處於驚嚇中。"

const userLearned = 182

const weakWords = [
  { word: "haltingly", meaning: "吞吞吐吐地", misses: 6, pct: 90 },
  { word: "vicarious", meaning: "替代的感受", misses: 5, pct: 74 },
  { word: "hectic", meaning: "忙亂的", misses: 4, pct: 60 },
  { word: "hub", meaning: "中心樞紐", misses: 3, pct: 44 },
  { word: "handout", meaning: "講義;施捨", misses: 2, pct: 28 },
]
</script>

<template>
  <div class="min-h-screen bg-[#f5ead8] font-body text-paper-fg pb-24">
    <AppNav />

    <div class="overview-content max-w-6xl mx-auto px-6 md:px-12 text-[#201e1d]">
      <div class="mt-8 mb-10">
        <span class="block text-[13px] tracking-[.06em] uppercase font-semibold text-[#8c491a] mb-2">學習儀表板</span>
        <h1 class="font-heading-organic m-0 text-[clamp(32px,4vw,48px)] text-[#201e1d]">今天也背一點單字吧</h1>
      </div>

      <div class="bento-grid grid gap-4">
        <div class="area-words relative overflow-hidden rounded-[32px] p-6.5 bg-[#fff2eb] shadow-[0_3px_10px_rgba(46,43,37,.16)]">
          <div class="absolute -right-10 -top-10 w-[140px] h-[140px] rounded-full bg-[#ffe1d0] pointer-events-none" />
          <div class="relative">
            <div class="text-[10px] uppercase tracking-[.1em] text-[#c67139]">單字庫總數</div>
            <div class="font-heading-organic text-[64px] leading-none text-[#8c491a] mt-2">
              {{ totalWordsDisplay.toLocaleString() }}
            </div>
            <div class="text-[13px] text-[#201e1d]/80 mt-2">收錄在所有主題書架裡的單字</div>
          </div>
        </div>

        <div
          class="area-books rounded-[32px] p-6.5 flex flex-col items-center justify-center text-center bg-[#f0fae1] shadow-[0_1px_2px_rgba(46,43,37,.14)]"
        >
          <div
            class="w-16 h-16 rounded-full bg-[#8fa073] text-[#f5ead8] flex items-center justify-center font-heading-organic text-[28px] mb-3"
          >
            {{ totalBooks }}
          </div>
          <div class="text-[10px] uppercase tracking-[.1em] text-[#c67139]">主題書架</div>
          <div class="text-[13px] text-[#201e1d]/80 mt-1">多益、航空、工程、資安…</div>
        </div>

        <div class="area-wordcard rounded-[32px] p-6.5 bg-[#2e2b25] text-[#f5ead8] shadow-[0_3px_10px_rgba(46,43,37,.16)]">
          <div class="text-[11px] uppercase tracking-[.1em] text-[#ffc6a5]">每日一字</div>
          <div class="font-heading-organic text-[38px] mt-2.5">{{ dailyWord }}</div>
          <div class="text-[#ffe1d0] text-[15px] mt-1.5">
            {{ dailyMeaning }} <span class="opacity-60 text-[13px]">· {{ dailyPos }}</span>
          </div>
        </div>

        <div class="area-quote rounded-[32px] p-6.5 flex flex-col justify-center bg-[#ebddc5] shadow-[0_1px_2px_rgba(46,43,37,.14)]">
          <div class="text-[10px] uppercase tracking-[.1em] text-[#c67139]">每日一句</div>
          <p class="font-heading-organic text-[22px] leading-[1.4] mt-2.5 mb-0">"{{ dailyQuoteEn }}"</p>
          <p class="text-[#645c50] text-sm mt-2.5">{{ dailyQuoteZh }}</p>
        </div>

        <div
          v-if="!loggedIn"
          class="area-status rounded-[32px] p-6.5 flex flex-col items-center justify-center text-center bg-[#ebddc5] shadow-[0_1px_2px_rgba(46,43,37,.14)]"
        >
          <h3 class="font-heading-organic text-[22px] m-0">看看你的學習狀況</h3>
          <p class="text-[13px] text-[#201e1d]/80 mt-2 mb-4">登入後可看已學單字量與最容易忘記的字。</p>
          <button
            class="font-heading-organic px-4 py-2 rounded-full bg-[#c67139] text-[#f5ead8] text-sm cursor-pointer transition-colors duration-200 ease-out hover:bg-[#b2622d]"
            @click="loggedIn = true"
          >
            登入 / 註冊
          </button>
        </div>
        <div
          v-else
          class="area-status rounded-[32px] p-6.5 flex flex-col items-center justify-center text-center bg-[#f0fae1] shadow-[0_1px_2px_rgba(46,43,37,.14)]"
        >
          <div class="text-[10px] uppercase tracking-[.1em] text-[#c67139]">你已經學了</div>
          <div class="font-heading-organic text-[52px] leading-none text-[#56633f] mt-1.5">{{ userLearned }}</div>
          <div class="text-[13px] text-[#201e1d]/80 mt-1.5">個單字,持續累積中</div>
        </div>

        <div
          v-if="!loggedIn"
          class="area-weak rounded-[32px] p-6.5 bg-[#ebddc5] shadow-[0_1px_2px_rgba(46,43,37,.14)] opacity-50 blur-[1.5px] pointer-events-none"
        >
          <div class="text-[10px] uppercase tracking-[.1em] text-[#c67139]">最容易忘記的 5 個字</div>
          <div class="flex flex-col gap-2.5 mt-3">
            <div v-for="w in weakWords" :key="w.word" class="flex items-center gap-3.5">
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
          <div class="flex flex-col gap-2.5 mt-3">
            <div v-for="w in weakWords" :key="w.word" class="flex items-center gap-3.5">
              <span class="font-heading-organic text-lg w-[110px]">{{ w.word }}</span>
              <span class="text-[#645c50] text-sm w-[90px]">{{ w.meaning }}</span>
              <div class="flex-1 h-1.5 rounded-full bg-[#eee7db]">
                <div class="h-full rounded-full bg-[#c67139]" :style="{ width: w.pct + '%' }" />
              </div>
              <span class="px-2.5 py-0.5 rounded-full border border-[#c67139] text-[#c67139] text-xs shrink-0">
                錯 {{ w.misses }} 次
              </span>
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
