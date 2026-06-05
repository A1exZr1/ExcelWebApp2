<template>
  <v-dialog width="700" persistent :model-value="isVisible">
    <v-card style="height: 100vh; display: flex; flex-direction: column">
      <v-container height="370">
        <v-tabs v-model="activeTab" align-tabs="center" color="primary">
          <v-tab :value="ReportType.OzonV1" text="Ozon V1" style="min-width: 120px; height: 50px" />
          <v-tab :value="ReportType.OzonV2" text="Ozon V2" style="min-width: 120px; height: 50px" />
          <v-tab :value="ReportType.Wildberries" text="Wildberries" style="min-width: 120px; height: 50px" />
        </v-tabs>

        <v-tabs-window
          class="d-flex flex-column"
          style="height: 95%; width: 100%"
          v-model="activeTab"
        >
          <v-tabs-window-item :value="ReportType.OzonV1" style="height: 100%">
            <div
              class="d-flex flex-column pa-2"
              style="width: 100%; height: 100%; overflow-y: auto; justify-content: center;  align-items: center;"
            >
              <FileUploader
                class="pb-2"
                label="файл начислений"
                :upload-type="FileUploadType.OzonV1Accrual"
                ref="accrualUploader1"
              />
              <FileUploader
                class="pb-2"
                label="файл рекламы"
                :upload-type="FileUploadType.OzonV1Advertisement"
                ref="adsUploader"
              />
              <FileUploader
                label="файл себестоимости"
                :upload-type="FileUploadType.OzonV1PrimeCost"
                ref="primeUploader1"
              />
            </div>
          </v-tabs-window-item>
          <v-tabs-window-item style="height: 100%" :value="ReportType.OzonV2">
            <div
              class="d-flex flex-column pa-2"
              style="width: 100%; height: 100%; overflow-y: auto; justify-content: center;  align-items: center;"
            >
              <FileUploader
                class="pb-2"
                label="файл отчёта по товарам"
                :upload-type="FileUploadType.OzonV2Accrual"
                ref="accrualUploader2"
              />
              <FileUploader
                label="файл себестоимости"
                :upload-type="FileUploadType.OzonV2PrimeCost"
                ref="primeUploader2"
              />
            </div>
          </v-tabs-window-item>
          <v-tabs-window-item style="height: 100%" :value="ReportType.Wildberries">
            <div
              class="flex-column pa-2"
              style="width: 100%; height: 100%; overflow-y: auto; justify-content: center;  align-items: center;"
            >
              <FileUploader
                class="pb-2"
                label="файлы отчётов"
                chips
                multiple
                :upload-type="FileUploadType.WildberriesAccruals"
                ref="accrualsUploaderWb"
              />
              <FileUploader
                class="pb-2"
                label="файл себестоимости"
                :upload-type="FileUploadType.WildberriesPrimeCost"
                ref="primeUploaderWb"
              />
              <FileUploader
                class="pb-2"
                label="файл отмен"
                :upload-type="FileUploadType.WildberriesCancellations"
                ref="cancellationsUploaderWb"
              />
              <div class="d-flex ">
                <div class="d-flex ml-3" style="width: 40%">
                  <v-switch
                    v-model="useDefaultDamagePercent"
                    color="primary"
                    label="Использовать
                    стандартное значение"
                  ></v-switch>
                </div>
                <div class="mt-2 ml-5 mr-2" style="width: 60%">
                  <v-text-field
                    v-model="wbReturnMaterialDamagePercentInput"
                    :disabled="useDefaultDamagePercent"
                    label="Потери материалов при возврате/отмене, %"
                    type="number"
                    min="0"
                    max="100"
                    step="1"
                    variant="solo-filled"
                    density="compact"
                    hide-details
                  />
                </div>
              </div>
            </div>
          </v-tabs-window-item>
        </v-tabs-window>
      </v-container>
      <v-card-actions>
        <v-spacer />
        <v-btn
          color="primary"
          hide-details
          :disabled="!canProcess"
          :loading="isProcessing || isActiveUploadInProgress"
          class="small-btn-text"
          @click="onConfirmClick"
        >
          Обработать
        </v-btn>
        <v-btn color="red" hide-details class="small-btn-text" @click="onCancelClick">
          Отмена
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>
<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import FileUploader from './FileUploader.vue'
import { FileUploadType, ReportType } from './reports/reportTypes'

