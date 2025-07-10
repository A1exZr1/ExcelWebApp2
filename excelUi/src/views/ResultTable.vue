<template>
  <div class="pl-2 pr-2 d-flex-column" style="width: 100%; height: 90%;">
    <div class="pb-3 d-flex-column" style="width: 100%;">
      <v-card-title>Результаты обработки</v-card-title>
      <v-row>
        <v-col cols="6">
          <v-text-field
            v-model="searchQuickFilterText"
            label="Поиск"
            prepend-icon="mdi-magnify"
            clearable
            hide-details
            density="compact"
          />
        </v-col>
        <v-col cols="2">
          <v-btn
            color="primary"
            :disabled="props.isProcessDisabled"
            @click="loadData"
          >
            Обработать данные
          </v-btn>
        </v-col>
        <v-col cols="2">
          <v-btn
            color="primary"
            :disabled="props.isProcessDisabled"
            @click="onExportDataAsExcel"
          >
            Экспорт данных (xlsx)
          </v-btn>
        </v-col>
        <v-col cols="2">
          <v-btn
            color="red"
            block
            @click="resetData"
          >
            Сброс
          </v-btn>
        </v-col>
      </v-row>
    </div>
    <div style="width: 100%; height: 95%;">
      <ag-grid-vue
        class="ag-theme-alpine"
        style="width: 100%; height: 100%"
        :gridOptions="gridOptions"
        :rowData="rowMainData"
        @gridReady="onGridReady"
        @filterChanged="onFilterChanged"
      />
    </div>
    <div class="pt-2 pr-2 pl-2 d-flex justify" style="font-size: 0.85rem;">
      Показано: {{ filteredRowCount }} из {{ totalRowCount }}
    </div>
  </div>
</template>

<script setup lang="ts">
import { AgGridVue,  } from 'ag-grid-vue3'
import { GridOptions, ColDef, GridApi, GridReadyEvent } from 'ag-grid-community'
import ResultGridItem from './ResultGridItem.ts'
import { ref, watch } from 'vue'
import axios from 'axios'

const emit = defineEmits(['resetData']);
const searchQuickFilterText = ref('');
const gridApi = ref<GridApi>();
const rowMainData = ref<ResultGridItem[]>();
const totalRowCount = ref(0);
const filteredRowCount = ref(0);

const props = defineProps<{
  isProcessDisabled: boolean;
}>()

watch(searchQuickFilterText, (newValue) => {
  if (!gridApi.value) 
    return;
  gridApi.value.setGridOption('quickFilterText', newValue);
  updateRowCounts();
});

/* function updateRowCounts() {
  if (!gridApi.value) return;
  // All rows
  totalRowCount.value = (gridApi.value as any).getModel().getRowCount();
  // Filtered rows
  filteredRowCount.value = gridApi.value.getDisplayedRowCount();
} */

/* function updateRowCounts() {
  if (!gridApi.value) return;

  const model = (gridApi.value as any).getModel?.();
  if (!model || typeof model.getRowCount !== 'function') return;

  filteredRowCount.value = gridApi.value.getDisplayedRowCount();
  totalRowCount.value = rowMainData.value?.length || 0;
} */

function updateRowCounts() {
  if (!gridApi.value) return;

  totalRowCount.value = rowMainData.value?.length ?? 0;
  filteredRowCount.value = gridApi.value?.getDisplayedRowCount();
}

// Update counts when grid is ready or data changes
watch(rowMainData, () => {
  updateRowCounts();
});


function onFilterChanged() {
  updateRowCounts();
}

const mainColumnDefs = [
  { field: 'articleName', headerName: 'Имя артикула', minWidth: 300, sort: 'asc', sortIndex: 0},
  { field: 'sku', headerName: 'SKU', minWidth: 150 },
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

const gridOptions: GridOptions = {
  columnDefs:  mainColumnDefs as ColDef<any, any>[],
  defaultColDef: {
    sortable: true,
    filter: 'agTextColumnFilter',
    resizable: true,
    flex: 1,
    wrapHeaderText: true,
    autoHeaderHeight: true
  },
  popupParent: document.body,
  rowModelType: 'clientSide',
  overlayNoRowsTemplate: '<span>Нет данных</span>',
  rowSelection: 'single',
/*   getContextMenuItems: (params) => [
    'copy',
    'copyWithHeaders',
    'export'
  ], */
  enableCellTextSelection: true,
/*   statusBar: {
        statusPanels: [
          { statusPanel: 'agTotalAndFilteredRowCountComponent', align: 'left'},
        ],
    } */
};

async function onGridReady(event: GridReadyEvent) {
  gridApi.value = event.api;
  gridApi.value.setGridOption('rowData', []);
  updateRowCounts();
}

async function loadData() {
  if (!gridApi.value) 
    return;
  
  try {
    const response = await axios.get('/api/FileReader/GetProcessedResults')

    rowMainData.value = response.data.map((item: any) =>
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
    gridApi.value.setGridOption('rowData', rowMainData.value);
    updateRowCounts();
  } catch (err) {
    console.error(err)
    alert('Ошибка при получении результатов')
  }
}

function onExportDataAsCsv() {
  if (!gridApi.value) 
    return;

  const params = {
    fileName: 'exported_data.csv',
    columnKeys: mainColumnDefs.map(col => col.field)
  };
  
  gridApi.value.exportDataAsCsv(params);
}

async function onExportDataAsExcel() {
  try {
    const response = await axios.get('/api/FileReader/ExportProcessedResults', {
      responseType: 'blob'
    });

    const blob = new Blob([response.data], {
      type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
    });

    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = 'processed_results.xlsx';
    link.click();
    URL.revokeObjectURL(url);
  } catch (err) {
    console.error(err);
    alert('Ошибка при экспорте');
  }
}

function resetData() {
  if (!gridApi.value) 
    return;
  
  gridApi.value.setGridOption('rowData', []);
  rowMainData.value = [];
  searchQuickFilterText.value = '';
  emit('resetData');
}

</script>

<style scoped>
.ag-theme-alpine {
  width: 100%;
  height: 100%;
}
</style>
