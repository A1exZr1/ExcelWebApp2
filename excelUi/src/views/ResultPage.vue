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
              hide-details:
              true
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
              hide-details:
              true
              class="small-btn-text"
              size="large"
              :disabled="!hasRows"
              @click="onExportDataAsExcel"
            >
              Экспорт (xlsx)
            </v-btn>
          </div>
          <div class="pl-2 d-flex" style="width: 20%">
            <v-btn
              color="red"
              block
              hide-details:
              true
              class="small-btn-text"
              size="large"
              @click="resetData"
            >
              Сброс
            </v-btn>
          </div>
        </div>
      </div>
    </div>
    <div class="pr-2 d-flex-column" style="width: 100%; height: 90%">
      <div style="width: 100%; height: 95%">
        <ag-grid-vue
          class="ag-theme-alpine"
          style="width: 100%; height: 100%"
          :gridOptions="gridOptions"
          :rowData="rowMainData"
          @gridReady="onGridReady"
          @filterChanged="onFilterChanged"
        />
      </div>
      <div class="pt-2 pr-2 pl-2 d-flex justify" style="font-size: 0.85rem">
        Показано: {{ filteredRowCount }} из {{ totalRowCount }}
      </div>
    </div>
    <file-selector-dialog
      :isVisible="isFileSelectorVisible"
      ref="activeTab"
      @confirmed="onConfirmedLoadAndProcessAll"
      @canceled="onConfirmedCancel"
    />
  </div>
</template>

<script setup lang="ts">
/* eslint-disable @typescript-eslint/no-explicit-any */
import { ref, computed, watch } from 'vue'
import { AgGridVue } from 'ag-grid-vue3'
import { GridOptions, ColDef, GridApi, GridReadyEvent, RowClassParams } from 'ag-grid-community'
import ResultGridOzonV1 from './ResultGridOzonV1'
import ResultGridOzonV2 from './ResultGridOzonV2'
import axios from 'axios'
import FileSelectorDialog from './FileSelectorDialog.vue'

const searchQuickFilterText = ref('')
const gridApi = ref<GridApi>()
const rowMainData = ref<ResultGridOzonV1[]>([])
const displayedRows = ref<ResultGridOzonV1[]>([])
const totalRowCount = ref(0)
const filteredRowCount = ref(0)
const mainColumnDefs = ref<ColDef<any, any>[]>([])

const isLoading = ref(false)
const isFileSelectorVisible = ref(false)
const activeTab = ref()

const hasRows = computed(() => rowMainData.value && rowMainData.value.length > 0)

watch(searchQuickFilterText, (newValue) => {
  if (!gridApi.value) return
  gridApi.value.setGridOption('quickFilterText', newValue)
  updateRowCounts()
})

function updateRowCounts() {
  if (!gridApi.value) return

  totalRowCount.value = rowMainData.value?.length ?? 0
  filteredRowCount.value = gridApi.value?.getDisplayedRowCount()
}

watch(rowMainData, () => {
  updateRowCounts()
})

function onFilterChanged() {
  updateRowCounts()
}

const gridOptions: GridOptions = {
  rowClassRules: {
    'red-row': (params: RowClassParams) => params.data?.primeCost == null,
  },
  defaultColDef: {
    sortable: true,
    filter: 'agTextColumnFilter',
    resizable: true,
    flex: 1,
    wrapHeaderText: true,
    autoHeaderHeight: true,
  },
  popupParent: document.body,
  rowModelType: 'clientSide',
  overlayNoRowsTemplate: '<span>Нет данных</span>',
  rowSelection: 'single',
  enableCellTextSelection: true,
  localeText: {
    equals: 'Равно',
    notEqual: 'Не равно',
    lessThan: 'Меньше',
    greaterThan: 'Больше',
    lessThanOrEqual: 'Меньше или равно',
    greaterThanOrEqual: 'Больше или равно',
    inRange: 'В диапазоне',
    contains: 'Содержит',
    notContains: 'Не содержит',
    startsWith: 'Начинается с',
    endsWith: 'Заканчивается на',
    filterOoo: 'Фильтр...',
    blanks: '(пусто)',
    apply: 'Применить',
    reset: 'Сбросить',
    clear: 'Очистить',
    cancel: 'Отмена',
    columns: 'Колонки',
    filters: 'Фильтры',
    noRowsToShow: 'Нет данных',
    blank: 'Пусто',
    notBlank: 'Не пусто',
  },
}

