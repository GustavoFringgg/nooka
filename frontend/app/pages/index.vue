<script setup lang="ts">
import { gsap } from "gsap";
import { ScrollTrigger } from "gsap/ScrollTrigger";
import { onMounted } from "vue";

// 字型與版面規格取自 design_handoff_velorah_hero/VelorahHero.html
useHead({
  link: [
    { rel: "preconnect", href: "https://fonts.googleapis.com" },
    {
      rel: "stylesheet",
      href: "https://fonts.googleapis.com/css2?family=Instrument+Serif:ital@0;1&family=Inter:wght@400;500&display=swap",
    },
  ],
});

gsap.registerPlugin(ScrollTrigger);

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
        toggleActions: "play none none reverse",
      },
    });
  });
});
</script>

<template>
  <div>
    <div class="hero">
      <video class="hero__video" autoplay loop muted playsinline src="/hero-video.mp4" />

      <nav class="hero__nav">
        <div class="hero__logo">Nooka<sup>&reg;</sup></div>

        <div class="hero__navlinks">
          <a href="#" class="hero__navlink hero__navlink--active">首頁</a>
          <a href="#" class="hero__navlink">分類</a>
          <a href="#" class="hero__navlink">練習</a>
          <a href="#" class="hero__navlink">學習紀錄</a>
          <a href="#" class="hero__navlink">登入</a>
        </div>

        <button class="hero__cta hero__cta--nav">開始學習</button>
      </nav>

      <section class="hero__content">
        <h1 class="hero__title animate-fade-rise">
          在<span class="hero__muted">寂靜</span>中,<br />
          讓<span class="hero__muted">單字</span>生根發芽
        </h1>
        <p class="hero__subtext animate-fade-rise-delay">
          佔位文字:這裡預留給 Nooka 的產品說明文案,示範版面與進場動畫用,待正式文案定稿後再替換。
        </p>
        <button class="hero__cta hero__cta--hero animate-fade-rise-delay-2">開始學習</button>
      </section>
    </div>

    <section class="feature">
      <span class="feature__index">01</span>
      <div class="feature__icon" />
      <h2 class="feature__title">分類</h2>
      <p class="feature__text">佔位文字:依主題整理的單字書架,例如航空、工程、資安、多益。</p>
    </section>

    <section class="feature">
      <span class="feature__index">02</span>
      <div class="feature__icon" />
      <h2 class="feature__title">練習模式</h2>
      <p class="feature__text">佔位文字:選擇題、消消樂、打字拼寫等多種練習方式。</p>
    </section>

    <section class="feature">
      <span class="feature__index">03</span>
      <div class="feature__icon" />
      <h2 class="feature__title">學習紀錄</h2>
      <p class="feature__text">佔位文字:採用 SM-2 間隔重複,依答對狀況安排下次複習時間。</p>
    </section>
  </div>
</template>

<style lang="scss" scoped>
@use "../assets/scss/tokens" as *;
@use "../assets/scss/liquid-glass" as *;

.hero {
  position: relative;
  width: 100%;
  min-height: 100vh;
  overflow: hidden;
  font-family: $font-body;
  color: $color-foreground;
  background: $color-background;

  &__video {
    position: absolute;
    inset: 0;
    width: 100%;
    height: 100%;
    object-fit: cover;
    z-index: 0;
  }

  &__nav {
    position: relative;
    z-index: 10;
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 24px 32px;
    max-width: 1280px;
    margin: 0 auto;
  }

  &__logo {
    font-family: $font-display;
    font-size: 30px;
    letter-spacing: -0.02em;
    color: $color-foreground;

    sup {
      font-size: 12px;
    }
  }

  &__navlinks {
    display: none;
    gap: 32px;
    align-items: center;

    @media (min-width: 768px) {
      display: flex;
    }
  }

  &__navlink {
    font-size: 14px;
    color: $color-muted-foreground;
    text-decoration: none;
    transition: color 0.2s ease;

    &:hover {
      color: $color-foreground;
    }

    &--active {
      color: $color-foreground;
    }
  }

  &__cta {
    @include liquid-glass(0.01, 0.1, 0.45, 0.15);
    @include liquid-glass-hover;
    border-radius: 999px;
    color: $color-foreground;
    background-color: transparent;
    font-family: inherit;

    &--nav {
      padding: 10px 24px;
      font-size: 14px;
    }

    &--hero {
      padding: 20px 56px;
      font-size: 16px;
      margin-top: 48px;
    }
  }

  &__content {
    position: relative;
    z-index: 10;
    display: flex;
    flex-direction: column;
    align-items: center;
    text-align: center;
    padding: 128px 24px 160px;
  }

  &__title {
    font-family: $font-display;
    font-weight: 400;
    font-size: clamp(48px, 9vw, 120px);
    line-height: 0.95;
    letter-spacing: -2.46px;
    max-width: 1280px;
    margin: 0;
  }

  &__muted {
    color: $color-muted-foreground;
  }

  &__subtext {
    color: $color-muted-foreground;
    font-size: 18px;
    max-width: 640px;
    margin-top: 32px;
    line-height: 1.6;
  }
}

.feature {
  position: relative;
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  text-align: center;
  padding: 24px;
  background: $night-bg;
  overflow: hidden;

  &:nth-of-type(even) {
    background: $night-bg-panel;
  }

  &__index {
    position: absolute;
    top: 48px;
    font-family: $font-display;
    font-size: clamp(80px, 14vw, 200px);
    line-height: 1;
    color: rgba(255, 255, 255, 0.04);
    pointer-events: none;
    user-select: none;
  }

  &__icon {
    width: 56px;
    height: 56px;
    border-radius: 16px;
    background: $night-accent;
    margin-bottom: 32px;
  }

  &__title {
    position: relative;
    font-family: $font-display;
    font-weight: 400;
    font-size: clamp(36px, 6vw, 64px);
    color: $night-foreground;
    margin: 0 0 20px;
  }

  &__text {
    position: relative;
    max-width: 480px;
    color: $night-muted-foreground;
    font-size: 16px;
    line-height: 1.7;
  }
}

@keyframes fade-rise {
  from {
    opacity: 0;
    transform: translateY(24px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.animate-fade-rise {
  animation: fade-rise 0.8s ease-out both;
}

.animate-fade-rise-delay {
  animation: fade-rise 0.8s ease-out 0.2s both;
}

.animate-fade-rise-delay-2 {
  animation: fade-rise 0.8s ease-out 0.4s both;
}
</style>
