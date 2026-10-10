<script setup lang="ts">
definePageMeta({ layout: "admin", middleware: "admin" })
import type { Category } from "~/types/practice"
import type { AdminWord, PagedResponse } from "~/types/admin"
const toast = useToast()

const { data: categories } = await useAsyncData("categories", () => useApiFetch<Category[]>("/api/categories"), {
  default: () => []
})

const { data: wordsPage, refresh: refresh } = await useAsyncData(
  "admin-words",
  () =>
    useApiFetch<PagedResponse<AdminWord>>("/api/admin/words", {
      query: { page: 1, pageSize: 20 }
    }),
  { default: () => ({ items: [], totalCount: 0 }) }
)

const categoryFilterOptions = computed(() => [
  { label: "全部分類", value: null },
  ...categories.value.map((c) => ({ label: c.name, value: c.id }))
])

const partsOfSpeech = ["形容詞", "副詞", "動詞", "名詞", "代名詞", "介系詞", "連接詞", "感嘆詞"]

const categoryById = (id: number) => categories.value.find((c) => c.id === id)

const isSaving = ref(false)
const saveError = ref("")

const saveWord = async () => {
  if (!form.term.trim()) {
    saveError.value = "名稱不能空白"
    return
  }

  isSaving.value = true
  saveError.value = ""
  const isEditing = editingId.value !== null
  const body = {
    word: {
      id: editingId.value ?? 0,
      term: form.term.trim(),
      ipa: form.ipa.trim() || null,
      partOfSpeech: form.partOfSpeech,
      definitionEN: form.definitionEN.trim(),
      definitionCN: form.definitionCN.trim(),
      examples: form.examples.filter((e) => e.trim() !== "")
    },
    categoryIds: form.categoryIds
  }
  try {
    if (isEditing) {
      await useApiFetch(`/api/admin/words/${editingId.value}`, {
        method: "PUT",
        body
      })
    } else {
      await useApiFetch("/api/admin/words", { method: "POST", body })
    }
    isDrawerOpen.value = false
    toast.add({ title: isEditing ? "已更新單字" : "已新增單字", color: "success" })
    await refresh()
  } catch (error) {
    const err = error as { statusCode?: number; data?: string }
    if (err.statusCode === 409) {
      saveError.value = err.data ?? "此單字已存在"
    } else {
      toast.add({ title: "儲存失敗，請稍後再試", color: "error" })
    }
  } finally {
    isSaving.value = false
  }
}

// ---- 列表展開例句 ----
const expandedIds = ref<Set<number>>(new Set())
const toggleExpand = (id: number) => {
  const next = new Set(expandedIds.value)
  if (next.has(id)) {
    next.delete(id)
  } else {
    next.add(id)
  }
  expandedIds.value = next
}

// ---- 篩選列(目前只是畫面,還沒接功能)----
const filterCategoryId = ref<number | null>(null)
const keyword = ref("")

// ---- 抽屜表單(新增跟編輯共用同一張表單)----
const isDrawerOpen = ref(false)
const editingId = ref<number | null>(null)
const form = reactive({
  term: "",
  ipa: "",
  partOfSpeech: partsOfSpeech[0] as string,
  definitionEN: "",
  definitionCN: "",
  examples: [] as string[],
  categoryIds: [] as number[]
})

const drawerTitle = computed(() => (editingId.value === null ? "新增單字" : "編輯單字"))

const openCreate = () => {
  editingId.value = null
  form.term = ""
  form.ipa = ""
  form.partOfSpeech = partsOfSpeech[0] as string
  form.definitionEN = ""
  form.definitionCN = ""
  form.examples = [""]
  form.categoryIds = []
  isDrawerOpen.value = true
}

const openEdit = (word: AdminWord) => {
  editingId.value = word.id
  form.term = word.term
  form.ipa = word.ipa ?? ""
  form.partOfSpeech = word.partOfSpeech
  form.definitionEN = word.definitionEN
  form.definitionCN = word.definitionCN
  form.examples = [...word.examples]
  form.categoryIds = [...word.categoryIds]
  isDrawerOpen.value = true
}

const addExample = () => form.examples.push("")
const removeExample = (index: number) => form.examples.splice(index, 1)

// ---- 刪除確認彈窗 ----
const deleteTarget = ref<AdminWord | null>(null)
const isDeleting = ref(false)
const isDeleteModalOpen = computed({
  get: () => deleteTarget.value !== null,
  set: (open: boolean) => {
    if (!open) deleteTarget.value = null
  }
})