async function onGridReady(event: GridReadyEvent) {
  gridApi.value = event.api
  gridApi.value.setGridOption('rowData', [])
  updateRowCounts()
}

/* async function loadAndProcessAll() {
  try {
    isLoading.value = true
    await Promise.all([
      accrualUploader.value.upload(),
      adsUploader.value.upload(),
      primeUploader.value.upload(),
    ])

    if (isAnyBadFileSelected.value) {
      isLoading.value = false
      return
    }

    await loadData()
  } catch (err) {
    console.error('Ошибка при загрузке файлов', err)
  }
  isLoading.value = false
} */

function onSelectFiles() {
  isFileSelectorVisible.value = true
}

function onConfirmedLoadAndProcessAll() {
  isFileSelectorVisible.value = false
  if (activeTab.value === 'ozon1') {
    gridApi.value?.setGridOption('columnDefs', mainColumnDefsOzonV1)
    loadProcessedOzonV1Data()
  } else {
    mainColumnDefs.value = mainColumnDefsOzonV2
    gridApi.value?.setGridOption('columnDefs', mainColumnDefs.value)
    loadProcessedOzonV2Data()
  }
}

function onConfirmedCancel() {
  isFileSelectorVisible.value = false
}

async function loadProcessedOzonV1Data() {
  if (!gridApi.value) return

  try {
    const response = await axios.get('/api/FileReader/GetOzonV1Results')

    rowMainData.value = response.data.map(
      (item: any) =>
        new ResultGridOzonV1(
          item.articleName,
          item.sku,
          item.quantity,
          item.totalSumm,
          item.revenue,
          item.advertisingCost,
          item.workCost,
          item.materialCost,
          item.unlinkedExpenses,
          item.netProfit,
          item.profitPercent,
        ),
    )
    gridApi.value.setGridOption('rowData', rowMainData.value)
    updateRowCounts()
  } catch (err) {
    console.error(err)
    alert('Ошибка при получении результатов')
  }
}

async function loadProcessedOzonV2Data() {
  if (!gridApi.value) return

  try {
    const response = await axios.get('/api/FileReader/GetOzonV1Results')

    rowMainData.value = response.data.map(
      (item: any) =>
        new ResultGridOzonV2(
          item.articleName,
          item.sku,
          item.warehouse,
          item.preCommissionAmount,
          item.quantity,
          item.workCost,
          item.materialCost,
          item.unlinkedExpenses,

          item.ozonFee,
          item.handlingFee,
          item.lastMileFee,
          item.logisticFee,
          item.netProfit,
          item.profitPercent,
        ),
    )
    gridApi.value.setGridOption('rowData', rowMainData.value)
    updateRowCounts()
  } catch (err) {
    console.error(err)
    alert('Ошибка при получении результатов')
  }
}

