<script setup lang="ts">
interface SparkParticle {
  angle: number
  dist: number
  delay: number
  warm: boolean
}

const SPARK_COUNT = 10

const particles: SparkParticle[] = Array.from({ length: SPARK_COUNT }, (_, i) => ({
  angle: (360 / SPARK_COUNT) * i + (Math.random() * 18 - 9),
  dist: 30 + Math.random() * 26,
  delay: Math.random() * 60,
  warm: i % 2 === 1
}))
</script>

<template>
  <span class="pointer-events-none" aria-hidden="true">
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
  width: 5px;
  height: 5px;
  border-radius: 50%;
  background: var(--color-night-accent);
  opacity: 0;
  animation: spark-fly 1s ease-out both;
  animation-delay: var(--delay, 0ms);
}

.spark-warm {
  width: 4px;
  height: 4px;
  background: var(--color-night-fg);
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
    transform: rotate(var(--angle)) translate(var(--dist), -8px) scale(0.3);
  }
}

@media (prefers-reduced-motion: reduce) {
  .spark {
    animation: none;
    opacity: 0;
  }
}
</style>
