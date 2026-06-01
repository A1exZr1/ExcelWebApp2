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
import ResultGridWB from '../ResultGridWB'
import {
  collectDisplayedRows,
  exportRows,
  percent,
  reportGridOptions,
  sumRows,
} from './reportGridShared'

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
    const response = await axios.get('/api/FileReader/GetWbResults', {
      params: {
        returnMaterialDamagePercent: props.returnMaterialDamagePercent ?? 15,
      },
    })
    rowData.value = response.data.map(
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
          item.cancellationWorkCost,
          item.workCost,
          item.materialCost,
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
  await exportRows('/api/FileReader/ExportProcessedResultsWb', collectDisplayedRows(gridApi.value), 'wb')
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
  totals.advertisingCost = sumRows(rows, (r) => r.advertisingCost)
  totals.reviewPointsCost = sumRows(rows, (r) => r.reviewPointsCost)
  totals.cancellationWorkCost = sumRows(rows, (r) => r.cancellationWorkCost)
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
      field: 'cancellationWorkCost',
      headerName: 'Расходы работы при отмене',
      minWidth: 160,
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
