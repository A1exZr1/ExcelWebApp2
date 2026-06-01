<template>
  <div class="pt-4 pl-2 pr-2 pb-3 d-flex-column main-container" style="width: 100%; height: 100%">
    <div class="d-flex header-section" style="width: 100%; height: 10%">
      <div class="pl-3 pr-2 d-flex-column" style="width: 100%">
        <div class="pb-3 pt-2 d-flex">
          <div class="pr-2 d-flex" style="width: 60%">
            <v-text-field
              v-model="searchQuickFilterText"
              variant="solo-filled"
              label="Поиск в таблице"
              prepend-icon="mdi-magnify"
              clearable
              hide-details
              density="compact"
            />
          </div>
          <div class="pr-2 d-flex" style="width: 20%">
            <v-btn
              color="primary"
              block
              class="small-btn-text"
              size="large"
              :loading="isLoading"
              @click="onSelectFiles"
            >
              Выбрать файлы
            </v-btn>
          </div>
          <div class="pl-2 d-flex" style="width: 20%">
            <v-btn
              color="primary"
              block
              class="small-btn-text"
              size="large"
              :disabled="!hasRows"
              @click="onExportDataAsExcel"
            >
              Экспорт (xlsx)
            </v-btn>
          </div>
          <div class="pl-2 d-flex" style="width: 20%">
            <v-btn color="red" block class="small-btn-text" size="large" @click="resetData">
              Сброс
            </v-btn>
          </div>
        </div>
      </div>
    </div>
    <div class="pr-2 d-flex-column" style="width: 100%; height: 90%">
      <div style="width: 100%; height: 100%">
        <component
          :is="activeReportComponent"
          v-if="activeReportComponent"
          ref="activeReportRef"
          :quick-filter-text="searchQuickFilterText"
          :return-material-damage-percent="wbReturnMaterialDamagePercent"
          @rows-changed="onRowsChanged"
        />
      </div>
    </div>
    <file-selector-dialog
      :isVisible="isFileSelectorVisible"
      ref="fileSelectorDialog"
      @confirmed="onConfirmedLoadAndProcessAll"
      @canceled="onConfirmedCancel"
    />
  </div>
</template>

<script setup lang="ts">
import { computed, nextTick, ref } from 'vue'
import axios from 'axios'
import FileSelectorDialog from './FileSelectorDialog.vue'
import OzonV1Report from './reports/OzonV1Report.vue'
import OzonV2Report from './reports/OzonV2Report.vue'
import WildberriesReport from './reports/WildberriesReport.vue'
import type { ReportComponentExpose, ReportType } from './reports/reportTypes'

const searchQuickFilterText = ref('')
const isLoading = ref(false)
const isFileSelectorVisible = ref(false)
const hasRows = ref(false)
const activeReportType = ref<ReportType | null>(null)
const activeReportRef = ref<ReportComponentExpose>()
const fileSelectorDialog = ref<{ activeTab: ReportType; wbReturnMaterialDamagePercent: number }>()
const wbReturnMaterialDamagePercent = ref(15)

const reportComponents = {
  ozon1: OzonV1Report,
  ozon2: OzonV2Report,
  wb: WildberriesReport,
} as const

const activeReportComponent = computed(() =>
  activeReportType.value ? reportComponents[activeReportType.value] : null,
)

function onRowsChanged(value: boolean) {
  hasRows.value = value
}

function onSelectFiles() {
  isFileSelectorVisible.value = true
}

async function onConfirmedLoadAndProcessAll() {
  isFileSelectorVisible.value = false
  const selectedReport = fileSelectorDialog.value?.activeTab

  if (!selectedReport || !(selectedReport in reportComponents)) {
    alert('Неизвестный тип загрузки')
    return
  }

  try {
    isLoading.value = true
    hasRows.value = false
    activeReportType.value = selectedReport
    wbReturnMaterialDamagePercent.value = Number(
      fileSelectorDialog.value?.wbReturnMaterialDamagePercent ?? 15,
    )
    searchQuickFilterText.value = ''

    await nextTick()
    await activeReportRef.value?.loadData()
  } finally {
    isLoading.value = false
  }
}

function onConfirmedCancel() {
  isFileSelectorVisible.value = false
}

async function onExportDataAsExcel() {
  if (!activeReportRef.value) return

  try {
    await activeReportRef.value.exportData()
  } catch (err) {
    console.error(err)
    alert('Ошибка при экспорте')
  }
}

async function resetData() {
  activeReportRef.value?.reset()
  activeReportType.value = null
  hasRows.value = false
  searchQuickFilterText.value = ''
  await resetBackendData()
}

async function resetBackendData() {
  try {
    await axios.post('/api/FileReader/Reset')
  } catch (err) {
    console.error(err)
    alert('Ошибка при сбросе данных')
  }
}
</script>

<style>
.ag-theme-alpine {
  width: 100%;
  height: 100%;
}
.red-row {
  background-color: #ffe6e6;
}
.small-btn-text {
  font-size: 0.7rem;
  line-height: 1.2;
  white-space: normal;
  text-align: center;
  padding: 4px 8px;
}
.main-container {
  height: 100vh;
  overflow: auto;
  padding: 16px;
  box-sizing: border-box;
}

.header-section {
  overflow-y: auto;
  max-height: 35vh;
}
</style>
