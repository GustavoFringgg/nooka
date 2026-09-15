<script setup lang="ts">
interface Slide {
  key: string
  title: string
  desc: string
  icon: string
}

const googleBtnContainer = ref<HTMLElement | null>(null)
const config = useRuntimeConfig()

let googlePoll: ReturnType<typeof setInterval> | null = null

const handleCredentialResponse = (response: { credential: string }) => {
  console.log("Google credential:", response.credential)
}

const renderGoogleButton = () => {
  if (!window.google || !googleBtnContainer.value) return

  window.google.accounts.id.initialize({
    client_id: config.public.googleClientId,
    callback: handleCredentialResponse
  })
  window.google.accounts.id.renderButton(googleBtnContainer.value, {
    theme: "outline",
    size: "large",
    shape: "pill",
    width: 320
  })
}

const slides: Slide[] = [
  {
    key: "cards",
    title: "單字卡",
    desc: "翻卡認字,靠印象快速過一輪。",
    icon: '<svg width="34" height="34" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.75" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="6" width="14" height="16" rx="2" transform="rotate(-8 10 14)"/><rect x="7" y="4" width="14" height="16" rx="2"/></svg>'
  },
  {
    key: "check",
    title: "選擇題",
    desc: "四選一練習,答錯馬上看詳解。",
    icon: '<svg width="34" height="34" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.75" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><path d="m9 12 2 2 4-4"/></svg>'
  },
  {
    key: "headphones",
    title: "聽寫",
    desc: "聽發音拼出單字,練聽力也練拼字。",
    icon: '<svg width="34" height="34" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.75" stroke-linecap="round" stroke-linejoin="round"><path d="M3 14h3a1 1 0 0 1 1 1v4a1 1 0 0 1-1 1H4a1 1 0 0 1-1-1v-5a9 9 0 1 1 18 0v5a1 1 0 0 1-1 1h-2a1 1 0 0 1-1-1v-4a1 1 0 0 1 1-1h3"/></svg>'
  }
]

const activeIndex = ref(0)
let slideTimer: ReturnType<typeof setInterval> | null = null

onMounted(() => {
  slideTimer = setInterval(() => {
    activeIndex.value = (activeIndex.value + 1) % slides.length
  }, 2800)
  if (window.google) {
    renderGoogleButton()
    return
  }
  googlePoll = setInterval(() => {
    if (window.google) {
      clearInterval(googlePoll!)
      renderGoogleButton()
    }
  }, 100)
})

onUnmounted(() => {
  if (slideTimer !== null) clearInterval(slideTimer)
  if (googlePoll !== null) clearInterval(googlePoll)
})

declare global {
  interface Window {
    google?: {
      accounts: {
        id: {
          initialize: (config: { client_id: string; callback: (response: { credential: string }) => void }) => void
          renderButton: (parent: HTMLElement, options: Record<string, unknown>) => void
        }
      }
    }
  }
}
</script>

<template>
  <div class="min-h-screen grid grid-cols-1 md:grid-cols-2 bg-paper-bg font-body" style="color-scheme: light">
    <div class="hidden md:flex relative items-center justify-center overflow-hidden bg-paper-accent/12 p-8">
      <div class="absolute w-[520px] h-[520px] rounded-full bg-paper-accent/20 -top-35 -left-35" aria-hidden="true" />
      <div class="absolute w-90 h-90 rounded-full bg-paper-primary/15 -bottom-25 -right-20" aria-hidden="true" />

      <div class="relative w-full max-w-105 h-105">
        <div
          v-for="(slide, idx) in slides"
          :key="slide.key"
          class="absolute inset-0 flex flex-col items-center justify-center gap-5 transition-opacity duration-500 ease-in-out"
          :class="idx === activeIndex ? 'opacity-100' : 'opacity-0'"
        >
          <div
            class="w-22 h-22 rounded-full bg-paper-bg text-paper-accent flex items-center justify-center shadow-[0_20px_40px_-26px_rgba(43,42,37,0.4)]"
            v-html="slide.icon"
          />
          <div class="rounded-2xl bg-paper-bg shadow-[0_20px_40px_-26px_rgba(43,42,37,0.4)] w-70 px-6 py-5 text-center">
            <div class="font-display text-2xl text-paper-fg">{{ slide.title }}</div>
            <div class="text-paper-muted text-sm mt-2">{{ slide.desc }}</div>
          </div>
          <div class="flex gap-2">
            <div
              v-for="(dot, dotIdx) in slides"
              :key="dot.key"
              class="w-2 h-2 rounded-full"
              :class="dotIdx === activeIndex ? 'bg-paper-accent' : 'bg-paper-accent/30'"
            />
          </div>
        </div>
      </div>
    </div>

    <div class="flex items-center justify-center p-8">
      <div class="w-full max-w-sm">
        <div class="font-display italic text-2xl tracking-tight text-paper-fg mb-2">Nooka</div>
        <h1 class="font-display text-3xl text-paper-fg mb-2">歡迎回來</h1>
        <div ref="googleBtnContainer"></div>
        <p class="text-center mt-5 text-[13px] text-paper-muted leading-relaxed">
          登入即代表你同意
          <a href="#" class="text-paper-accent">服務條款</a>
          與
          <a href="#" class="text-paper-accent">隱私權政策</a>
          。
        </p>
      </div>
    </div>
  </div>
</template>
