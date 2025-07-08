<template>
  <v-container class="fill-height" fluid>
    <v-row>
      <v-col cols="12" md="4">
        <FileUploader
          label="Загрузить файл начислений"
          endpoint="/api/FileReader/ReadAccrual"
          @uploaded="onAccrualUploaded"
        />
      </v-col>

      <v-col cols="12" md="4">
        <FileUploader
          label="Загрузить файл рекламы"
          endpoint="/api/FileReader/ReadAdvertisment"
          @uploaded="onAdsUploaded"
        />
      </v-col>

      <v-col cols="12" md="4">
        <FileUploader
          label="Загрузить файл себестоимости"
          endpoint="/api/FileReader/PrimeCostModel"
          @uploaded="onPrimeUploaded"
        />
      </v-col>
    </v-row>

    <v-row class="mb-4">
      <v-col cols="12" class="text-center">
        <v-btn color="primary" :disabled="!canProcess" @click="fetchProcessedResults">
          Обработать и показать таблицу
        </v-btn>
      </v-col>
    </v-row>

    <v-row v-if="results.length > 0">
      <v-col cols="12">
        <ResultTable :rows="results" />
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import axios from 'axios'
import FileUploader from './FileUploader.vue'
import ResultTable from './ResultTable.vue'
import ResultGridItem from './ResultGridItem.ts'

const accrualUploaded = ref(false)
const adsUploaded = ref(false)
const primeUploaded = ref(false)
const results = ref<ResultGridItem[]>([])

const canProcess = computed(() => accrualUploaded.value && adsUploaded.value && primeUploaded.value)

function onAccrualUploaded() {
  accrualUploaded.value = true
}
function onAdsUploaded() {
  adsUploaded.value = true
}
function onPrimeUploaded() {
  primeUploaded.value = true
}

async function fetchProcessedResults() {
try {
    const response = await axios.get('/api/FileReader/GetProcessedResults')

    results.value = response.data.map((item: any) =>
      new ResultGridItem(
        item.articleName,
        item.sku,
        item.revenue,
        item.advertisingCost,
        item.primeCost,
        item.netProfit,
        item.profitPercent
      )
    )
  } catch (err) {
    console.error(err)
    alert('Ошибка при получении результатов')
  }
}
</script>

<style scoped>
.fill-height {
  min-height: 100vh;
}
</style>
