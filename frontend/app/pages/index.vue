<script setup lang="ts">
import { onMounted, onUnmounted, ref } from "vue"

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

const goPractice = () => navigateTo("/practice")
</script>

<template>
  <div class="font-body text-paper-fg">
    <AppNav />

    <!-- 1. Hero -->
    <section class="relative overflow-hidden bg-paper-bg px-6 md:px-12 py-20 md:py-24">
      <div
        class="absolute w-[620px] h-[620px] -right-40 -top-56 rounded-full pointer-events-none"
        style="background: radial-gradient(circle, rgba(193, 101, 59, 0.12), transparent 65%)"
      />

      <div class="relative max-w-7xl mx-auto grid md:grid-cols-[1.1fr_.9fr] gap-16 items-center">
        <div class="animate-fade-rise">
          <div
            class="inline-flex items-center gap-2 px-3.5 py-1.5 rounded-full border border-paper-primary/35 text-paper-primary text-[13px] mb-7"
          >
            <span class="w-1.5 h-1.5 rounded-full bg-paper-primary inline-block" />
            間隔重複排程,記了就不容易忘
          </div>

          <h1
            class="m-0 font-display font-normal text-[clamp(40px,6vw,76px)] leading-[1.08] tracking-[-1px] text-paper-fg"
          >
            一天十分鐘,
            <br />
            把單字
            <span class="italic text-paper-accent">真的</span>
            記住
          </h1>

          <p class="mt-6 max-w-[440px] text-paper-muted text-[17px] leading-[1.75]">
            挑一本你需要的單字書 — 多益、航空、資安 —
            用選擇題快速練過一輪。答錯的會被排到更近的時間再問你一次,答對的就往後放。
          </p>

          <div class="flex flex-wrap gap-3.5 mt-9">
            <button
              class="px-8 py-4 rounded-full bg-paper-primary text-paper-bg text-base font-medium cursor-pointer transition-[transform,box-shadow] duration-250 ease-out hover:-translate-y-0.5 hover:shadow-[0_14px_30px_-12px_rgba(60,90,68,.5)]"
              @click="goPractice"
            >
              免費開始練習
            </button>
            <button
              class="px-7 py-4 rounded-full border border-paper-fg/25 bg-white/50 text-paper-fg text-base cursor-pointer transition-colors duration-250 ease-out hover:bg-white/85"
            >
              看看怎麼運作
            </button>
          </div>

          <p class="mt-6 text-paper-muted text-[13px]">不用先註冊,直接開始練 · 六個主題書架持續增加中</p>
        </div>

        <div class="relative h-[340px] animate-fade-rise-delay">
          <div
            class="absolute left-1/2 bottom-0 w-[220px] h-[150px] rounded-2xl border border-paper-fg/10 bg-white/40"
            style="transform: translateX(-50%) translateX(-170px) rotate(-9deg)"
          />
          <div
            class="absolute left-1/2 bottom-0 w-[220px] h-[150px] rounded-2xl border border-paper-fg/10 bg-white/40"
            style="transform: translateX(-50%) translateX(170px) rotate(9deg)"
          />
          <div
            class="absolute left-1/2 bottom-2.5 -translate-x-1/2 w-[250px] h-[190px] rounded-[20px] border border-paper-fg/15 bg-white/60 backdrop-blur-[6px] shadow-[0_30px_50px_-24px_rgba(43,42,37,.35)] p-6 flex flex-col justify-between text-left"
          >
            <div class="text-paper-muted text-[10.5px] tracking-[.12em] uppercase">多益(600)</div>
            <div>
              <div class="font-display text-[34px] leading-[1.08] text-paper-fg">{{ seedWords[wordIndex]!.word }}</div>
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

    <!-- 2. 先選書 -->
    <section class="bg-paper-bg-alt overflow-hidden py-20">
      <div class="max-w-7xl mx-auto grid md:grid-cols-2 gap-0 items-center px-6 md:pl-12">
        <div class="md:pr-16">
          <h2 class="m-0 font-display font-normal text-[clamp(32px,4.5vw,52px)] text-paper-fg">
            先選書,不用先想從哪開始
          </h2>
          <p class="mt-5 max-w-[440px] text-paper-muted text-base leading-[1.8]">
            單字依主題整理成一本一本的書。要考多益就開多益,工作上要看資安文件就開資安 — 你只要挑一本,剩下的順序我們排。
          </p>
        </div>

        <div
          class="relative h-[260px] overflow-hidden [mask-image:linear-gradient(90deg,transparent,black_12%,black_88%,transparent)]"
        >
          <div class="absolute top-1/2 left-0 flex items-end gap-5.5 animate-shelf-scroll">
            <div
              v-for="(book, i) in shelfLoop"
              :key="i"
              class="rounded-[4px_4px_3px_3px] flex items-center justify-center"
              :style="{
                width: book.width + 'px',
                height: book.height + 'px',
                background: `linear-gradient(90deg, ${book.from}, ${book.to} 12%, ${book.to} 88%, ${book.from})`
              }"
            >
              <span
                class="[writing-mode:vertical-rl] font-medium tracking-[.16em] text-[15px]"
                :style="{ color: book.fg }"
              >
                {{ book.label }}
              </span>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- 3. 挑練習方式 -->
    <section class="bg-paper-bg py-20 px-6 md:px-12">
      <div class="max-w-7xl mx-auto">
        <div class="mb-11">
          <h2 class="m-0 font-display font-normal text-[clamp(32px,4.5vw,52px)] text-paper-fg">挑一種方式開始練</h2>
          <p class="mt-4 max-w-[480px] text-paper-muted text-base leading-[1.8]">
            同一本書,可以用不同方式複習。選擇題已經可以直接玩,其他兩種正在做。
          </p>
        </div>

        <div class="grid md:grid-cols-3 gap-5">
          <div
            class="p-7 rounded-[22px] border border-paper-fg/15 bg-white/50 backdrop-blur-[4px] transition-[transform,background-color] duration-250 ease-out hover:-translate-y-1.5 hover:bg-white/85"
          >
            <div class="w-[220px] h-[120px] mx-auto mb-6 relative">
              <div class="absolute left-3.5 top-2.5 w-[120px] h-[90px] rounded-xl bg-paper-fg/8 -rotate-6" />
              <div
                class="absolute left-8.5 top-0 w-[120px] h-[90px] rounded-xl border border-paper-fg/15 bg-paper-bg flex items-center justify-center font-display text-[22px] text-paper-fg"
              >
                hub
              </div>
            </div>
            <h3 class="m-0 font-display font-normal text-2xl text-paper-fg text-center">單字卡練習</h3>
            <p class="mt-2.5 text-paper-muted text-sm leading-[1.7] text-center">一張一張翻,先靠印象認一輪。</p>
          </div>

          <div
            class="relative p-7 rounded-[22px] border-[1.5px] border-paper-primary bg-white/70 backdrop-blur-[4px] transition-transform duration-250 ease-out hover:-translate-y-1.5 cursor-pointer"
            @click="goPractice"
          >
            <span
              class="absolute top-4.5 right-4.5 px-3 py-1 rounded-full bg-paper-primary text-paper-bg text-[11.5px]"
            >
              推薦先玩這個
            </span>
            <div class="w-[220px] h-[120px] mx-auto mt-8.5 mb-6 flex flex-col gap-1.5 justify-center">
              <div
                class="px-3.5 py-2 rounded-[10px] border border-paper-fg/15 bg-paper-bg text-paper-muted text-[13px]"
              >
                忙亂的
              </div>
              <div
                class="px-3.5 py-2 rounded-[10px] border border-paper-accent/55 bg-paper-accent/14 text-paper-fg text-[13px] flex justify-between"
              >
                <span>吞吞吐吐地</span>
                <span class="text-paper-accent">✓</span>
              </div>
              <div
                class="px-3.5 py-2 rounded-[10px] border border-paper-fg/15 bg-paper-bg text-paper-muted text-[13px]"
              >
                中心樞紐
              </div>
            </div>
            <h3 class="m-0 font-display font-normal text-2xl text-paper-fg text-center">選擇題</h3>
            <p class="mt-2.5 text-paper-muted text-sm leading-[1.7] text-center">四選一,答錯馬上看正解和例句。</p>
          </div>

          <div
            class="p-7 rounded-[22px] border border-paper-fg/15 bg-white/50 backdrop-blur-[4px] transition-[transform,background-color] duration-250 ease-out hover:-translate-y-1.5 hover:bg-white/85"
          >
            <div class="w-[220px] h-[120px] mx-auto mb-6 flex items-center justify-center gap-2">
              <div
                class="w-14 h-[70px] rounded-lg border-[1.5px] border-dashed border-paper-fg/25 flex items-center justify-center text-paper-muted text-[13px] font-mono"
              >
                h_b
              </div>
              <span class="text-paper-muted text-lg">→</span>
              <div
                class="w-14 h-[70px] rounded-lg border-[1.5px] border-dashed border-paper-fg/25 flex items-center justify-center text-paper-muted text-[13px] font-mono"
              >
                hub
              </div>
            </div>
            <h3 class="m-0 font-display font-normal text-2xl text-paper-fg text-center">打字拼寫</h3>
            <p class="mt-2.5 text-paper-muted text-sm leading-[1.7] text-center">看中文意思,自己把單字拼出來。</p>
          </div>
        </div>
      </div>
    </section>

    <!-- 4. 答錯詳解 -->
    <section class="bg-paper-bg-alt py-20 px-6 md:px-12">
      <div class="max-w-7xl mx-auto grid md:grid-cols-[.95fr_1.05fr] gap-16 items-center">
        <div class="flex flex-col gap-3.5">
          <div class="p-6 rounded-[18px] border border-[#9c4a3c]/40 bg-[#9c4a3c]/12">
            <div class="flex items-center justify-between mb-2.5">
              <span class="text-[#9c4a3c] text-xs tracking-[.12em] uppercase">你選的 · 錯</span>
              <span class="text-[#9c4a3c] text-base">✕</span>
            </div>
            <div class="font-display text-[32px] text-paper-fg">
              hectic
              <span class="font-body text-[15px] text-paper-muted">adj.</span>
            </div>
            <div class="text-paper-fg text-[15px] mt-1.5">忙亂的</div>
            <div class="text-paper-muted text-[13.5px] mt-2.5 leading-[1.7]">
              Full of incessant or frantic activity.
            </div>
            <div class="text-paper-muted text-[13px] mt-2 italic leading-[1.6]">
              "It's been a hectic week at the office."
            </div>
          </div>

          <div class="p-6 rounded-[18px] border border-paper-primary/45 bg-paper-primary/10">
            <div class="flex items-center justify-between mb-2.5">
              <span class="text-paper-primary text-xs tracking-[.12em] uppercase">正解</span>
              <span class="text-paper-primary text-base">✓</span>
            </div>
            <div class="font-display text-[32px] text-paper-fg">
              haltingly
              <span class="font-body text-[15px] text-paper-muted">adv.</span>
            </div>
            <div class="text-paper-fg text-[15px] mt-1.5">吞吞吐吐地</div>
            <div class="text-paper-muted text-[13.5px] mt-2.5 leading-[1.7]">
              In a hesitant or faltering way, pausing often.
            </div>
            <div class="text-paper-muted text-[13px] mt-2 italic leading-[1.6]">
              "He explained the accident haltingly, still shaken."
            </div>
          </div>
        </div>

        <div>
          <h2 class="m-0 font-display font-normal text-[clamp(32px,4.5vw,52px)] text-paper-fg">答錯的時候,才是重點</h2>
          <p class="mt-5 max-w-[460px] text-paper-muted text-base leading-[1.8]">
            選錯不會只給你一個叉。我們會把你選的那個字和正確答案並排攤開 — 詞性、英文定義、例句都在 —
            讓你當場看懂差在哪,而不是回頭再猜一次。
          </p>
          <div class="flex flex-wrap gap-2.5 mt-7">
            <span class="px-4 py-2 rounded-full border border-paper-fg/20 text-paper-fg text-sm">選擇題</span>
            <span class="px-4 py-2 rounded-full border border-paper-fg/12 text-paper-muted text-sm">
              消消樂 · 即將推出
            </span>
            <span class="px-4 py-2 rounded-full border border-paper-fg/12 text-paper-muted text-sm">
              打字拼寫 · 即將推出
            </span>
          </div>
        </div>
      </div>
    </section>

    <!-- 5. 為什麼會記得住 -->
    <section class="bg-paper-bg py-20 px-6 md:px-12">
      <div class="max-w-7xl mx-auto grid md:grid-cols-[.95fr_1.05fr] gap-16 items-center">
        <div>
          <h2 class="m-0 font-display font-normal text-[clamp(32px,4.5vw,52px)] text-paper-fg">為什麼會記得住?</h2>
          <p class="mt-5 max-w-[460px] text-paper-muted text-base leading-[1.8]">
            人腦本來就會忘記 — 這條線是沒有複習的話,記憶會怎麼往下掉。Nooka 用
            <span class="text-paper-fg font-medium">SM-2 間隔重複演算法</span>
            ,在你快要忘記之前叫你回來看一眼,每答對一次,下次的間隔就拉得更長;答錯,馬上排回明天。
          </p>
          <div class="flex flex-col gap-2.5 mt-7">
            <div class="flex items-baseline gap-2.5">
              <span class="w-2 h-2 rounded-full bg-paper-primary inline-block" />
              <span class="text-paper-fg text-[15px]">答對 → 間隔乘上一個係數,下次更久才問你</span>
            </div>
            <div class="flex items-baseline gap-2.5">
              <span class="w-2 h-2 rounded-full bg-[#9c4a3c] inline-block" />
              <span class="text-paper-fg text-[15px]">答錯 → 間隔重置,明天馬上再問一次</span>
            </div>
          </div>
        </div>

        <div class="p-9 rounded-3xl border border-paper-fg/15 bg-white/55 backdrop-blur-[6px]">
          <svg viewBox="0 0 560 260" class="w-full block overflow-visible">
            <line x1="0" y1="230" x2="560" y2="230" stroke="rgba(43,42,37,.15)" stroke-width="1" />
            <path
              d="M 0 60 Q 40 170 70 210 L 70 60 Q 120 150 160 195 L 160 40 Q 220 130 270 185 L 270 25 Q 350 110 420 175 L 420 15 Q 480 40 540 55"
              fill="none"
              stroke="#C1653B"
              stroke-width="2.5"
              stroke-linecap="round"
            />
            <g>
              <circle cx="70" cy="60" r="6" fill="#3C5A44" />
              <circle cx="160" cy="40" r="6" fill="#3C5A44" />
              <circle cx="270" cy="25" r="6" fill="#3C5A44" />
              <circle cx="420" cy="15" r="6" fill="#3C5A44" />
            </g>
            <text x="70" y="252" text-anchor="middle" fill="#6B6558" font-size="13" font-family="Work Sans">明天</text>
            <text x="160" y="252" text-anchor="middle" fill="#6B6558" font-size="13" font-family="Work Sans">
              4 天後
            </text>
            <text x="270" y="252" text-anchor="middle" fill="#6B6558" font-size="13" font-family="Work Sans">
              2 週後
            </text>
            <text x="420" y="252" text-anchor="middle" fill="#6B6558" font-size="13" font-family="Work Sans">
              1 個月後
            </text>
          </svg>
          <div class="mt-4.5 flex flex-wrap items-center gap-2.5 text-paper-muted text-[13px]">
            <span class="w-4 h-0.5 bg-paper-accent inline-block" />
            沒複習會忘記的速度
            <span class="w-2 h-2 rounded-full bg-paper-primary inline-block ml-4" />
            每次答對後,記憶被重新拉高
          </div>
        </div>
      </div>
    </section>

    <!-- 6. CTA + footer -->
    <section class="bg-paper-bg-alt py-20 px-6 md:px-12 pb-14 text-center">
      <h2
        class="mx-auto max-w-[600px] m-0 font-display font-normal text-[clamp(28px,4vw,42px)] text-paper-fg leading-[1.25]"
      >
        先練六個字,看看有沒有感覺
      </h2>
      <button
        class="mt-6.5 inline-block px-10 py-4 rounded-full bg-paper-primary text-paper-bg text-base font-medium cursor-pointer transition-[transform,box-shadow] duration-250 ease-out hover:-translate-y-0.5 hover:shadow-[0_14px_30px_-12px_rgba(60,90,68,.5)]"
        @click="goPractice"
      >
        免費開始練習
      </button>

      <div class="max-w-7xl mx-auto mt-11 pt-6 border-t border-paper-fg/15 flex justify-between items-center">
        <span class="font-display italic text-xl text-paper-fg">
          Nooka
          <sup class="text-[10px] not-italic">&reg;</sup>
        </span>
        <span class="text-paper-muted text-[13px]">© 2026 Nooka · 用對的節奏背單字</span>
      </div>
    </section>
  </div>
</template>
