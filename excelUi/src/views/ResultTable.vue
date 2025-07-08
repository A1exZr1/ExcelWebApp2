<template>
  <v-container fluid>
    <v-card>
      <v-card-title>Результаты обработки</v-card-title>

      <v-card-text>
        <ag-grid-vue
          v-if="rows.length > 0"
          class="ag-theme-alpine"
          style="width: 100%; height: 500px"
          :gridOptions="gridOptions"
        />
        <div v-else class="text-subtitle-1 mt-4">Данные пока не загружены.</div>
      </v-card-text>
    </v-card>
  </v-container>
</template>

<script setup lang="ts">
import { AgGridVue } from 'ag-grid-vue3'
import { GridOptions } from 'ag-grid-community'
import type ResultGridItem from './ResultGridItem.ts'
import { computed } from 'vue'

const props = defineProps<{
  rows: ResultGridItem[]
}>()

const columnDefs = [
  { field: 'articleName', headerName: 'Имя артикула', minWidth: 150 },
  { field: 'sku', headerName: 'SKU', minWidth: 120 },
  { field: 'revenue', headerName: 'Выручка', minWidth: 120 },
  { field: 'advertisingCost', headerName: 'Расходы на рекламу', minWidth: 150 },
  { field: 'primeCost', headerName: 'Себестоимость', minWidth: 130 },
  { field: 'netProfit', headerName: 'Чистая прибыль', minWidth: 130 },
  {
    field: 'profitPercent',
    headerName: '% от выручки',
    minWidth: 130,
    valueFormatter: (params: any) => `${params.value.toFixed(2)}%`
  }
]

const gridOptions = computed<GridOptions>(() => ({
  columnDefs,
  rowData: props.rows,
  defaultColDef: {
    sortable: true,
    filter: 'agTextColumnFilter',
    resizable: true,
    flex: 1,
    wrapHeaderText: true,
    autoHeaderHeight: true
  },
  popupParent: document.body,
  overlayNoRowsTemplate: '<span>Нет данных</span>',
  rowSelection: 'single'
}))
</script>

<style scoped>
.ag-theme-alpine {
  width: 100%;
  height: 100%;
}
</style>
