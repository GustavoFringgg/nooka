<script setup lang="ts">
import { gsap } from "gsap"
import { ScrollTrigger } from "gsap/ScrollTrigger"
import { MotionPathPlugin } from "gsap/MotionPathPlugin"
gsap.registerPlugin(ScrollTrigger, MotionPathPlugin)

// 版面規格取自 claude-design/Nooka 首頁設計.dc.html
useHead({
  link: [
    { rel: "preconnect", href: "https://fonts.googleapis.com" },
    {
      rel: "stylesheet",
      href: "https://fonts.googleapis.com/css2?family=Newreader:ital,wght@0,400;0,500;1,400;1,500&family=Work+Sans:wght@400;500;600&family=Noto+Sans+TC:wght@400;500;700&display=swap"
    }
  ]
})

const seedWords = [
  { word: "vicarious", meaning: "替代的感受" },
  { word: "haltingly", meaning: "吞吞吐吐地" },
  { word: "hectic", meaning: "忙亂的" },
  { word: "hub", meaning: "中心樞紐" },
  { word: "hysterical", meaning: "歇斯底里的" },
  { word: "handout", meaning: "講義;施捨" }
]

const wordIndex = ref(0)
let carouselTimer: ReturnType<typeof setInterval> | undefined

onMounted(() => {
  carouselTimer = setInterval(() => {
    wordIndex.value = (wordIndex.value + 1) % seedWords.length
  }, 2600)
})

onUnmounted(() => {
  if (carouselTimer) clearInterval(carouselTimer)
})

const shelfBooks = [
  { label: "多益", from: "#2c433a", to: "#3C5A44", fg: "#f2f5f0", width: 60, height: 210 },
  { label: "航空", from: "#8f4128", to: "#C1653B", fg: "#fbeee7", width: 52, height: 180 },
  { label: "工程", from: "#4c4640", to: "#6B6558", fg: "#f5f3ef", width: 64, height: 230 },
  { label: "資安", from: "#2c433a", to: "#3C5A44", fg: "#f2f5f0", width: 56, height: 196 },
  { label: "商務", from: "#8f4128", to: "#C1653B", fg: "#fbeee7", width: 68, height: 222 }
]
const shelfLoop = [...shelfBooks, ...shelfBooks]

const heatCellClass = ["bg-paper-fg/16", "bg-paper-primary/28", "bg-paper-primary/52", "bg-paper-primary/78", "bg-paper-primary"]
const streakHeatmap = [
  [0, 0, 1, 0, 0, 0, 1],
  [0, 1, 0, 0, 0, 1, 0],
  [1, 0, 0, 1, 0, 0, 1],
  [0, 1, 1, 0, 0, 1, 0],
  [0, 0, 1, 1, 0, 1, 1],
  [1, 1, 0, 1, 1, 0, 1],
  [1, 0, 2, 1, 1, 2, 1],
  [1, 2, 1, 1, 2, 1, 2],
  [2, 1, 2, 2, 1, 3, 2],
  [2, 3, 2, 1, 3, 2, 3],
  [2, 3, 2, 4, 3, 2, 4],
  [3, 4, 3, 4, 4, 3, 4],
  [3, 4, 4, 3, 4, 4, 3],
  [4, 3, 4, 4, 4, 4, 4],
  [4, 4, 4, 3, 4, 4, 4],
  [4, 4, 4, 4, 4, 3, 4]
]

const miniBarDays = [
  { choice: 2, typing: 1 },
  { choice: 0, typing: 1 },
  { choice: 3, typing: 0 },
  { choice: 1, typing: 2 },
  { choice: 2, typing: 2 },
  { choice: 4, typing: 1 },
  { choice: 3, typing: 3 }
]
const miniBarMax = Math.max(...miniBarDays.flatMap((d) => [d.choice, d.typing]))

const goPractice = () => navigateTo("/practice")

const practiceSection = ref<HTMLElement | null>(null)
let practiceCtx: gsap.Context | undefined

