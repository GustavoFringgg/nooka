<script setup lang="ts">
definePageMeta({ layout: "admin", middleware: "admin" })

// 切版階段:資料全部是假資料,按鈕只控制抽屜/彈窗的開關,還沒串 API。
// 之後串功能時:categories 改成打 GET /api/categories,儲存/刪除改打 /api/admin/categories。
interface CategoryRow {
  id: number
  name: string
  description: string
  color: string
  wordCount: number
  updatedAt: string
}

const categories = ref<CategoryRow[]>([
  {
    id: 1,
    name: "TOEIC",
    description: "多益核心單字,涵蓋職場、商務情境常見詞彙",
    color: "#b5651d",
    wordCount: 128,
    updatedAt: "2026/09/20"
  },
  {
    id: 2,
    name: "TOEIC(600)",
    description: "",
    color: "#8a7a3e",
    wordCount: 86,
    updatedAt: "2026/09/18"
  },
  { id: 3, name: "航空英文", description: "", color: "#3e6e8e", wordCount: 54, updatedAt: "2026/09/12" },
  { id: 4, name: "工程英文", description: "", color: "#6b4c6e", wordCount: 37, updatedAt: "2026/09/05" },
  { id: 5, name: "資安英文", description: "", color: "#2f6e6b", wordCount: 22, updatedAt: "2026/08/29" }
])

const swatches = ["#b5651d", "#8a7a3e", "#3e6e8e", "#6b4c6e", "#2f6e6b", "#3c5a44", "#c1653b", "#9c4a4a"]

// ---- 抽屜表單(新增跟編輯共用同一張表單)----
const isDrawerOpen = ref(false)
const editingId = ref<number | null>(null)
const form = reactive({ name: "", description: "", color: swatches[0] as string })

const drawerTitle = computed(() => (editingId.value === null ? "新增分類" : "編輯分類"))

const openCreate = () => {
  editingId.value = null
  form.name = ""
  form.description = ""
  form.color = swatches[0] as string
  isDrawerOpen.value = true
}

const openEdit = (category: CategoryRow) => {
  editingId.value = category.id
  form.name = category.name
  form.description = category.description
  form.color = category.color
  isDrawerOpen.value = true
}

// ---- 刪除確認彈窗 ----
const deleteTarget = ref<CategoryRow | null>(null)
const isDeleteModalOpen = computed({
  get: () => deleteTarget.value !== null,
  set: (open: boolean) => {
    if (!open) deleteTarget.value = null
  }
})

const modalUi = {
  content: "bg-paper-bg text-paper-fg ring-paper-fg/10 divide-paper-fg/10",
  header: "border-paper-fg/10",
  footer: "border-paper-fg/10",
  title: "text-paper-fg font-display text-2xl font-normal",
  close: "text-paper-muted hover:bg-paper-fg/10 hover:text-paper-fg",
  overlay: "bg-paper-fg/40"
}
</script>

