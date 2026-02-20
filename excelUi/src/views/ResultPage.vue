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
      <div style="width: 100%; height: 100%">
        <ag-grid-vue
          class="ag-theme-alpine"
          style="width: 100%; height: 100%"
          :gridOptions="gridOptions"
          :rowData="rowMainData"
          @gridReady="onGridReady"
          @filterChanged="onFilterChanged"
        />
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
import ResultGridWB from './ResultGridWB'
import axios from 'axios'
import FileSelectorDialog from './FileSelectorDialog.vue'

const searchQuickFilterText = ref('')
const gridApi = ref<GridApi>()
const rowMainData = ref<ResultGridOzonV1[]>([])
const displayedRows = ref<ResultGridOzonV1[]>([])
const totalRowCount = ref(0)
const filteredRowCount = ref(0)
const mainColumnDefs = ref<ColDef<any, any>[]>([])
const feeKeysRef = ref<string[]>([])
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
  updatePinnedTotals()
}

const gridOptions: GridOptions = {
  columnDefs: mainColumnDefs.value,
  rowClassRules: {
    'red-row': (params: RowClassParams) => params.data?.materialCost == null,
  },
  defaultColDef: {
    sortable: true,
    filter: 'agTextColumnFilter',
    resizable: true,
    wrapHeaderText: true,
    autoHeaderHeight: true,
  },
  getRowStyle: (p) => (p.node.rowPinned ? { fontWeight: '600', background: '#f8f9fb' } : undefined),
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
  updatePinnedTotals()
}

function onSelectFiles() {
  isFileSelectorVisible.value = true
}

function onConfirmedLoadAndProcessAll() {
  isFileSelectorVisible.value = false
  if (activeTab.value.activeTab === 'ozon1') {
    mainColumnDefs.value = mainColumnDefsOzonV1
    gridApi.value?.setGridOption('columnDefs', mainColumnDefs.value)
    loadProcessedOzonV1Data()
  } else if (activeTab.value.activeTab === 'ozon2') {
    loadProcessedOzonV2Data()
  } else if (activeTab.value.activeTab === 'wb') {
    mainColumnDefs.value = mainColumnDefsWb
    gridApi.value?.setGridOption('columnDefs', mainColumnDefs.value)
    loadProcessedWbData()
  } else {
    alert('Неизвестный тип загрузки')
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
    updatePinnedTotals()
  } catch (err) {
    console.error(err)
    alert('Ошибка при получении результатов')
  }
}

async function loadProcessedOzonV2Data() {
  if (!gridApi.value) return

  try {
    const response = await axios.get('/api/FileReader/GetOzonV2Results')

    feeKeysRef.value = Array.from(
      new Set((response.data as any[]).flatMap((x) => Object.keys(x.additionalFees ?? {}))),
    )

    mainColumnDefs.value = buildOzonV2Columns(feeKeysRef.value)
    gridApi.value.setGridOption('columnDefs', mainColumnDefs.value)

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
          item.logisticsFee,
          item.netProfit,
          item.profitPercent,
          item.additionalFees ?? {},
        ),
    )
    gridApi.value.setGridOption('rowData', rowMainData.value)
    updateRowCounts()
    updatePinnedTotals()
  } catch (err) {
    console.error(err)
    alert('Ошибка при получении результатов')
  }
}

async function loadProcessedWbData() {
  if (!gridApi.value) return

  try {
    const response = await axios.get('/api/FileReader/GetWbResults')
    rowMainData.value = response.data.map(
      (item: any) =>
        new ResultGridWB(
          item.articleName,
          item.sku,
          item.supplierArticleName,
          item.brand,
          item.quantity,
          item.retailPriceSumm,
          item.amountPayableToSellerSumm,
          item.logisticsFee,
          item.cancelledQuantity,
          item.cancelledSumm,
          item.paidAcceptanceSumm,
          item.totalAmountOfFines,
          item.returnedQuantity,
          item.returnedSumm,
          item.advertisingCost,
          item.reviewPointsCost,
          item.workCost,
          item.materialCost,
          item.netProfit,
          item.profitPercent,
        ),
    )
    gridApi.value.setGridOption('rowData', rowMainData.value)
    updateRowCounts()
    updatePinnedTotals()
  } catch (err) {
    console.error(err)
    alert('Ошибка при получении результатов')
  }
}