const confirmDelete = async () => {
  if (!deleteTarget.value) return
  isDeleting.value = true

  const { id, term } = deleteTarget.value
  try {
    await useApiFetch(`/api/admin/words/${id}`, { method: "DELETE" })
    deleteTarget.value = null
    toast.add({ title: `已刪除 ${term}`, color: "success" })
    await refresh()
  } catch (error) {
    const err = error as { statusCode?: number }
    if (err.statusCode === 404) {
      deleteTarget.value = null
      toast.add({ title: "此單字已經不存在", color: "warning" })
      await refresh()
    } else {
      toast.add({ title: "刪除失敗，請稍後再試", color: "error" })
    }
  } finally {
    isDeleting.value = false
  }
}

const inputClass =
  "w-full rounded-[10px] border border-paper-fg/20 bg-white px-3 py-2.25 text-sm focus:outline-2 focus:outline-offset-1 focus:outline-paper-primary"
</script>

<template>
  <div class="flex flex-col gap-5">
    <div class="flex items-end justify-between">
      <div>
        <h1 class="font-display italic text-[28px] font-normal mb-1.5">單字管理</h1>
      </div>
      <button
        type="button"
        class="rounded-full bg-paper-primary text-paper-bg px-5.5 py-2.5 text-sm font-medium cursor-pointer transition-[transform,background-color] duration-250 ease-out hover:scale-[1.03] hover:bg-paper-accent"
        @click="openCreate"
      >
        + 新增單字
      </button>
    </div>

    <div class="flex gap-3">
      <USelect
        v-model="filterCategoryId"
        :items="categoryFilterOptions"
        aria-label="依分類篩選"
        :ui="{ base: 'w-44 shrink-0 rounded-full bg-paper-bg px-4.5 py-2.25 text-[13px]' }"
      />
      <input
        v-model="keyword"
        type="search"
        placeholder="搜尋單字..."
        aria-label="搜尋單字"
        class="flex-1 rounded-full border border-paper-fg/15 bg-paper-bg px-4.5 py-2.25 text-[13px] placeholder:text-paper-muted focus:outline-2 focus:outline-offset-1 focus:outline-paper-primary"
      />
    </div>

    <div class="rounded-2xl border border-paper-fg/10 bg-paper-bg overflow-hidden">
      <div
        class="grid grid-cols-[200px_280px_90px_1fr_120px] bg-[#ebddc5] text-[11.5px] uppercase tracking-[0.06em] text-paper-muted"
      >
        <div class="px-5 py-3.5">單字</div>
        <div class="px-5 py-3.5">中文釋義</div>
        <div class="px-5 py-3.5">詞性</div>
        <div class="px-5 py-3.5">所屬分類</div>
        <div class="px-5 py-3.5">操作</div>
      </div>

      <template v-for="word in wordsPage.items" :key="word.id">
        <div class="grid grid-cols-[200px_280px_90px_1fr_120px] items-center border-t border-paper-fg/8 text-sm">
          <div class="px-5 py-3.5 flex items-center gap-1.5">
            <button
              type="button"
              :aria-label="expandedIds.has(word.id) ? '收合例句' : '展開例句'"
              class="shrink-0 cursor-pointer text-paper-muted transition-transform duration-150"
              :class="{ 'rotate-90': expandedIds.has(word.id) }"
              @click="toggleExpand(word.id)"
            >
              ▸
            </button>
            <span class="font-display italic text-base">{{ word.term }}</span>
          </div>
          <div class="px-5 py-3.5">{{ word.definitionCN }}</div>
          <div class="px-5 py-3.5">{{ word.partOfSpeech }}</div>
          <div class="px-5 py-3.5 flex flex-wrap gap-1.5">
            <span
              v-for="categoryId in word.categoryIds"
              :key="categoryId"
              class="inline-flex items-center rounded-full px-2.5 py-0.75 text-[11.5px] font-medium"
              :style="{
                backgroundColor: `color-mix(in srgb, ${categoryById(categoryId)?.color} 16%, transparent)`,
                color: `color-mix(in srgb, ${categoryById(categoryId)?.color} 65%, black)`
              }"
            >
              {{ categoryById(categoryId)?.name }}
            </span>
            <span v-if="word.categoryIds.length === 0" class="text-[12.5px] text-paper-muted">未分類</span>
          </div>
          <div class="px-5 py-3.5 flex gap-3.5 text-[13px]">
            <button type="button" class="text-paper-primary cursor-pointer hover:underline" @click="openEdit(word)">
              編輯
            </button>
            <button type="button" class="text-paper-accent cursor-pointer hover:underline" @click="deleteTarget = word">
              刪除
            </button>
          </div>
        </div>
        <div v-if="expandedIds.has(word.id)" class="border-t border-paper-fg/8 bg-[#f7f1e6] px-5 py-3.5 text-[13px]">
          <ul v-if="word.examples.length > 0" class="flex flex-col gap-1 text-paper-fg/80">
            <li v-for="(example, index) in word.examples" :key="index">{{ example }}</li>
          </ul>
          <span v-else class="text-paper-muted">尚無例句</span>
        </div>
      </template>
    </div>
  </div>

  <!-- 新增/編輯 抽屜 -->
  <USlideover
    v-model:open="isDrawerOpen"
    side="right"
    :title="drawerTitle"
    :ui="{
      content: 'w-full max-w-140 bg-[#fdfbf6]',
      header: 'px-7 py-6',
      body: 'px-7 py-6',
      footer: 'px-7 py-4.5'
    }"
  >
    <template #body>
      <div class="flex flex-col gap-4.5">
        <div>
          <label for="word-term" class="mb-1.5 block text-[12.5px] font-semibold">單字</label>
          <input id="word-term" v-model="form.term" type="text" :class="inputClass" />
          <p v-if="saveError" class="mt-1.5 text-xs text-paper-accent">{{ saveError }}</p>
        </div>

        <div class="flex gap-3">
          <div class="flex-1">
            <label for="word-ipa" class="mb-1.5 block text-[12.5px] font-semibold">KK 音標</label>
            <input id="word-ipa" v-model="form.ipa" type="text" :class="[inputClass, 'font-mono']" />
          </div>
          <div class="flex-1">
            <label for="word-pos" class="mb-1.5 block text-[12.5px] font-semibold">詞性</label>
            <select id="word-pos" v-model="form.partOfSpeech" :class="inputClass">
              <option v-for="pos in partsOfSpeech" :key="pos" :value="pos">{{ pos }}</option>
            </select>
          </div>
        </div>

        <div>
          <label for="word-def-en" class="mb-1.5 block text-[12.5px] font-semibold">英文釋義</label>
          <input id="word-def-en" v-model="form.definitionEN" type="text" :class="inputClass" />
        </div>

        <div>
          <label for="word-def-cn" class="mb-1.5 block text-[12.5px] font-semibold">中文釋義</label>
          <input id="word-def-cn" v-model="form.definitionCN" type="text" :class="inputClass" />
        </div>

        <div>
          <span class="mb-1.5 block text-[12.5px] font-semibold">例句</span>
          <div class="flex flex-col gap-2">
            <div v-for="(_, index) in form.examples" :key="index" class="flex items-center gap-2">
              <input
                v-model="form.examples[index]"
                type="text"
                :aria-label="`例句 ${index + 1}`"
                :class="[inputClass, 'flex-1']"
              />
              <button
                type="button"
                aria-label="刪除這筆例句"
                class="size-7.5 shrink-0 cursor-pointer rounded-lg border border-paper-fg/15 bg-white text-paper-accent"
                @click="removeExample(index)"
              >
                &times;
              </button>
            </div>
          </div>
          <button
            type="button"
            class="mt-2.5 cursor-pointer rounded-[10px] border border-dashed border-paper-fg/30 px-3.5 py-2 text-[13px] text-paper-muted"
            @click="addExample"
          >
            + 新增例句
          </button>
        </div>

        <div>
          <span class="mb-1.5 block text-[12.5px] font-semibold">所屬分類</span>
          <div class="rounded-[10px] border border-paper-fg/15 bg-white px-3.5 py-2.5">
            <label
              v-for="category in categories"
              :key="category.id"
              class="flex cursor-pointer items-center gap-2 py-1 text-[13.5px]"
            >
              <input v-model="form.categoryIds" type="checkbox" :value="category.id" />
              <span class="inline-block size-2.25 rounded-[3px]" :style="{ backgroundColor: category.color }"></span>
              {{ category.name }}
            </label>
          </div>
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
        <UButton
          label="儲存"
          class="flex-2 justify-center rounded-[10px] bg-paper-primary text-sm font-medium text-paper-bg"
          @click="saveWord"
          :loading="isSaving"
          :disabled="isSaving"
        ></UButton>
      </div>
    </template>
  </USlideover>

  <!-- 刪除確認彈窗 -->
  <UModal v-model:open="isDeleteModalOpen" title="確定要刪除這個單字嗎？">
    <template #body>
      <p class="text-[15px] leading-relaxed text-paper-muted">
        「{{ deleteTarget?.term }}」會從所有書本中移除,無法復原。
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
          :loading="isDeleting"
          @click="confirmDelete"
        />
      </div>
    </template>
  </UModal>
</template>
