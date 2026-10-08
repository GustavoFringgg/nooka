// Nuxt UI 元件的全域預設樣式。顏色變數在 assets/css/main.css 的 :root(對到 Paper Palette),
// 這裡只放「變數管不到」的部分:字型、遮罩顏色。
export default defineAppConfig({
  ui: {
    modal: {
      slots: {
        title: "font-display text-2xl font-normal"
      },
      variants: {
        // UModal 的遮罩底色寫在 variants.overlay 裡(優先於 slots),所以要從這裡覆蓋
        overlay: {
          true: {
            overlay: "bg-paper-fg/40"
          }
        }
      }
    },
    slideover: {
      slots: {
        overlay: "bg-paper-fg/35",
        title: "font-display italic text-xl font-normal"
      }
    }
  }
})
