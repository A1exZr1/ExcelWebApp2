<template>
    <div class="pt-4 pl-2 pr-2 pb-3 d-flex-column" style="width: 100%; height: 100%;">
        <div class="d-flex" style="width: 100%; height: 25%;">
            <div class="d-flex-column" style="width: 70%;">
                <FileUploader
                    label="файл начислений"
                    endpoint="/api/FileReader/ReadAccrual"
                    ref="accrualUploader"/>
                <FileUploader
                    label="файл рекламы"
                    endpoint="/api/FileReader/ReadAdvertisment"
                    ref="adsUploader"/>
                <FileUploader
                    label="файл себестоимости"
                    endpoint="/api/FileReader/PrimeCostModel"
                    ref="primeUploader"/>
            </div>
            
            <div class="pl-3 pr-2 d-flex-column" style="width: 30%;">
                <div class="pb-2 d-flex">
                    <div class="pr-2 d-flex" style="width: 50%;">
                        <v-btn color="primary" 
                            block
                            hide-details: true
                            :disabled="!canUpload "
                            size="large"
                            :loading="isLoading"
                            @click="loadAndProcessAll">
                            Обработать файлы
                        </v-btn>
                    </div>
                    <div class="pl-2 d-flex" style="width: 50%;">
                        <v-btn color="red" 
                            block
                            hide-details: true
                            size="large"
                            @click="resetData">
                            Сброс
                        </v-btn>
                    </div>
                </div>
                <div class="pb-3 pt-3 d-flex">
                    <div class="pr-2 d-flex" style="width: 50%;">
                        <v-btn color="primary" 
                            block
                            hide-details: true
                            size="large"
                            :disabled="!hasRows"
                            @click="onExportDataAsCsv">
                            Экспорт (csv)
                        </v-btn>
                    </div>
                    <div class="pl-2 d-flex" style="width: 50%;">
                        <v-btn color="primary" 
                            block
                            hide-details: true
                            size="large"
                            :disabled="!hasRows"
                            @click="onExportDataAsExcel">
                            Экспорт (xlsx)
                        </v-btn>
                    </div>
                </div>
                <div  class="pb-3 pt-2 d-flex">
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
            </div>
        </div>
        <div class="pr-2 d-flex-column" style="width: 100%; height: 75%;">
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
    </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import FileUploader from './FileUploader.vue';
import { AgGridVue,  } from 'ag-grid-vue3'
import { GridOptions, ColDef, GridApi, GridReadyEvent, RowClassParams } from 'ag-grid-community'
import ResultGridItem from './ResultGridItem'
import axios from 'axios'

const searchQuickFilterText = ref('');
const gridApi = ref<GridApi>();
const rowMainData = ref<ResultGridItem[]>();
const totalRowCount = ref(0);
const filteredRowCount = ref(0);
const accrualUploader = ref()
const adsUploader = ref()
const primeUploader = ref()
const isLoading = ref(false);

const canUpload = computed(() =>
  accrualUploader.value?.hasFile &&
  adsUploader.value?.hasFile &&
  primeUploader.value?.hasFile &&
  !isAnyBadFileSelected.value
);

const isAnyBadFileSelected = computed(() =>
  accrualUploader.value?.isBadFileSelected ||
  adsUploader.value?.isBadFileSelected ||
  primeUploader.value?.isBadFileSelected
);

const hasRows = computed(() => rowMainData.value && rowMainData.value.length > 0);

watch(searchQuickFilterText, (newValue) => {
  if (!gridApi.value) 
    return;
  gridApi.value.setGridOption('quickFilterText', newValue);
  updateRowCounts();
});

function updateRowCounts() {
  if (!gridApi.value) return;

  totalRowCount.value = rowMainData.value?.length ?? 0;
  filteredRowCount.value = gridApi.value?.getDisplayedRowCount();
}

watch(rowMainData, () => {
  updateRowCounts();
});


function onFilterChanged() {
  updateRowCounts();
}

const mainColumnDefs = [
  { field: 'articleName', headerName: 'Имя артикула', minWidth: 300, sort: 'asc', sortIndex: 0},
  { field: 'sku', headerName: 'SKU', minWidth: 150 },
  { field: 'totalSummary', headerName: 'Поступило на счёт', minWidth: 150, valueFormatter: (params: any) => params.value?.toFixed(2) },
  { field: 'revenue', headerName: 'Выручка', minWidth: 120, valueFormatter: (params: any) => params.value?.toFixed(2) },
  { field: 'advertisingCost', headerName: 'Расходы на рекламу', minWidth: 150, valueFormatter: (params: any) => params.value?.toFixed(2) },
  { field: 'primeCost', headerName: 'Себестоимость', minWidth: 130, valueFormatter: (params: any) => params.value?.toFixed(2) },
  { field: 'netProfit', headerName: 'Чистая прибыль', minWidth: 130, valueFormatter: (params: any) => params.value?.toFixed(2) },
  {
    field: 'profitPercent',
    headerName: '% от выручки',
    minWidth: 130,
      valueFormatter: (params: any) => params.value != null ? `${params.value.toFixed(2)}%` : 'н/д'
  }
]

const gridOptions: GridOptions = {
  columnDefs:  mainColumnDefs as ColDef<any, any>[],
  rowClassRules: {
    'red-row': (params: RowClassParams) => params.data?.primeCost === 0
  },
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
  enableCellTextSelection: true,
};

async function onGridReady(event: GridReadyEvent) {
  gridApi.value = event.api;
  gridApi.value.setGridOption('rowData', []);
  updateRowCounts();
}

async function loadAndProcessAll() {
  try {
    isLoading.value = true;
    await Promise.all([
      accrualUploader.value.upload(),
      adsUploader.value.upload(),
      primeUploader.value.upload()
    ]);
    
    if (isAnyBadFileSelected.value) {
        isLoading.value = false;
        return;
    }

    await loadData(); 
  } catch (err) {
    console.error('Ошибка при загрузке файлов', err);
  }
  isLoading.value = false;
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
        item.totalSumm,
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

async function resetData() {
  gridApi.value?.setGridOption('rowData', []);
  rowMainData.value = [];
  searchQuickFilterText.value = '';
  await ResetBackendData();

  accrualUploader.value.resetData();
  adsUploader.value.resetData();
  primeUploader.value.resetData();
}

async function ResetBackendData() {
  try {
    await axios.post('/api/FileReader/Reset');
  } catch (err) {
    console.error(err);
    alert('Ошибка при сбросе данных');
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
</style>