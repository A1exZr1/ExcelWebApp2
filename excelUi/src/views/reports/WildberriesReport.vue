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
import ResultGridWB from '../ResultGridWB'
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
  returnMaterialDamagePercent?: number
}>()

const emit = defineEmits<{
  rowsChanged: [hasRows: boolean]
}>()

const gridApi = ref<GridApi>()
const rowData = ref<ResultGridWB[]>([])
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
    const results = await reportResultsClient.getWbResults(props.returnMaterialDamagePercent ?? 15)
    rowData.value = results.map(
      (item) =>
        new ResultGridWB(
          item.articleName ?? '',
          item.sku ?? '',
          item.supplierArticleName ?? '',
          item.brand ?? '',
          item.quantity ?? 0,
          item.retailPriceSumm ?? 0,
          item.amountPayableToSellerSumm ?? 0,
          item.logisticsFee ?? 0,
          item.cancelledQuantity ?? 0,
          item.cancelledSumm ?? 0,
          item.paidAcceptanceSumm ?? 0,
          item.totalAmountOfFines ?? 0,
          item.returnedQuantity ?? 0,
          item.returnedSumm ?? 0,
          item.returnMaterialDamageCost ?? 0,
          item.advertisingCost ?? 0,
          item.reviewPointsCost ?? 0,
          item.cancellationWorkQuantity ?? 0,
          item.cancellationWorkCost ?? 0,
          item.cancellationMaterialDamageCost ?? 0,
          item.workCost ?? null,
          item.materialCost ?? null,
          item.netProfit ?? 0,
          item.profitPercent ?? null,
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
  await exportRows(ReportType.Wildberries, collectDisplayedRows(gridApi.value))
}

function reset() {
  rowData.value = []
  gridApi.value?.setGridOption('rowData', [])
  gridApi.value?.setGridOption('pinnedBottomRowData', [])
  emit('rowsChanged', false)
}

function updatePinnedTotals() {
  if (!gridApi.value) return

  const rows: ResultGridWB[] = []
  gridApi.value.forEachNodeAfterFilterAndSort((node) => {
    if (node.data) rows.push(node.data)
  })

  const totals: any = { supplierArticleName: `Итого: ${rows.length} из ${totalRowCount.value}` }
  totals.quantity = sumRows(rows, (r) => r.quantity)
  totals.retailPriceSumm = sumRows(rows, (r) => r.retailPriceSumm)
  totals.amountPayableToSellerSumm = sumRows(rows, (r) => r.amountPayableToSellerSumm)
  totals.logisticsFee = sumRows(rows, (r) => r.logisticsFee)
  totals.cancelledQuantity = sumRows(rows, (r) => r.cancelledQuantity)
  totals.cancelledSumm = sumRows(rows, (r) => r.cancelledSumm)
  totals.paidAcceptanceSumm = sumRows(rows, (r) => r.paidAcceptanceSumm)
  totals.totalAmountOfFines = sumRows(rows, (r) => r.totalAmountOfFines)
  totals.returnedQuantity = sumRows(rows, (r) => r.returnedQuantity)
  totals.returnedSumm = sumRows(rows, (r) => r.returnedSumm)
  totals.returnMaterialDamageCost = sumRows(rows, (r) => r.returnMaterialDamageCost)
  totals.advertisingCost = sumRows(rows, (r) => r.advertisingCost)
  totals.reviewPointsCost = sumRows(rows, (r) => r.reviewPointsCost)
  totals.cancellationWorkQuantity = sumRows(rows, (r) => r.cancellationWorkQuantity)
  totals.cancellationWorkCost = sumRows(rows, (r) => r.cancellationWorkCost)
  totals.cancellationMaterialDamageCost = sumRows(rows, (r) => r.cancellationMaterialDamageCost)
  totals.workCost = sumRows(rows, (r) => r.workCost)
  totals.materialCost = sumRows(rows, (r) => r.materialCost)
  totals.netProfit = sumRows(rows, (r) => r.netProfit)
  totals.profitPercent = percent(totals.netProfit, totals.amountPayableToSellerSumm)

  gridApi.value.setGridOption('pinnedBottomRowData', [totals])
}

function buildColumns() {
  return [
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
      field: 'returnMaterialDamageCost',
      headerName: '15% материалов при возвратах',
      minWidth: 180,
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
      field: 'cancellationWorkQuantity',
      headerName: 'Количество работ при отменах',
      minWidth: 160,
      filter: 'agNumberColumnFilter',
      valueFormatter: (params: any) => params.value?.toFixed(2),
    },
    {
      field: 'cancellationWorkCost',
      headerName: 'Расходы работы при отмене',
      minWidth: 160,
      filter: 'agNumberColumnFilter',
      valueFormatter: (params: any) => params.value?.toFixed(2),
    },
    {
      field: 'cancellationMaterialDamageCost',
      headerName: '15% материалов при отменах',
      minWidth: 180,
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
