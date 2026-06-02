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
import { AgGridVue } from 'ag-grid-vue3'
import type { ColDef, GridApi, GridReadyEvent } from 'ag-grid-community'
import ResultGridOzonV2 from '../ResultGridOzonV2'
import { reportResultsClient } from '../../api/fileReaderApi'
import {
  collectDisplayedRows,
  exportRows,
  percent,
  reportGridOptions,
  sumRows,
} from './reportGridShared'
import { ReportType } from './reportTypes'

const props = defineProps<{
  quickFilterText: string
}>()

const emit = defineEmits<{
  rowsChanged: [hasRows: boolean]
}>()

const gridApi = ref<GridApi>()
const rowData = ref<ResultGridOzonV2[]>([])
const feeKeys = ref<string[]>([])
const totalRowCount = computed(() => rowData.value.length)

const gridOptions = {
  ...reportGridOptions,
  columnDefs: buildColumns([]),
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
    const results = await reportResultsClient.getOzonV2Results()

    feeKeys.value = Array.from(
      new Set(results.flatMap((x) => Object.keys(x.additionalFees ?? {}))),
    )
    gridApi.value?.setGridOption('columnDefs', buildColumns(feeKeys.value))

    rowData.value = results.map(
      (item) =>
        new ResultGridOzonV2(
          item.articleName ?? '',
          item.sku ?? '',
          item.warehouse ?? '',
          item.preCommissionAmount ?? 0,
          item.quantity ?? 0,
          item.workCost ?? null,
          item.materialCost ?? null,
          item.unlinkedExpenses ?? 0,
          item.ozonFee ?? 0,
          item.handlingFee ?? 0,
          item.lastMileFee ?? 0,
          item.logisticsFee ?? 0,
          item.netProfit ?? 0,
          item.profitPercent ?? 0,
          item.additionalFees ?? {},
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
  await exportRows(ReportType.OzonV2, collectDisplayedRows(gridApi.value))
}

function reset() {
  rowData.value = []
  feeKeys.value = []
  gridApi.value?.setGridOption('columnDefs', buildColumns([]))
  gridApi.value?.setGridOption('rowData', [])
  gridApi.value?.setGridOption('pinnedBottomRowData', [])
  emit('rowsChanged', false)
}

function updatePinnedTotals() {
  if (!gridApi.value) return

  const rows: ResultGridOzonV2[] = []
  gridApi.value.forEachNodeAfterFilterAndSort((node) => {
    if (node.data) rows.push(node.data)
  })

  const totals: any = { articleName: `Итого: ${rows.length} из ${totalRowCount.value}` }
  totals.preCommissionAmount = sumRows(rows, (r) => r.preCommissionAmount)
  totals.quantity = sumRows(rows, (r) => r.quantity)
  totals.workCost = sumRows(rows, (r) => r.workCost)
  totals.materialCost = sumRows(rows, (r) => r.materialCost)
  totals.unlinkedExpenses = sumRows(rows, (r) => r.unlinkedExpenses)
  totals.ozonFee = sumRows(rows, (r) => r.ozonFee)
  totals.handlingFee = sumRows(rows, (r) => r.handlingFee)
  totals.lastMileFee = sumRows(rows, (r) => r.lastMileFee)
  totals.logisticsFee = sumRows(rows, (r) => r.logisticsFee)
  totals.netProfit = sumRows(rows, (r) => r.netProfit)
  totals.profitPercent = percent(totals.netProfit, totals.preCommissionAmount)

  const fees: Record<string, number> = {}
  for (const key of feeKeys.value) {
    fees[key] = sumRows(rows, (r) => r.additionalFees?.[key])
  }
  totals.additionalFees = fees

  gridApi.value.setGridOption('pinnedBottomRowData', [totals])
}

function buildColumns(dynamicFeeKeys: string[]): ColDef[] {
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

  const dynamicFeeCols: ColDef[] = dynamicFeeKeys.map((key) => ({
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

defineExpose({
  loadData,
  exportData,
  reset,
})
</script>
