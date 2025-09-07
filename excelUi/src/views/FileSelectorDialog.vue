<template>
  <v-dialog width="700" persistent :model-value="isVisible">
    <v-card style="height: 100vh; display: flex; flex-direction: column">
      <v-container height="300">
        <v-tabs v-model="activeTab" align-tabs="center" color="primary">
          <v-tab value="ozon1" text="Ozon V1" style="min-width: 120px; height: 50px" />
          <v-tab value="ozon2" text="Ozon V2" style="min-width: 120px; height: 50px" />
          <v-tab value="wb" text="Wildberries" style="min-width: 120px; height: 50px" />
        </v-tabs>

        <v-tabs-window
          class="d-flex flex-column"
          style="height: 95%; width: 100%"
          v-model="activeTab"
        >
          <v-tabs-window-item value="ozon1" style="height: 100%">
            <div
              class="d-flex flex-column pa-2"
              style="width: 100%; height: 100%; overflow-y: auto"
            >
              <FileUploader
                class="pb-2"
                label="файл начислений"
                endpoint="/api/FileReader/ReadAccrualV1"
                ref="accrualUploader1"
              />
              <FileUploader
                class="pb-2"
                label="файл рекламы"
                endpoint="/api/FileReader/ReadAdvertisment"
                ref="adsUploader"
              />
              <FileUploader
                label="файл себестоимости"
                endpoint="/api/FileReader/PrimeCostModel"
                ref="primeUploader1"
              />
            </div>
          </v-tabs-window-item>
          <v-tabs-window-item style="height: 100%" value="ozon2">
            <div
              class="d-flex flex-column pa-2"
              style="width: 100%; height: 100%; overflow-y: auto"
            >
              <FileUploader
                class="pb-2"
                label="файл отчёта по товарам"
                endpoint="/api/FileReader/ReadAccrualV2"
                ref="accrualUploader2"
              />
              <FileUploader
                label="файл себестоимости"
                endpoint="/api/FileReader/PrimeCostModel"
                ref="primeUploader2"
              />
            </div>
          </v-tabs-window-item>
          <v-tabs-window-item style="height: 100%" value="wb">
            <div
              class="d-flex flex-column pa-2"
              style="width: 100%; height: 100%; overflow-y: auto"
            >
              <FileUploader
                class="pb-2"
                label="файлы отчётов"
                chips
                multiple
                endpoint="/api/FileReader/ReadAccrualsWb"
                ref="accrualsUploaderWb"
              />
              <FileUploader
                label="файл себестоимости"
                endpoint="/api/FileReader/PrimeCostModelWb"
                ref="primeUploaderWb"
              />
            </div>
          </v-tabs-window-item>
        </v-tabs-window>
      </v-container>
      <v-card-actions>
        <v-spacer />
        <v-btn
          color="primary"
          hide-details
          :disabled="!canUpload"
          :loading="isLoading"
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

const activeTab = ref<string>('ozon1')
const accrualUploader1 = ref()
const accrualUploader2 = ref()
const adsUploader = ref()
const primeUploader1 = ref()
const primeUploader2 = ref()
const accrualsUploaderWb = ref()
const primeUploaderWb = ref()
const isLoading = ref(false)

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
    primeUploaderWb.value?.isBadFileSelected,
)

const canUpload = computed(() => {
  if (activeTab.value === 'ozon1') {
    return (
      accrualUploader1.value?.hasFile &&
      adsUploader.value?.hasFile &&
      primeUploader1.value?.hasFile &&
      !isAnyBadFileSelected.value
    )
  } else if (activeTab.value === 'ozon2') {
    return (
      accrualUploader2.value?.hasFile &&
      primeUploader2.value?.hasFile &&
      !isAnyBadFileSelected.value
    )
  } else {
    return (
      accrualsUploaderWb.value?.hasFile &&
      primeUploaderWb.value?.hasFile &&
      !isAnyBadFileSelected.value
    )
  }
})

const emit = defineEmits(['confirmed', 'canceled'])

watch(props, (newValue) => {
  if (newValue.isVisible) {
  }
})

async function onConfirmClick() {
  if (!canUpload.value) return

  if (activeTab.value == 'ozon1') {
    try {
      isLoading.value = true
      await Promise.all([
        accrualUploader1.value?.upload(),
        adsUploader.value?.upload(),
        primeUploader1.value?.upload(),
      ])
      if (isAnyBadFileSelected.value) return
      emit('confirmed')
    } finally {
      isLoading.value = false
    }
  }

  if (activeTab.value == 'ozon2') {
    try {
      isLoading.value = true
      await Promise.all([accrualUploader2.value?.upload(), primeUploader2.value?.upload()])
      if (isAnyBadFileSelected.value) return
      emit('confirmed')
    } finally {
      isLoading.value = false
    }
  }

  if (activeTab.value == 'wb') {
    try {
      isLoading.value = true
      await Promise.all([accrualsUploaderWb.value?.upload(), primeUploaderWb.value?.upload()])
      if (isAnyBadFileSelected.value) return
      emit('confirmed')
    } finally {
      isLoading.value = false
    }
  }
}

function onCancelClick() {
  emit('canceled')
}

defineExpose({
  activeTab: activeTab,
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