onMounted(() => {
  if (!practiceSection.value) return
  practiceCtx = gsap.context(() => {
    gsap.set(".practice-card", { opacity: 0, y: 24 })
    ScrollTrigger.batch(".practice-card", {
      start: "top 85%",
      once: true,
      onEnter: (elements) => {
        gsap.to(elements, { opacity: 1, y: 0, duration: 0.6, stagger: 0.15, ease: "power2.out" })
      }
    })
  }, practiceSection.value)

  document.fonts.ready.then(() => ScrollTrigger.refresh())
})

onUnmounted(() => {
  practiceCtx?.revert()
})

const memorySection = ref<HTMLElement | null>(null)
let memoryCtx: gsap.Context | undefined

onMounted(() => {
  if (!memorySection.value) return
  memoryCtx?.revert()
  memoryCtx = gsap.context(() => {
    gsap.to("#memoryBall", {
      motionPath: {
        path: "#memoryPath",
        align: "#memoryPath",
        alignOrigin: [0.5, 0.5]
      },
      duration: 4,
      ease: "none",
      repeat: -1,
      scrollTrigger: {
        trigger: memorySection.value,
        start: "top 85%",
        once: true
      }
    })
  }, memorySection.value)
})

onUnmounted(() => {
  memoryCtx?.revert()
})
</script>