function updatePinnedTotals(visibleOnly = true) {
  if (!gridApi.value) return

  const rows: any[] = []
  if (visibleOnly) {
    gridApi.value.forEachNodeAfterFilterAndSort((n) => n.data && rows.push(n.data))
  } else {
    rows.push(...rowMainData.value)
  }

  const totals: any = { articleName: `Итого: ${filteredRowCount.value} из ${totalRowCount.value}` }

  const sum = (sel: (r: any) => number | undefined | null) =>
    Number(rows.reduce((acc, r) => acc + (Number(sel(r)) || 0), 0).toFixed(2))

  const pct = (num: number, den: number) =>
    den !== 0 ? Number(((num / Math.abs(den)) * 100).toFixed(2)) : null

  if (activeTab.value?.activeTab === 'ozon1') {
    totals.totalSumm = sum((r) => r.totalSumm)
    totals.quantity = sum((r) => r.quantity)
    totals.revenue = sum((r) => r.revenue)
    totals.advertisingCost = sum((r) => r.advertisingCost)
    totals.unlinkedExpenses = sum((r) => r.unlinkedExpenses)
    totals.workCost = sum((r) => r.workCost)
    totals.materialCost = sum((r) => r.materialCost)
    totals.netProfit = sum((r) => r.netProfit)
    totals.profitPercent = pct(totals.netProfit, totals.revenue)
  }

  if (activeTab.value?.activeTab === 'ozon2') {
    totals.preCommissionAmount = sum((r) => r.preCommissionAmount)
    totals.quantity = sum((r) => r.quantity)
    totals.workCost = sum((r) => r.workCost)
    totals.materialCost = sum((r) => r.materialCost)
    totals.unlinkedExpenses = sum((r) => r.unlinkedExpenses)
    totals.ozonFee = sum((r) => r.ozonFee)
    totals.handlingFee = sum((r) => r.handlingFee)
    totals.lastMileFee = sum((r) => r.lastMileFee)
    totals.logisticsFee = sum((r) => r.logisticsFee)
    totals.netProfit = sum((r) => r.netProfit)
    totals.profitPercent = pct(totals.netProfit, totals.preCommissionAmount)

    const fees: Record<string, number> = {}
    for (const k of feeKeysRef.value) {
      fees[k] = Number(
        rows.reduce((acc, r) => acc + (Number(r.additionalFees?.[k]) || 0), 0).toFixed(2),
      )
    }
    totals.additionalFees = fees
  }

  if (activeTab.value?.activeTab === 'wb') {
    totals.quantity = sum((r) => r.quantity)
    totals.retailPriceSumm = sum((r) => r.retailPriceSumm)
    totals.amountPayableToSellerSumm = sum((r) => r.amountPayableToSellerSumm)
    totals.logisticsFee = sum((r) => r.logisticsFee)
    totals.cancelledQuantity = sum((r) => r.cancelledQuantity)
    totals.cancelledSumm = sum((r) => r.cancelledSumm)
    totals.paidAcceptanceSumm = sum((r) => r.paidAcceptanceSumm)
    totals.totalAmountOfFines = sum((r) => r.totalAmountOfFines)
    totals.returnedQuantity = sum((r) => r.returnedQuantity)
    totals.returnedSumm = sum((r) => r.returnedSumm)
    totals.advertisingCost = sum((r) => r.advertisingCost)
    totals.reviewPointsCost = sum((r) => r.reviewPointsCost)
    totals.workCost = sum((r) => r.workCost)
    totals.materialCost = sum((r) => r.materialCost)
    totals.netProfit = sum((r) => r.netProfit)
    totals.profitPercent = pct(totals.netProfit, totals.amountPayableToSellerSumm)
  }

  gridApi.value?.setGridOption('pinnedBottomRowData', [totals])
}

