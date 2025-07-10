<template>
    <div class="pt-4 pl-2 pr-2 pb-3 d-flex-column" style="width: 100%; height: 100%;">
        <v-expansion-panels
            v-model="panelIndex"
            :disabled="!canProcess">
            <v-expansion-panel>
                <v-expansion-panel-title v-slot="{ expanded }">
                    <v-row no-gutters>
                    <v-col class="d-flex justify-start" cols="4">
                        Выбор и загрузка файлов
                    </v-col>
                    <v-col
                        class="text--secondary"
                        cols="8"
                    >
                        <v-fade-transition leave-absolute>
                        <span v-if="expanded"></span>
                        <v-row
                            v-else
                            style="width: 100%"
                            no-gutters
                        >
                            <v-col class="d-flex justify-start" cols="4">
                                Начисления: {{ accrualCount }}
                            </v-col>
                            <v-col class="d-flex justify-start" cols="4">
                                Реклама: {{ adsCount }}
                            </v-col>
                            <v-col class="d-flex justify-start" cols="4">
                                Себестоимость: {{ primeCount  }}
                            </v-col>
                        </v-row>
                        </v-fade-transition>
                    </v-col>
                    </v-row>
                </v-expansion-panel-title>
                <v-expansion-panel-text persistent>
                    <div class="pb-3 d-flex">
                        <FileUploader
                            label="файл начислений"
                            endpoint="/api/FileReader/ReadAccrual"
                            @uploaded="onAccrualUploaded"
                            @reset="resetData"
                            @upload-complete="accrualCount = $event"
                            />
                        <FileUploader
                            label="файл рекламы"
                            endpoint="/api/FileReader/ReadAdvertisment"
                            @uploaded="onAdsUploaded"
                            @reset="resetData"
                            @upload-complete="adsCount = $event"
                        />
                        <FileUploader
                            label="файл себестоимости"
                            endpoint="/api/FileReader/PrimeCostModel"
                            @uploaded="onPrimeUploaded"
                            @reset="resetData"
                            @upload-complete="primeCount = $event"
                        />

                    </div>
                </v-expansion-panel-text>
            </v-expansion-panel>
        </v-expansion-panels>
        
        <div class="d-flex" style="width: 100%; height: 90%;">
            <ResultTable 
                :isProcessDisabled="!canProcess"
                @resetData="resetData"/>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import FileUploader from './FileUploader.vue';
import ResultTable from './ResultTable.vue';

const accrualUploaded = ref(false);
const adsUploaded = ref(false)
const primeUploaded = ref(false)
const accrualCount = ref(0);
const adsCount = ref(0);
const primeCount = ref(0);
const panelIndex = ref<number[]>([0]) // Start with the panel open

const canProcess = computed(() => accrualUploaded.value && adsUploaded.value && primeUploaded.value)

watch(canProcess, (newValue) => {
  if (!newValue) {
    panelIndex.value = [0]  // open the panel
  } else {
    panelIndex.value = []   // close the panel
  }
}, { immediate: true })

function onAccrualUploaded() {
  accrualUploaded.value = true
}
function onAdsUploaded() {
  adsUploaded.value = true
}
function onPrimeUploaded() {
  primeUploaded.value = true
}

function resetData() {
  accrualUploaded.value = false
  adsUploaded.value = false
  primeUploaded.value = false
  accrualCount.value = 0
    adsCount.value = 0
    primeCount.value = 0
    panelIndex.value = [0]
}

</script>