<template>
  <div class="font-body text-paper-fg">
    <AppNav />

    <!-- 1. Hero -->
    <section class="relative overflow-hidden bg-paper-bg-alt px-6 py-6 md:px-6 md:py-14">
      <div
        class="absolute w-[620px] h-[620px] -right-40 -top-56 rounded-full pointer-events-none"
        style="background: radial-gradient(circle, rgba(193, 101, 59, 0.12), transparent 65%)"
      />

      <div class="relative max-w-7xl mx-auto grid md:grid-cols-[1.1fr_.9fr] gap-8 md:gap-16 items-center">
        <div class="animate-fade-rise">
          <div class="flex flex-wrap items-center gap-2 mb-5 md:mb-7">
            <div
              class="inline-flex items-center gap-2 px-3.5 py-1.5 rounded-full border border-paper-primary/35 text-paper-primary text-xs md:text-[13px]"
            >
              <span class="w-1.5 h-1.5 rounded-full bg-paper-primary inline-block" />
              間隔排程設計
            </div>
            <div
              class="inline-flex items-center gap-2 px-3.5 py-1.5 rounded-full border border-paper-primary/35 text-paper-primary text-xs md:text-[13px]"
            >
              <span class="w-1.5 h-1.5 rounded-full bg-paper-primary inline-block" />
              答錯更常考
            </div>
          </div>

          <h1 class="m-0 font-display font-normal text-[25px] md:text-[clamp(32px,4vw,40px)] leading-[1.08] tracking-[-1px] text-paper-fg">
            一天十分鐘，把單字
            <span class="text-paper-accent">記住</span>
          </h1>

          <div class="flex flex-wrap gap-3 md:gap-3.5 mt-6 md:mt-9">
            <button
              class="px-5 py-2.5 md:px-8 md:py-4 rounded-full bg-paper-primary text-paper-bg text-xs md:text-base font-medium cursor-pointer transition-[transform,box-shadow] duration-250 ease-out hover:-translate-y-0.5 hover:shadow-[0_14px_30px_-12px_rgba(60,90,68,.5)]"
              @click="goPractice"
            >
              開始練習
            </button>
          </div>
        </div>

        <div class="relative h-[180px] md:h-[340px] animate-fade-rise-delay">
          <div
            class="hidden md:block absolute left-1/2 bottom-0 w-[220px] h-[150px] rounded-2xl border border-paper-fg/10 bg-white/40"
            style="transform: translateX(-50%) translateX(-170px) rotate(-9deg)"
          />
          <div
            class="hidden md:block absolute left-1/2 bottom-0 w-[220px] h-[150px] rounded-2xl border border-paper-fg/10 bg-white/40"
            style="transform: translateX(-50%) translateX(170px) rotate(9deg)"
          />
          <div
            class="absolute left-1/2 bottom-2.5 -translate-x-1/2 w-[220px] h-[170px] md:w-[250px] md:h-[190px] rounded-[20px] border border-paper-fg/15 bg-white/60 backdrop-blur-[6px] shadow-[0_30px_50px_-24px_rgba(43,42,37,.35)] p-5 md:p-6 flex flex-col justify-between text-left"
          >
            <div class="text-paper-muted text-[10px] md:text-[10.5px] tracking-[.12em] uppercase">多益(600)</div>
            <div>
              <div class="font-display text-[26px] md:text-[34px] leading-[1.08] text-paper-fg">
                {{ seedWords[wordIndex]!.word }}
              </div>
              <div class="text-paper-accent text-sm mt-2">{{ seedWords[wordIndex]!.meaning }}</div>
            </div>
            <div class="flex gap-1.5">
              <span class="w-5.5 h-[3px] rounded-full bg-paper-primary" />
              <span class="w-5.5 h-[3px] rounded-full bg-paper-fg/15" />
              <span class="w-5.5 h-[3px] rounded-full bg-paper-fg/15" />
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- 2. 為什麼會記得住 -->
    <section class="bg-paper-bg py-12 md:py-20 px-6 md:px-12">
      <div class="max-w-7xl mx-auto grid md:grid-cols-[.95fr_1.05fr] gap-8 md:gap-16 items-center">
        <div>
          <h2 class="m-0 font-display font-normal text-[26px] md:text-[clamp(32px,4.5vw,40px)] text-paper-fg">人腦會忘記，那怎麼記得住?</h2>
          <p class="mt-3 md:mt-5 max-w-[460px] text-paper-muted text-sm md:text-base leading-[1.7] md:leading-[1.8]">
            Nooka 會記得你單字的熟悉程度<br />答對的字, 之後出現的頻率會慢慢降低<br />答錯的字, 則會越來越常出現
          </p>
          <div class="flex flex-col gap-2 md:gap-2.5 mt-5 md:mt-7">
            <div class="flex items-baseline gap-2 md:gap-2.5">
              <span class="w-2 h-2 rounded-full bg-paper-primary inline-block" />
              <span class="text-paper-fg text-sm md:text-[15px]">答對 → 這個字之後比較少出現</span>
            </div>
            <div class="flex items-baseline gap-2 md:gap-2.5">
              <span class="w-2 h-2 rounded-full bg-[#9c4a3c] inline-block" />
              <span class="text-paper-fg text-sm md:text-[15px]">答錯 → 這個字之後更容易出現</span>
            </div>
          </div>
        </div>

        <div ref="memorySection" class="p-5 md:p-9 rounded-3xl border border-paper-fg/15 bg-white/55 backdrop-blur-[6px]">
          <svg viewBox="0 0 560 260" class="w-full block overflow-visible">
            <line x1="0" y1="230" x2="560" y2="230" stroke="rgba(43,42,37,.15)" stroke-width="1" />
            <path
              id="memoryPath"
              d="M 0 60 Q 40 170 70 210 L 70 60 Q 120 150 160 195 L 160 40 Q 220 130 270 185 L 270 25 Q 350 110 420 175 L 420 15 Q 480 40 540 55"
              fill="none"
              stroke="#C1653B"
              stroke-width="2.5"
              stroke-linecap="round"
            />
            <circle id="memoryBall" r="6" fill="#3C5A44" />
            <text x="70" y="252" text-anchor="middle" fill="#6B6558" font-size="13" font-family="Work Sans">明天</text>
            <text x="160" y="252" text-anchor="middle" fill="#6B6558" font-size="13" font-family="Work Sans">4 天後</text>
            <text x="270" y="252" text-anchor="middle" fill="#6B6558" font-size="13" font-family="Work Sans">2 週後</text>
            <text x="420" y="252" text-anchor="middle" fill="#6B6558" font-size="13" font-family="Work Sans">1 個月後</text>
          </svg>
          <div class="mt-3 md:mt-4.5 flex flex-wrap items-center gap-x-4 gap-y-1.5 text-paper-muted text-[11px] md:text-[13px]">
            <span class="inline-flex items-center gap-2">
              <span class="w-4 h-0.5 bg-paper-accent inline-block shrink-0" />
              沒複習會遺忘的速度
            </span>
            <span class="inline-flex items-center gap-2">
              <span class="w-2 h-2 rounded-full bg-paper-primary inline-block shrink-0" />
              複習後, 記憶被重新拉高
            </span>
          </div>
        </div>
      </div>
    </section>

    <!-- 3. 先選書 -->
    <section class="bg-paper-bg-alt overflow-hidden py-12 md:py-20 px-6 md:px-12">
      <div class="max-w-7xl mx-auto grid md:grid-cols-2 gap-0 items-center">
        <div class="md:pr-16">
          <h2 class="m-0 font-display font-normal text-[26px] md:text-[clamp(32px,4.5vw,40px)] text-paper-fg">先選書,不用先想從哪開始</h2>
          <p class="mt-3 md:mt-5 max-w-[440px] text-paper-muted text-sm md:text-base leading-[1.7] md:leading-[1.8]">
            單字依主題整理成一本一本的書。要考多益就開多益,工作上要看資安文件就開資安 — 你只要挑一本,剩下的順序我們排。
          </p>
        </div>

        <div class="relative h-[180px] md:h-[260px] overflow-hidden [mask-image:linear-gradient(90deg,transparent,black_12%,black_88%,transparent)]">
          <div class="absolute top-1/2 left-0 flex items-end gap-4 md:gap-5.5 animate-shelf-scroll">
            <div
              v-for="(book, i) in shelfLoop"
              :key="i"
              class="rounded-[4px_4px_3px_3px] flex items-center justify-center"
              :style="{
                width: book.width * 0.8 + 'px',
                height: book.height * 0.8 + 'px',
                background: `linear-gradient(90deg, ${book.from}, ${book.to} 12%, ${book.to} 88%, ${book.from})`
              }"
            >
              <span class="[writing-mode:vertical-rl] font-medium tracking-[.16em] text-[13px] md:text-[15px]" :style="{ color: book.fg }">
                {{ book.label }}
              </span>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- 4. 挑練習方式 -->
    <section ref="practiceSection" class="bg-paper-bg py-12 md:py-20 px-6 md:px-12">
      <div class="max-w-7xl mx-auto">
        <div class="mb-7 md:mb-11">
          <h2 class="m-0 font-display font-normal text-[26px] md:text-[clamp(32px,4.5vw,40px)] text-paper-fg">選一種方式練習</h2>
        </div>

        <div class="grid md:grid-cols-3 gap-4 md:gap-5">
          <div
            class="practice-card p-5 md:p-7 rounded-[22px] border border-paper-fg/15 bg-white/50 backdrop-blur-[4px] transition-[transform,background-color] duration-250 ease-out hover:-translate-y-1.5 hover:bg-white/85"
          >
            <div class="w-[170px] h-[95px] md:w-[220px] md:h-[120px] mx-auto mb-4 md:mb-6 relative">
              <div
                class="absolute left-3 top-2 md:left-3.5 md:top-2.5 w-[95px] h-[70px] md:w-[120px] md:h-[90px] rounded-xl bg-paper-fg/8 -rotate-6"
              />
              <div
                class="absolute left-7 top-0 md:left-8.5 w-[95px] h-[70px] md:w-[120px] md:h-[90px] rounded-xl border border-paper-fg/15 bg-paper-bg flex items-center justify-center font-display text-lg md:text-[22px] text-paper-fg"
              >
                hub
              </div>
            </div>
            <h3 class="m-0 font-display font-normal text-xl md:text-2xl text-paper-fg text-center">單字卡</h3>
            <p class="mt-2 md:mt-2.5 text-paper-muted text-sm leading-[1.6] md:leading-[1.7] text-center">一張一張翻, 先靠印象認一輪</p>
          </div>

          <div
            class="practice-card relative p-5 md:p-7 rounded-[22px] border-[1.5px] border-paper-primary bg-white/70 backdrop-blur-[4px] transition-transform duration-250 ease-out hover:-translate-y-1.5 cursor-pointer"
            @click="goPractice"
          >
            <span
              class="absolute top-3.5 right-3.5 md:top-4.5 md:right-4.5 px-2.5 py-1 md:px-3 rounded-full bg-paper-primary text-paper-bg text-[10.5px] md:text-[11.5px]"
            >
              推薦先玩這個
            </span>
            <div class="w-[170px] h-[95px] md:w-[220px] md:h-[120px] mx-auto mt-7 md:mt-8.5 mb-4 md:mb-6 flex flex-col gap-1.5 justify-center">
              <div class="px-3 py-1.5 md:px-3.5 md:py-2 rounded-[10px] border border-paper-fg/15 bg-paper-bg text-paper-muted text-xs md:text-[13px]">
                忙亂的
              </div>
              <div
                class="px-3 py-1.5 md:px-3.5 md:py-2 rounded-[10px] border border-paper-accent/55 bg-paper-accent/14 text-paper-fg text-xs md:text-[13px] flex justify-between"
              >
                <span>吞吞吐吐地</span>
                <span class="text-paper-accent">✓</span>
              </div>
              <div class="px-3 py-1.5 md:px-3.5 md:py-2 rounded-[10px] border border-paper-fg/15 bg-paper-bg text-paper-muted text-xs md:text-[13px]">
                中心樞紐
              </div>
            </div>
            <h3 class="m-0 font-display font-normal text-xl md:text-2xl text-paper-fg text-center">選擇題</h3>
            <p class="mt-2 md:mt-2.5 text-paper-muted text-sm leading-[1.6] md:leading-[1.7] text-center">四選一, 答錯馬上看正解和例句</p>
          </div>

          <div
            class="practice-card p-5 md:p-7 rounded-[22px] border border-paper-fg/15 bg-white/50 backdrop-blur-[4px] transition-[transform,background-color] duration-250 ease-out hover:-translate-y-1.5 hover:bg-white/85"
          >
            <div class="w-[170px] h-[95px] md:w-[220px] md:h-[120px] mx-auto mb-4 md:mb-6 flex items-center justify-center gap-2">
              <div
                class="w-11 h-[55px] md:w-14 md:h-[70px] rounded-lg border-[1.5px] border-dashed border-paper-fg/25 flex items-center justify-center text-paper-muted text-xs md:text-[13px] font-mono"
              >
                h_b
              </div>
              <span class="text-paper-muted text-base md:text-lg">→</span>
              <div
                class="w-11 h-[55px] md:w-14 md:h-[70px] rounded-lg border-[1.5px] border-dashed border-paper-fg/25 flex items-center justify-center text-paper-muted text-xs md:text-[13px] font-mono"
              >
                hub
              </div>
            </div>
            <h3 class="m-0 font-display font-normal text-xl md:text-2xl text-paper-fg text-center">打字拼寫</h3>
            <p class="mt-2 md:mt-2.5 text-paper-muted text-sm leading-[1.6] md:leading-[1.7] text-center">看中文, 把單字拼出來</p>
          </div>
        </div>
      </div>
    </section>

    <!-- 5. 答錯詳解 -->
    <section class="bg-paper-bg-alt py-12 md:py-20 px-6 md:px-12">
      <div class="max-w-7xl mx-auto grid md:grid-cols-[1fr_.9fr] gap-8 md:gap-16 items-center">
        <div>
          <h2 class="m-0 font-display font-normal text-[26px] md:text-[clamp(32px,4.5vw,40px)] text-paper-fg">答錯才是重點</h2>
          <p class="mt-3 md:mt-5 max-w-[460px] text-paper-muted text-sm md:text-base leading-[1.7] md:leading-[1.8]">
            選錯我們把答案和正解並排攤開 — 詞性、定義、例句都在,讓你看懂差在哪
          </p>

          <p class="mt-3 md:mt-5 max-w-[460px] text-paper-muted text-sm md:text-base leading-[1.0] md:leading-[1.8]">
            錯過的字,之後會更常出現,直到你記住
          </p>
          <div class="flex flex-wrap gap-2 md:gap-2.5 mt-5 md:mt-7">
            <span class="px-3.5 py-1.5 md:px-4 md:py-2 rounded-full border border-paper-fg/20 text-paper-fg text-xs md:text-sm">單字卡</span>
            <span class="px-3.5 py-1.5 md:px-4 md:py-2 rounded-full border border-paper-fg/12 text-paper-muted text-xs md:text-sm"> 選擇題</span>
            <span class="px-3.5 py-1.5 md:px-4 md:py-2 rounded-full border border-paper-fg/12 text-paper-muted text-xs md:text-sm"> 打字拼寫</span>
          </div>
        </div>

        <div class="flex flex-col gap-3 md:gap-3.5">
          <div class="p-5 md:p-6 rounded-[18px] border border-[#9c4a3c]/40 bg-[#9c4a3c]/12">
            <div class="flex items-center justify-between mb-2 md:mb-2.5">
              <span class="text-[#9c4a3c] text-xs tracking-[.12em] uppercase">你選的 · 錯</span>
              <span class="text-[#9c4a3c] text-sm md:text-base">✕</span>
            </div>
            <div class="font-display text-[26px] md:text-[32px] text-paper-fg">
              hectic
              <span class="font-body text-sm md:text-[15px] text-paper-muted">adj.</span>
            </div>
            <div class="text-paper-fg text-sm md:text-[15px] mt-1.5">忙亂的</div>
            <div class="flex items-start gap-2 text-paper-muted text-xs md:text-[13.5px] mt-2 md:mt-2.5 leading-[1.6] md:leading-[1.7]">
              <span class="shrink-0 text-[10px] text-[#dc2626] font-medium uppercase tracking-[.05em] mt-0.5">英義</span>
              <span>Full of incessant or frantic activity.</span>
            </div>
            <div class="flex items-start gap-2 text-paper-muted text-xs md:text-[13px] mt-2 leading-[1.6]">
              <span class="shrink-0 text-[10px] text-[#dc2626] font-medium uppercase tracking-[.05em] mt-0.5">例句</span>
              <span class="italic">"It's been a hectic week at the office."</span>
            </div>
          </div>

          <div class="p-5 md:p-6 rounded-[18px] border border-paper-primary/45 bg-paper-primary/10">
            <div class="flex items-center justify-between mb-2 md:mb-2.5">
              <span class="text-paper-primary text-xs tracking-[.12em] uppercase">正解</span>
              <span class="text-paper-primary text-sm md:text-base">∨</span>
            </div>
            <div class="font-display text-[26px] md:text-[32px] text-paper-fg">
              haltingly
              <span class="font-body text-sm md:text-[15px] text-paper-muted">adv.</span>
            </div>
            <div class="text-paper-fg text-sm md:text-[15px] mt-1.5">吞吞吐吐地</div>
            <div class="flex items-start gap-2 text-paper-muted text-xs md:text-[13.5px] mt-2 md:mt-2.5 leading-[1.6] md:leading-[1.7]">
              <span class="shrink-0 text-[10px] text-[#dc2626] font-medium uppercase tracking-[.05em] mt-0.5">英義</span>
              <span>In a hesitant or faltering way, pausing often.</span>
            </div>
            <div class="flex items-start gap-2 text-paper-muted text-xs md:text-[13px] mt-2 leading-[1.6]">
              <span class="shrink-0 text-[10px] text-[#dc2626] font-medium uppercase tracking-[.05em] mt-0.5">例句</span>
              <span class="italic">"He explained the accident haltingly, still shaken."</span>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- 5.5 學習紀錄 -->
    <section class="bg-paper-bg py-12 md:py-20 px-6 md:px-12">
      <div class="max-w-7xl mx-auto grid md:grid-cols-[1fr_.9fr] gap-8 md:gap-16 items-center">
        <div>
          <h2 class="m-0 font-display font-normal text-[26px] md:text-[clamp(32px,4.5vw,40px)] text-paper-fg">看得見自己的累積</h2>
          <p class="mt-3 md:mt-5 max-w-[460px] text-paper-muted text-sm md:text-base leading-[1.7] md:leading-[1.8]">
            每次練習都會被記下來。連續練習了幾天、哪幾天特別認真, 一眼就明白, 登入即可開始記錄
          </p>
          <div class="flex flex-wrap gap-2 md:gap-2.5 mt-5 md:mt-7">
            <span class="px-3.5 py-1.5 md:px-4 md:py-2 rounded-full border border-paper-fg/20 text-paper-fg text-xs md:text-sm">學習長條圖</span>
            <span class="px-3.5 py-1.5 md:px-4 md:py-2 rounded-full border border-paper-fg/12 text-paper-muted text-xs md:text-sm">學習熱力圖</span>
          </div>
        </div>

        <div class="p-6 md:p-8 rounded-3xl border border-paper-fg/15 bg-white/60 backdrop-blur-[6px]">
          <div class="flex items-center justify-between mb-1">
            <span class="text-[10px] md:text-[11px] uppercase tracking-[.1em] text-paper-muted">連續練習</span>
            <span class="text-xs text-paper-primary">▲ 最長紀錄 18 天</span>
          </div>
          <div class="font-display text-[40px] md:text-[44px] leading-none text-paper-fg mb-5">
            12 <span class="text-base font-body text-paper-muted">天</span>
          </div>

          <div class="flex flex-col md:flex-row md:items-start justify-between gap-6">
            <div class="w-full md:w-auto">
              <div class="flex gap-[3px] overflow-hidden">
                <div v-for="(week, wi) in streakHeatmap" :key="wi" class="flex-1 md:flex-initial flex flex-col gap-[3px]">
                  <div
                    v-for="(level, di) in week"
                    :key="di"
                    class="w-full aspect-square md:w-3.5 md:h-3.5 md:aspect-auto rounded-[2px]"
                    :class="heatCellClass[level]"
                  />
                </div>
              </div>

              <div class="flex items-center gap-1.5 mt-3 text-[11px] text-paper-muted">
                少
                <span v-for="(cls, i) in heatCellClass" :key="i" class="w-2.5 h-2.5 rounded-[2px] inline-block" :class="cls" />
                多
              </div>
            </div>

            <div class="w-full md:flex-1 md:max-w-[180px]">
              <div class="flex items-end gap-1.5 h-[88px] md:h-[101px]">
                <div v-for="(day, i) in miniBarDays" :key="i" class="flex items-end gap-0.5 flex-1 h-full">
                  <div class="flex-1 rounded-t-sm bg-paper-primary" :style="{ height: (day.choice / miniBarMax) * 100 + '%' }" />
                  <div class="flex-1 rounded-t-sm bg-paper-accent" :style="{ height: (day.typing / miniBarMax) * 100 + '%' }" />
                </div>
              </div>

              <div class="flex items-center gap-3 mt-3 text-[11px] text-paper-muted">
                <span class="inline-flex items-center gap-1">
                  <span class="w-2 h-2 rounded-full bg-paper-primary inline-block" />
                  選擇題
                </span>
                <span class="inline-flex items-center gap-1">
                  <span class="w-2 h-2 rounded-full bg-paper-accent inline-block" />
                  打字拼寫
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- 6. CTA + footer -->
    <section class="bg-paper-bg-alt py-12 md:py-20 px-6 md:px-12 text-center">
      <h2 class="mx-auto max-w-[600px] m-0 font-display font-normal text-[24px] md:text-[clamp(28px,4vw,40px)] text-paper-fg leading-[1.25]">
        先試試看, 看看有沒有感覺
      </h2>
      <button
        class="mt-5 md:mt-6.5 inline-block px-8 py-3.5 md:px-10 md:py-4 rounded-full bg-paper-primary text-paper-bg text-sm md:text-base font-medium cursor-pointer transition-[transform,box-shadow] duration-250 ease-out hover:-translate-y-0.5 hover:shadow-[0_14px_30px_-12px_rgba(60,90,68,.5)]"
        @click="goPractice"
      >
        開始練習
      </button>

      <div class="max-w-7xl mx-auto mt-8 md:mt-11 pt-5 md:pt-6 border-t border-paper-fg/15 flex justify-between items-center">
        <span class="font-display italic text-lg md:text-xl text-paper-fg">
          Nooka
          <sup class="text-[10px] not-italic">&reg;</sup>
        </span>
        <span class="text-paper-muted text-xs md:text-[13px]">© 2026 Nooka · 用對的節奏背單字</span>
      </div>
    </section>
  </div>
</template>