const mainColumnDefsOzonV1 = [
  {
    field: 'articleName',
    headerName: 'Имя артикула',
    minWidth: 300,
    sort: 'asc',
    sortIndex: 0,
    pinned: 'left',
  },
  { field: 'sku', headerName: 'SKU', minWidth: 140, maxWidth: 150 },
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

const mainColumnDefsWb = [
  {
    field: 'supplierArticleName',
    headerName: 'Артикул поставщика',
    minWidth: 200,
    sort: 'asc',
    sortIndex: 0,
    pinned: 'left',
  },
  {
    field: 'articleName',
    headerName: 'Предмет',
    minWidth: 150,
  },
  {
    field: 'brand',
    headerName: 'Бренд',
    minWidth: 130,
  },
  { field: 'sku', headerName: 'Код номенклатуры', minWidth: 140, maxWidth: 150 },
  {
    field: 'retailPriceSumm',
    headerName: 'Цена розничная',
    minWidth: 150,
    filter: 'agNumberColumnFilter',
    valueFormatter: (params: any) => params.value?.toFixed(2),
  },
  {
    field: 'amountPayableToSellerSumm',
    headerName: 'К перечислению Продавцу за реализованный Товар',
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
    field: 'logisticsFee',
    headerName: 'Логистика',
    minWidth: 100,
    filter: 'agNumberColumnFilter',
    valueFormatter: (params: any) => params.value?.toFixed(2),
  },
  {
    field: 'cancelledQuantity',
    headerName: 'Количество отмен',
    minWidth: 100,
    filter: 'agNumberColumnFilter',
    valueFormatter: (params: any) => params.value?.toFixed(2),
  },
  {
    field: 'cancelledSumm',
    headerName: 'Отмены',
    minWidth: 100,
    filter: 'agNumberColumnFilter',
    valueFormatter: (params: any) => params.value?.toFixed(2),
  },
  {
    field: 'paidAcceptanceSumm',
    headerName: 'Платная приемка',
    minWidth: 100,
    filter: 'agNumberColumnFilter',
    valueFormatter: (params: any) => params.value?.toFixed(2),
  },
  {
    field: 'totalAmountOfFines',
    headerName: 'Штрафы',
    minWidth: 100,
    filter: 'agNumberColumnFilter',
    valueFormatter: (params: any) => params.value?.toFixed(2),
  },
  {
    field: 'returnedQuantity',
    headerName: 'Количество возвратов',
    minWidth: 100,
    filter: 'agNumberColumnFilter',
    valueFormatter: (params: any) => params.value?.toFixed(2),
  },
  {
    field: 'returnedSumm',
    headerName: 'Возвраты',
    minWidth: 100,
    filter: 'agNumberColumnFilter',
    valueFormatter: (params: any) => params.value?.toFixed(2),
  },
  {
    field: 'advertisingCost',
    headerName: 'Реклама',
    minWidth: 100,
    filter: 'agNumberColumnFilter',
    valueFormatter: (params: any) => params.value?.toFixed(2),
  },
  {
    field: 'reviewPointsCost',
    headerName: 'Отзывы за баллы',
    minWidth: 140,
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

function buildOzonV2Columns(feeKeys: string[]): ColDef[] {
  const baseBefore: ColDef[] = [
    {
      field: 'articleName',
      headerName: 'Имя артикула',
      minWidth: 300,
      sort: 'asc' as const,
      sortIndex: 0,
      pinned: 'left',
    },
    { field: 'sku', headerName: 'SKU', minWidth: 140, maxWidth: 150 },
    { field: 'warehouse', headerName: 'Склад', minWidth: 130, maxWidth: 150 },
    {
      field: 'preCommissionAmount',
      headerName: 'Цена продажи',
      minWidth: 160,
      filter: 'agNumberColumnFilter',
      valueFormatter: (p) => p.value?.toFixed(2),
    },
    {
      field: 'quantity',
      headerName: 'Кол-во продаж',
      minWidth: 110,
      filter: 'agNumberColumnFilter',
      valueFormatter: (p) => p.value?.toFixed(2),
    },
    {
      field: 'workCost',
      headerName: 'Стоимость работы',
      minWidth: 140,
      filter: 'agNumberColumnFilter',
      valueFormatter: (p) => p.value?.toFixed(2),
    },
    {
      field: 'materialCost',
      headerName: 'Стоимость материалов',
      minWidth: 160,
      filter: 'agNumberColumnFilter',
      valueFormatter: (p) => p.value?.toFixed(2),
    },
    {
      field: 'unlinkedExpenses',
      headerName: 'Нераспределённые расходы',
      minWidth: 170,
      filter: 'agNumberColumnFilter',
      valueFormatter: (p) => p.value?.toFixed(2),
    },
    {
      field: 'ozonFee',
      headerName: 'Комиссия Озон',
      minWidth: 130,
      filter: 'agNumberColumnFilter',
      valueFormatter: (p) => p.value?.toFixed(2),
    },
    {
      field: 'handlingFee',
      headerName: 'Комиссия за обработку',
      minWidth: 160,
      filter: 'agNumberColumnFilter',
      valueFormatter: (p) => p.value?.toFixed(2),
    },
    {
      field: 'lastMileFee',
      headerName: 'Последняя миля',
      minWidth: 130,
      filter: 'agNumberColumnFilter',
      valueFormatter: (p) => p.value?.toFixed(2),
    },
    {
      field: 'logisticsFee',
      headerName: 'Логистика',
      minWidth: 120,
      filter: 'agNumberColumnFilter',
      valueFormatter: (p) => p.value?.toFixed(2),
    },
  ]

  const dynamicFeeCols: ColDef[] = feeKeys.map((key) => ({
    headerName: key,
    colId: `fee:${key}`,
    valueGetter: (params) => params.data?.additionalFees?.[key] ?? 0,
    filter: 'agNumberColumnFilter',
    minWidth: 140,
    valueFormatter: (p) => (p.value != null ? Number(p.value).toFixed(2) : ''),
  }))

  const tail: ColDef[] = [
    {
      field: 'netProfit',
      headerName: 'Чистая прибыль',
      minWidth: 140,
      filter: 'agNumberColumnFilter',
      valueFormatter: (p) => p.value?.toFixed(2),
    },
    {
      field: 'profitPercent',
      headerName: '% от выручки',
      minWidth: 120,
      filter: 'agNumberColumnFilter',
      valueFormatter: (p) => (p.value != null ? `${p.value.toFixed(2)}%` : 'н/д'),
    },
  ]

  return [...baseBefore, ...dynamicFeeCols, ...tail]
}

async function onExportDataAsExcel() {
  if (!gridApi.value) return
  displayedRows.value = []
  try {
    gridApi.value.forEachNodeAfterFilterAndSort((node) => {
      if (node.data) displayedRows.value!.push(node.data)
    })

    const pinned = gridApi.value.getPinnedBottomRow(0)?.data
    if (pinned) {
      displayedRows.value.push(pinned)
    }

    let queryUrl = ''
    if (activeTab.value?.activeTab === 'ozon1') {
      queryUrl = '/api/FileReader/ExportProcessedResultsV1'
    } else if (activeTab.value?.activeTab === 'ozon2') {
      queryUrl = '/api/FileReader/ExportProcessedResultsV2'
    } else if (activeTab.value?.activeTab === 'wb') {
      queryUrl = '/api/FileReader/ExportProcessedResultsWb'
    } else {
      alert('Неизвестный тип экспорта')
      return
    }

    const plainRows = JSON.parse(JSON.stringify(displayedRows.value))
    const response = await axios.post(queryUrl, plainRows, {
      responseType: 'blob',
      headers: { 'Content-Type': 'application/json' },
    })

    const blob = new Blob([response.data], {
      type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
    })

    const fileName = `processed_results_${activeTab.value?.activeTab || 'data'}.xlsx`
    const url = URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = fileName
    link.click()
    URL.revokeObjectURL(url)
  } catch (err) {
    console.error(err)
    alert('Ошибка при экспорте')
  }
}

async function resetData() {
  gridApi.value?.setGridOption('rowData', [])
  mainColumnDefs.value = []
  gridApi.value?.setGridOption('columnDefs', [])
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
