<template>
  <v-dialog width="700" persistent :model-value="isVisible">
    <v-card style="height: 100vh; display: flex; flex-direction: column">
      <v-container height="300">
        <v-tabs v-model="activeTab" align-tabs="center" color="primary">
          <v-tab value="ozon1" text="Ozon V1" style="min-width: 120px; height: 50px" />
          <v-tab value="ozon2" text="Ozon V2" style="min-width: 120px; height: 50px" />
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
                endpoint="/api/FileReader/ReadAccrual"
                ref="accrualUploader"
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
                ref="primeUploader"
              />
            </div>
          </v-tabs-window-item>
          <v-tabs-window-item style="height: 100%" value="ozon2"> </v-tabs-window-item>
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
const accrualUploader = ref()
const adsUploader = ref()
const primeUploader = ref()
const isLoading = ref(false)

const props = defineProps({
  isVisible: Boolean,
})

const isAnyBadFileSelected = computed(
  () =>
    accrualUploader.value?.isBadFileSelected ||
    adsUploader.value?.isBadFileSelected ||
    primeUploader.value?.isBadFileSelected,
)

const canUpload = computed(
  () =>
    accrualUploader.value?.hasFile &&
    adsUploader.value?.hasFile &&
    primeUploader.value?.hasFile &&
    !isAnyBadFileSelected.value,
)

const emit = defineEmits(['confirmed', 'canceled'])

watch(props, (newValue) => {
  if (newValue.isVisible) {
  }
})

async function onConfirmClick() {
  if (activeTab.value !== 'ozon1') {
    emit('confirmed')
    return
  }

  if (!canUpload.value) return

  try {
    isLoading.value = true
    await Promise.all([
      accrualUploader.value?.upload(),
      adsUploader.value?.upload(),
      primeUploader.value?.upload(),
    ])
    if (isAnyBadFileSelected.value) return
    emit('confirmed')
  } finally {
    isLoading.value = false
  }
}

function onCancelClick() {
  emit('canceled')
}
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