<template>
  <div class="flex flex-col gap-6">
    <div class="flex items-end justify-between">
      <div>
        <h1 class="font-display italic text-[28px] font-normal mb-1.5">書本管理</h1>
      </div>
      <button
        type="button"
        class="rounded-full bg-paper-primary text-paper-bg px-5.5 py-2.5 text-sm font-medium cursor-pointer transition-[transform,background-color] duration-250 ease-out hover:scale-[1.03] hover:bg-paper-accent"
        @click="openCreate"
      >
        + 新增分類
      </button>
    </div>

    <div class="rounded-2xl border border-paper-fg/10 bg-paper-bg overflow-hidden">
      <div
        class="grid grid-cols-[64px_1fr_140px_160px_140px] bg-[#ebddc5] text-[11.5px] uppercase tracking-[0.06em] text-paper-muted"
      >
        <div class="px-5 py-3.5"></div>
        <div class="px-5 py-3.5">名稱</div>
        <div class="px-5 py-3.5">單字數</div>
        <div class="px-5 py-3.5">更新時間</div>
        <div class="px-5 py-3.5">操作</div>
      </div>

      <div
        v-for="category in categories"
        :key="category.id"
        class="grid grid-cols-[64px_1fr_140px_160px_140px] items-center border-t border-paper-fg/8 text-sm"
      >
        <div class="px-5 py-4">
          <span class="inline-block size-5 rounded-md" :style="{ backgroundColor: category.color }"></span>
        </div>
        <div class="px-5 py-4 font-display italic text-base">{{ category.name }}</div>
        <div class="px-5 py-4">{{ category.wordCount }}</div>
        <div class="px-5 py-4">{{ category.updatedAt }}</div>
        <div class="px-5 py-4 flex gap-3.5 text-[13px]">
          <button type="button" class="text-paper-primary cursor-pointer hover:underline" @click="openEdit(category)">
            編輯
          </button>
          <button
            type="button"
            class="text-paper-accent cursor-pointer hover:underline"
            @click="deleteTarget = category"
          >
            刪除
          </button>
        </div>
      </div>
    </div>
  </div>

  <!-- 新增/編輯 抽屜 -->
  <USlideover
    v-model:open="isDrawerOpen"
    side="right"
    :title="drawerTitle"
    :ui="{
      content: 'w-full max-w-120 bg-[#fdfbf6] text-paper-fg ring-paper-fg/10',
      header: 'border-b border-paper-fg/10 px-7 py-6',
      title: 'text-paper-fg font-display italic text-xl font-normal',
      close: 'text-paper-muted hover:bg-paper-fg/10 hover:text-paper-fg',
      body: 'px-7 py-6',
      footer: 'border-t border-paper-fg/10 px-7 py-4.5',
      overlay: 'bg-paper-fg/35'
    }"
  >
    <template #body>
      <div class="flex flex-col gap-5">
        <div>
          <label for="category-name" class="mb-1.5 block text-[12.5px] font-semibold">名稱</label>
          <input
            id="category-name"
            v-model="form.name"
            type="text"
            class="w-full rounded-[10px] border border-paper-fg/20 bg-white px-3 py-2.25 text-sm focus:outline-2 focus:outline-offset-1 focus:outline-paper-primary"
          />
        </div>

        <div>
          <label for="category-desc" class="mb-1.5 block text-[12.5px] font-semibold">描述</label>
          <textarea
            id="category-desc"
            v-model="form.description"
            rows="3"
            class="w-full resize-y rounded-[10px] border border-paper-fg/20 bg-white px-3 py-2.25 text-sm focus:outline-2 focus:outline-offset-1 focus:outline-paper-primary"
          ></textarea>
        </div>

        <div>
          <span class="mb-1.5 block text-[12.5px] font-semibold">顏色</span>
          <div class="mb-3 flex flex-wrap gap-2.5">
            <button
              v-for="swatch in swatches"
              :key="swatch"
              type="button"
              aria-label="選擇這個顏色"
              class="size-7.5 cursor-pointer rounded-full border-2 border-transparent p-0"
              :class="{ 'border-paper-fg ring-2 ring-offset-2 ring-paper-fg': form.color === swatch }"
              :style="{ backgroundColor: swatch }"
              @click="form.color = swatch"
            ></button>
          </div>
          <div class="flex items-center gap-2.5">
            <input
              v-model="form.color"
              type="color"
              aria-label="自訂顏色"
              class="h-9 w-10 cursor-pointer rounded-lg border border-paper-fg/20 bg-white p-0.5"
            />
            <input
              v-model="form.color"
              type="text"
              aria-label="色碼"
              class="flex-1 rounded-[10px] border border-paper-fg/20 bg-white px-3 py-2.25 font-mono text-sm uppercase focus:outline-2 focus:outline-offset-1 focus:outline-paper-primary"
            />
          </div>
          <p class="mt-2 text-xs text-paper-muted">可以直接點色票,或用色盤/輸入色碼自訂</p>
        </div>
      </div>
    </template>

    <template #footer>
      <div class="flex w-full gap-2.5">
        <button
          type="button"
          class="flex-1 cursor-pointer rounded-[10px] border border-paper-fg/20 bg-white py-2.75 text-sm"
          @click="isDrawerOpen = false"
        >
          取消
        </button>
        <button
          type="button"
          class="flex-2 cursor-pointer rounded-[10px] bg-paper-primary py-2.75 text-sm font-medium text-paper-bg"
          @click="isDrawerOpen = false"
        >
          儲存
        </button>
      </div>
    </template>
  </USlideover>

  <!-- 刪除確認彈窗 -->
  <UModal v-model:open="isDeleteModalOpen" title="確定要刪除這個分類嗎？" :ui="modalUi">
    <template #body>
      <p class="text-[15px] leading-relaxed text-paper-muted">
        「{{ deleteTarget?.name }}」會從書架上移除,裡面的單字本身不會被刪除。
      </p>
    </template>
    <template #footer>
      <div class="flex w-full gap-3">
        <UButton
          label="取消"
          color="neutral"
          variant="ghost"
          class="flex-1 justify-center text-paper-muted hover:bg-paper-fg/5 hover:text-paper-fg"
          @click="deleteTarget = null"
        />
        <UButton
          label="刪除"
          class="flex-1 justify-center bg-paper-accent text-paper-bg hover:bg-[#a8552f]"
          @click="deleteTarget = null"
        />
      </div>
    </template>
  </UModal>
</template>
