<template>
  <v-app style="height: 100vh; width: 100vw;">
    <v-main style="height: 100%; width: 100%;">
      <ResultPage />
    </v-main>
  </v-app>
</template>

<script setup lang="ts">
import ResultPage from './views/ResultPage.vue'
</script>

<style lang="scss">

.v-navigation-drawer .v-ripple__container {
  display: none !important;
}

html {
  overflow-y: auto !important;
}

pre {
  white-space: pre-wrap;
}
</style>