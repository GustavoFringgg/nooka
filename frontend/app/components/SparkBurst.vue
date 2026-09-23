<script setup lang="ts">
interface SparkParticle {
  angle: number
  dist: number
  delay: number
  warm: boolean
}

type SparkSize = "sm" | "md" | "lg" | "xl"

const SIZE_PRESETS: Record<SparkSize, { count: number; distMin: number; distMax: number; scale: number }> = {
  sm: { count: 8, distMin: 18, distMax: 30, scale: 0.75 },
  md: { count: 10, distMin: 30, distMax: 56, scale: 1 },
  lg: { count: 16, distMin: 46, distMax: 80, scale: 1.25 },
  xl: { count: 30, distMin: 100, distMax: 160, scale: 1.5 }
}

const props = withDefaults(defineProps<{ size?: SparkSize }>(), { size: "md" })

const preset = computed(() => SIZE_PRESETS[props.size])

const particles = computed<SparkParticle[]>(() => {
  const { count, distMin, distMax } = preset.value
  return Array.from({ length: count }, (_, i) => ({
    angle: (360 / count) * i + (Math.random() * 18 - 9),
    dist: distMin + Math.random() * (distMax - distMin),
    delay: Math.random() * 60,
    warm: i % 2 === 1
  }))
})
</script>

<template>
  <span class="pointer-events-none" aria-hidden="true" :style="{ '--spark-scale': preset.scale }">
    <span
      v-for="(p, i) in particles"
      :key="i"
      class="spark"
      :class="{ 'spark-warm': p.warm }"
      :style="{ '--angle': `${p.angle}deg`, '--dist': `${p.dist}px`, '--delay': `${p.delay}ms` }"
    />
  </span>
</template>

<style scoped>
.spark {
  position: absolute;
  top: 0;
  left: 0;
  width: calc(5px * var(--spark-scale, 1));
  height: calc(5px * var(--spark-scale, 1));
  border-radius: 50%;
  background: var(--color-paper-accent);
  opacity: 0;
  animation: spark-fly 1s ease-out both;
  animation-delay: var(--delay, 0ms);
}

.spark-warm {
  width: calc(4px * var(--spark-scale, 1));
  height: calc(4px * var(--spark-scale, 1));
  background: var(--color-paper-primary);
}

@keyframes spark-fly {
  0% {
    opacity: 1;
    transform: rotate(var(--angle)) translateX(0) scale(1);
  }
  70% {
    opacity: 1;
  }
  100% {
    opacity: 0;
    transform: rotate(var(--angle)) translate(var(--dist), calc(var(--dist) * -0.15)) scale(0.3);
  }
}

@media (prefers-reduced-motion: reduce) {
  .spark {
    animation: none;
    opacity: 0;
  }
}
</style>
