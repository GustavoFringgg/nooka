<script setup lang="ts">
import { motion } from "motion-v"
import { gsap } from "gsap"
import { onMounted, ref } from "vue"

// ---- motion-v 版本 ----
const flippedMotion = ref(false)

// ---- gsap 版本 ----
const gsapCardRef = ref<HTMLElement | null>(null)
const gsapTest = ref<HTMLElement | null>(null)
const flippedGsap = ref(false)

function flipGsap() {
  flippedGsap.value = !flippedGsap.value
  gsap.to(gsapCardRef.value, {
    rotateY: flippedGsap.value ? 180 : 0,
    duration: 0.6
  })
}

onMounted(() => {
  gsap.to(".blueBox", {
    x: 500,
    duration: 3,
    rotation: 180
  })
})
</script>
<style>
.blueBox {
  width: 100px;
  height: 100px;
  background-color: blue;
}
</style>

<template>
  <div class="blueBox" ref="gsapTest"></div>
  <div style="display: flex; justify-content: center; gap: 80px; margin-top: 100px">
    <!-- ============ motion-v 版本 ============ -->
    <div style="text-align: center">
      <h3 style="color: white; margin-bottom: 16px">motion-v</h3>
      <div
        @click="flippedMotion = !flippedMotion"
        style="width: 200px; height: 280px; position: relative; cursor: pointer"
      >
        <motion.div
          :animate="{ rotateY: flippedMotion ? 180 : 0 }"
          :transition="{ duration: 0.6, type: 'spring' }"
          style="width: 100%; height: 100%; position: relative; transform-style: preserve-3d"
        >
          <div
            style="
              position: absolute;
              inset: 0;
              backface-visibility: hidden;
              background: #12294a;
              color: white;
              display: flex;
              align-items: center;
              justify-content: center;
              font-size: 24px;
              border-radius: 16px;
            "
          >
            正面
          </div>
          <div
            style="
              position: absolute;
              inset: 0;
              backface-visibility: hidden;
              transform: rotateY(180deg);
              background: #1c3a63;
              color: white;
              display: flex;
              align-items: center;
              justify-content: center;
              font-size: 24px;
              border-radius: 16px;
            "
          >
            背面
          </div>
        </motion.div>
      </div>
    </div>

    <!-- ============ gsap 版本 ============ -->
    <div style="text-align: center">
      <h3 style="color: white; margin-bottom: 16px">gsap</h3>
      <div
        ref="gsapCardRef"
        @click="flipGsap"
        style="width: 200px; height: 280px; position: relative; transform-style: preserve-3d; cursor: pointer"
      >
        <div
          style="
            position: absolute;
            inset: 0;
            backface-visibility: hidden;
            background: #12294a;
            color: white;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 24px;
            border-radius: 16px;
          "
        >
          正面
        </div>
        <div
          style="
            position: absolute;
            inset: 0;
            backface-visibility: hidden;
            transform: rotateY(180deg);
            background: #1c3a63;
            color: white;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 24px;
            border-radius: 16px;
          "
        >
          背面
        </div>
      </div>
    </div>
  </div>
</template>
