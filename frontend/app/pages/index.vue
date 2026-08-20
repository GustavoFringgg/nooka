<script setup lang="ts">
import { gsap } from "gsap"
import { ScrollTrigger } from "gsap/ScrollTrigger"
import { onMounted } from "vue"

// 字型與版面規格取自 design_handoff_velorah_hero/VelorahHero.html
useHead({
  link: [
    { rel: "preconnect", href: "https://fonts.googleapis.com" },
    {
      rel: "stylesheet",
      href: "https://fonts.googleapis.com/css2?family=Instrument+Serif:ital@0;1&family=Inter:wght@400;500&display=swap"
    }
  ]
})

gsap.registerPlugin(ScrollTrigger)

onMounted(() => {
  gsap.utils.toArray<HTMLElement>(".feature").forEach((section) => {
    gsap.from(section.querySelectorAll(".feature__icon, .feature__title, .feature__text"), {
      opacity: 0,
      y: 32,
      duration: 0.6,
      ease: "power1.out",
      stagger: 0.1,
      scrollTrigger: {
        trigger: section,
        start: "top 70%",
        toggleActions: "play none none reverse"
      }
    })
  })
})
</script>

<template>
  <div>
    <div class="relative w-full min-h-screen overflow-hidden font-body text-hero-fg bg-hero-bg">
      <video
        class="absolute inset-0 w-full h-full object-cover z-0"
        autoplay
        loop
        muted
        playsinline
        src="/herovideo2_HQ.mp4"
      />

      <AppNav />

      <section class="relative z-10 flex flex-col items-center text-center px-6 pt-32 pb-40">
        <h1
          class="animate-fade-rise font-display font-normal text-[clamp(48px,9vw,120px)] leading-[0.95] tracking-[-2.46px] max-w-7xl m-0"
        >
          在
          <span class="text-hero-muted">寂靜</span>
          中,
          <br />
          讓
          <span class="text-hero-muted">單字</span>
          生根發芽
        </h1>
        <p class="animate-fade-rise-delay text-hero-muted text-lg max-w-xl mt-8 leading-relaxed">
          佔位文字:這裡預留給 Nooka 的產品說明文案,示範版面與進場動畫用,待正式文案定稿後再替換。
        </p>
        <button
          class="liquid-glass-cta liquid-glass-hover animate-fade-rise-delay-2 rounded-full bg-transparent text-hero-fg font-body mt-12 px-14 py-5 text-base border-0"
        >
          開始學習
        </button>
      </section>
    </div>

    <section
      class="relative min-h-screen flex flex-col items-center justify-center text-center p-6 overflow-hidden bg-night-bg [&:nth-of-type(even)]:bg-night-panel"
    >
      <span
        class="absolute top-12 font-display text-[clamp(80px,14vw,200px)] leading-none text-white/4 pointer-events-none select-none"
      >
        01
      </span>
      <div class="w-14 h-14 rounded-2xl bg-night-accent mb-8" />
      <h2 class="relative font-display font-normal text-[clamp(36px,6vw,64px)] text-night-fg mb-5">分類</h2>
      <p class="relative max-w-[480px] text-night-muted text-base leading-[1.7]">
        佔位文字:依主題整理的單字書架,例如航空、工程、資安、多益。
      </p>
    </section>

    <section
      class="relative min-h-screen flex flex-col items-center justify-center text-center p-6 overflow-hidden bg-night-bg [&:nth-of-type(even)]:bg-night-panel"
    >
      <span
        class="absolute top-12 font-display text-[clamp(80px,14vw,200px)] leading-none text-white/4 pointer-events-none select-none"
      >
        02
      </span>
      <div class="w-14 h-14 rounded-2xl bg-night-accent mb-8" />
      <h2 class="relative font-display font-normal text-[clamp(36px,6vw,64px)] text-night-fg mb-5">練習模式</h2>
      <p class="relative max-w-[480px] text-night-muted text-base leading-[1.7]">
        佔位文字:選擇題、消消樂、打字拼寫等多種練習方式。
      </p>
    </section>

    <section
      class="relative min-h-screen flex flex-col items-center justify-center text-center p-6 overflow-hidden bg-night-bg [&:nth-of-type(even)]:bg-night-panel"
    >
      <span
        class="absolute top-12 font-display text-[clamp(80px,14vw,200px)] leading-none text-white/4 pointer-events-none select-none"
      >
        03
      </span>
      <div class="w-14 h-14 rounded-2xl bg-night-accent mb-8" />
      <h2 class="relative font-display font-normal text-[clamp(36px,6vw,64px)] text-night-fg mb-5">學習紀錄</h2>
      <p class="relative max-w-[480px] text-night-muted text-base leading-[1.7]">
        佔位文字:採用 SM-2 間隔重複,依答對狀況安排下次複習時間。
      </p>
    </section>
  </div>
</template>
