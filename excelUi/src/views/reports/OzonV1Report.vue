<template>
  <ag-grid-vue
    class="ag-theme-alpine"
    style="width: 100%; height: 100%"
    :gridOptions="gridOptions"
    :rowData="rowData"
    @gridReady="onGridReady"
    @filterChanged="onFilterChanged"
  />
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import axios from 'axios'
import { AgGridVue } from 'ag-grid-vue3'
import type { ColDef, GridApi, GridReadyEvent } from 'ag-grid-community'
import ResultGridOzonV1 from '../ResultGridOzonV1'
import {
  collectDisplayedRows,
  exportRows,
  percent,
  reportGridOptions,
  sumRows,
} from './reportGridShared'

const props = defineProps<{
  quickFilterText: string
}>()

const emit = defineEmits<{
  rowsChanged: [hasRows: boolean]
}>()

const gridApi = ref<GridApi>()
const rowData = ref<ResultGridOzonV1[]>([])
const totalRowCount = computed(() => rowData.value.length)

const gridOptions = {
  ...reportGridOptions,
  columnDefs: buildColumns(),
}

watch(
  () => props.quickFilterText,
  (newValue) => {
    gridApi.value?.setGridOption('quickFilterText', newValue)
    updatePinnedTotals()
  },
)

watch(rowData, () => {
  emit('rowsChanged', rowData.value.length > 0)
  updatePinnedTotals()
})

function onGridReady(event: GridReadyEvent) {
  gridApi.value = event.api
  gridApi.value.setGridOption('rowData', rowData.value)
  updatePinnedTotals()
}

function onFilterChanged() {
  updatePinnedTotals()
}

async function loadData() {
  try {
    const response = await axios.get('/api/FileReader/GetOzonV1Results')
    rowData.value = response.data.map(
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
    gridApi.value?.setGridOption('rowData', rowData.value)
    updatePinnedTotals()
  } catch (err) {
    console.error(err)
    alert('Ошибка при получении результатов')
  }
}

async function exportData() {
  if (!gridApi.value) return
  await exportRows('/api/FileReader/ExportProcessedResultsV1', collectDisplayedRows(gridApi.value), 'ozon1')
}

function reset() {
  rowData.value = []
  gridApi.value?.setGridOption('rowData', [])
  gridApi.value?.setGridOption('pinnedBottomRowData', [])
  emit('rowsChanged', false)
}

function updatePinnedTotals() {
  if (!gridApi.value) return

  const rows: ResultGridOzonV1[] = []
  gridApi.value.forEachNodeAfterFilterAndSort((node) => {
    if (node.data) rows.push(node.data)
  })

  const totals: any = { articleName: `Итого: ${rows.length} из ${totalRowCount.value}` }
  totals.totalSumm = sumRows(rows, (r) => r.totalSumm)
  totals.quantity = sumRows(rows, (r) => r.quantity)
  totals.revenue = sumRows(rows, (r) => r.revenue)
  totals.advertisingCost = sumRows(rows, (r) => r.advertisingCost)
  totals.unlinkedExpenses = sumRows(rows, (r) => r.unlinkedExpenses)
  totals.workCost = sumRows(rows, (r) => r.workCost)
  totals.materialCost = sumRows(rows, (r) => r.materialCost)
  totals.netProfit = sumRows(rows, (r) => r.netProfit)
  totals.profitPercent = percent(totals.netProfit, totals.revenue)

  gridApi.value.setGridOption('pinnedBottomRowData', [totals])
}

function buildColumns() {
  return [
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
}

defineExpose({
  loadData,
  exportData,
  reset,
})
</script>