const mainColumnDefsOzonV1 = [
  { field: 'articleName', headerName: 'Имя артикула', minWidth: 300, sort: 'asc', sortIndex: 0 },
  { field: 'sku', headerName: 'SKU', minWidth: 150 },
  {
    field: 'totalSumm',
    headerName: 'Поступило на счёт',
    minWidth: 150,
    filter: 'agNumberColumnFilter',
    valueFormatter: (params: any) => params.value?.toFixed(2),
  },
  {
    field: 'quantity',
    headerName: 'Количество продаж',
    minWidth: 80,
    filter: 'agNumberColumnFilter',
    valueFormatter: (params: any) => params.value?.toFixed(2),
  },
  {
    field: 'revenue',
    headerName: 'Выручка',
    minWidth: 100,
    filter: 'agNumberColumnFilter',
    valueFormatter: (params: any) => params.value?.toFixed(2),
  },
  {
    field: 'advertisingCost',
    headerName: 'Расходы на рекламу',
    minWidth: 100,
    filter: 'agNumberColumnFilter',
    valueFormatter: (params: any) => params.value?.toFixed(2),
  },
  {
    field: 'unlinkedExpenses',
    headerName: 'Нераспределённые расходы',
    minWidth: 100,
    filter: 'agNumberColumnFilter',
    valueFormatter: (params: any) => params.value?.toFixed(2),
  },
  {
    field: 'workCost',
    headerName: 'Стоимость работы',
    minWidth: 100,
    filter: 'agNumberColumnFilter',
    valueFormatter: (params: any) => params.value?.toFixed(2),
  },
  {
    field: 'materialCost',
    headerName: 'Стоимость материалов',
    minWidth: 100,
    filter: 'agNumberColumnFilter',
    valueFormatter: (params: any) => params.value?.toFixed(2),
  },
  {
    field: 'netProfit',
    headerName: 'Чистая прибыль',
    minWidth: 130,
    filter: 'agNumberColumnFilter',
    valueFormatter: (params: any) => params.value?.toFixed(2),
  },
  {
    field: 'profitPercent',
    headerName: '% от выручки',
    minWidth: 130,
    filter: 'agNumberColumnFilter',
    valueFormatter: (params: any) => (params.value != null ? `${params.value.toFixed(2)}%` : 'н/д'),
  },
] as const satisfies ColDef[]

const mainColumnDefsOzonV2 = [
  { field: 'articleName', headerName: 'Имя артикула', minWidth: 300, sort: 'asc', sortIndex: 0 },
  { field: 'sku', headerName: 'SKU', minWidth: 150 },
  {
    field: 'totalSumm',
    headerName: 'Поступило на счёт',
    minWidth: 150,
    filter: 'agNumberColumnFilter',
    valueFormatter: (params: any) => params.value?.toFixed(2),
  },
  {
    field: 'quantity',
    headerName: 'Количество продаж',
    minWidth: 80,
    filter: 'agNumberColumnFilter',
    valueFormatter: (params: any) => params.value?.toFixed(2),
  },
  {
    field: 'unlinkedExpenses',
    headerName: 'Нераспределённые расходы',
    minWidth: 100,
    filter: 'agNumberColumnFilter',
    valueFormatter: (params: any) => params.value?.toFixed(2),
  },
  {
    field: 'primeCost',
    headerName: 'Себестоимость',
    minWidth: 100,
    filter: 'agNumberColumnFilter',
    valueFormatter: (params: any) => params.value?.toFixed(2),
  },
] as const satisfies ColDef[]

async function onExportDataAsExcel() {
  if (!gridApi.value) return
  displayedRows.value = []
  try {
    // Get only what user sees

    gridApi.value.forEachNodeAfterFilterAndSort((node) => {
      if (node.data) displayedRows.value!.push(node.data)
    })

    // Convert proxies/class instances to plain objects
    const plainRows = JSON.parse(JSON.stringify(displayedRows.value))

    const response = await axios.post('/api/FileReader/ExportProcessedResultsFiltered', plainRows, {
      responseType: 'blob',
      headers: { 'Content-Type': 'application/json' },
    })

    const blob = new Blob([response.data], {
      type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
    })

    const url = URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = 'processed_results.xlsx'
    link.click()
    URL.revokeObjectURL(url)
  } catch (err) {
    console.error(err)
    alert('Ошибка при экспорте')
  }
}

async function resetData() {
  gridApi.value?.setGridOption('rowData', [])
  rowMainData.value = []
  displayedRows.value = []
  totalRowCount.value = 0
  filteredRowCount.value = 0
  searchQuickFilterText.value = ''
  await ResetBackendData()
}

async function ResetBackendData() {
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
  background-color: #ffe6e6; /* светло-красный фон */
}
.small-btn-text {
  font-size: 0.7rem; /* или 12px, как тебе удобно */
  line-height: 1.2;
  white-space: normal; /* разрешить перенос строк */
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
