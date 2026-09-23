<script setup lang="ts">
type SweepDirection = "up" | "down" | "left" | "right"

const props = withDefaults(
  defineProps<{
    fill?: string
    textColor?: string
    direction?: SweepDirection
  }>(),
  {
    fill: "var(--color-paper-primary)",
    textColor: "var(--color-paper-bg)",
    direction: "up"
  }
)
</script>

<template>
  <button
    type="button"
    class="fill-button"
    :class="`fill-button--${props.direction}`"
    :style="{ '--fill-color': props.fill, '--fill-text': props.textColor }"
  >
    <span class="fill-button__label"><slot /></span>
  </button>
</template>

<style scoped>
.fill-button {
  position: relative;
  overflow: hidden;
  isolation: isolate;
  cursor: pointer;
}

.fill-button__label {
  position: relative;
  z-index: 1;
  transition: color 0.35s ease;
}

.fill-button::before {
  content: "";
  position: absolute;
  inset: 0;
  z-index: 0;
  background: var(--fill-color);
  transition: transform 0.65s cubic-bezier(0.25, 1, 0.5, 1);
}

.fill-button--up::before {
  transform: translateY(100%);
}
.fill-button--down::before {
  transform: translateY(-100%);
}
.fill-button--left::before {
  transform: translateX(100%);
}
.fill-button--right::before {
  transform: translateX(-100%);
}

.fill-button:hover::before,
.fill-button:focus-visible::before,
.fill-button:active::before {
  transform: translate(0, 0);
}

.fill-button:hover .fill-button__label,
.fill-button:focus-visible .fill-button__label,
.fill-button:active .fill-button__label {
  color: var(--fill-text);
}

@media (prefers-reduced-motion: reduce) {
  .fill-button::before,
  .fill-button__label {
    transition: none;
  }
}
</style>
