<template>
  <div>
    <NuxtRouteAnnouncer />
    <NuxtPage />
  </div>
</template>

<script setup lang="ts">
// 載入外部資源的設定
useHead({
  script: [{ src: "https://accounts.google.com/gsi/client", async: true, defer: true }]
})

// src:要載入的外部 script 檔案網址
// async:布林值(true/false)。設 true 代表「瀏覽器一邊解析 HTML,一邊背景下載這支 script,下載完就馬上執行,不管 HTML 解析到哪」
// defer:布林值。設 true 代表「一樣背景下載,但要等整份 HTML 都解析完才執行」

// 規格上如果兩個都設 true,瀏覽器會照 async 的行為走(defer 被忽略)。兩個一起加是常見寫法,是為了相容一些很舊、不支援 async 的瀏覽器(那些瀏覽器就會退回用 defer 的行為)
</script>