const activeTab = ref<ReportType>(ReportType.OzonV1)
const accrualUploader1 = ref()
const accrualUploader2 = ref()
const adsUploader = ref()
const primeUploader1 = ref()
const primeUploader2 = ref()
const accrualsUploaderWb = ref()
const cancellationsUploaderWb = ref()
const primeUploaderWb = ref()
const wbReturnMaterialDamagePercent = ref(15)
const useDefaultDamagePercent = ref(true)
const isProcessing = ref(false)

const wbReturnMaterialDamagePercentInput = computed({
  get: () => wbReturnMaterialDamagePercent.value,
  set: (value: string | number) => {
    wbReturnMaterialDamagePercent.value = clampPercent(value)
  },
})

const props = defineProps({
  isVisible: Boolean,
})

const isAnyBadFileSelected = computed(
  () =>
    accrualUploader1.value?.isBadFileSelected ||
    accrualUploader2.value?.isBadFileSelected ||
    adsUploader.value?.isBadFileSelected ||
    primeUploader1.value?.isBadFileSelected ||
    primeUploader2.value?.isBadFileSelected ||
    accrualsUploaderWb.value?.isBadFileSelected ||
    cancellationsUploaderWb.value?.isBadFileSelected ||
    primeUploaderWb.value?.isBadFileSelected,
)

watch(useDefaultDamagePercent, (newValue) => {
  if (newValue) {
    wbReturnMaterialDamagePercent.value = 15
  }
})

function clampPercent(value: string | number) {
  const percent = Number(value)

  if (!Number.isFinite(percent)) {
    return 0
  }

  return Math.min(100, Math.max(0, percent))
}

const canUpload = computed(() => {
  if (activeTab.value === ReportType.OzonV1) {
    return (
      accrualUploader1.value?.hasFile &&
      adsUploader.value?.hasFile &&
      primeUploader1.value?.hasFile &&
      !isAnyBadFileSelected.value
    )
  } else if (activeTab.value === ReportType.OzonV2) {
    return (
      accrualUploader2.value?.hasFile &&
      primeUploader2.value?.hasFile &&
      !isAnyBadFileSelected.value
    )
  } else {
    return (
      accrualsUploaderWb.value?.hasFile &&
      primeUploaderWb.value?.hasFile &&
      wbReturnMaterialDamagePercent.value >= 0 &&
      wbReturnMaterialDamagePercent.value <= 100 &&
      !isAnyBadFileSelected.value
    )
  }
})

const activeRequiredUploaders = computed(() => {
  if (activeTab.value === ReportType.OzonV1) {
    return [accrualUploader1.value, adsUploader.value, primeUploader1.value]
  }

  if (activeTab.value === ReportType.OzonV2) {
    return [accrualUploader2.value, primeUploader2.value]
  }

  return [accrualsUploaderWb.value, primeUploaderWb.value]
})

const activeOptionalUploaders = computed(() => {
  if (activeTab.value === ReportType.Wildberries && cancellationsUploaderWb.value?.hasFile) {
    return [cancellationsUploaderWb.value]
  }

  return []
})

const activeUploaders = computed(() => [
  ...activeRequiredUploaders.value,
  ...activeOptionalUploaders.value,
])

const isActiveUploadInProgress = computed(() =>
  activeUploaders.value.some((uploader) => uploader?.isUploading),
)

const areRequiredFilesUploaded = computed(() =>
  activeRequiredUploaders.value.every((uploader) => uploader?.isUploaded),
)

const canProcess = computed(() => canUpload.value && areRequiredFilesUploaded.value && !isAnyBadFileSelected.value)

const emit = defineEmits(['confirmed', 'canceled'])

watch(props, (newValue) => {
  if (newValue.isVisible) {
  }
})

async function onConfirmClick() {
  if (!canUpload.value) return

  try {
    isProcessing.value = true
    await Promise.all(activeUploaders.value.map((uploader) => uploader?.upload()))

    if (!areRequiredFilesUploaded.value || isAnyBadFileSelected.value) return
    emit('confirmed')
  } finally {
    isProcessing.value = false
  }
}

function onCancelClick() {
  emit('canceled')
}

defineExpose({
  activeTab: activeTab,
  wbReturnMaterialDamagePercent: wbReturnMaterialDamagePercent,
})
</script>

<style lang="scss">
.small-btn-text {
  font-size: 0.7rem;
  line-height: 1.2;
  white-space: normal;
  text-align: center;
  padding: 4px 8px;
}
.scroll-inner {
  height: 50vh;
  overflow-y: auto;
}
</style>
