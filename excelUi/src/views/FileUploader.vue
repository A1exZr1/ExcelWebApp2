<template>
  <div class="d-flex flex-column" style="width: 100%">
    <div class="pl-3 pr-2 d-flex transition-width" :style="{ width: message ? '100%' : '100%' }">
      <v-file-input
        v-model="selectedFile"
        :label="'Выбрать ' + props.label"
        accept=".xlsx, .xls"
        prepend-icon="mdi-file-excel"
        density="compact"
        v-bind="$attrs"
        :error-messages="alertType === 'error' ? shortMessage : ''"
        :hint="alertType === 'success' ? shortMessage : ''"
        :persistent-hint="alertType === 'success'"
        :loading="isUploading"
      >
        <template #append-inner>
          <v-tooltip v-if="alertType === 'error' && isMessageLong" location="bottom">
            <template #activator="{ props: tip }">
              <v-icon v-bind="tip" size="18" class="ml-1" color="error"> mdi-alert-circle </v-icon>
            </template>
            <div style="max-width: 360px; white-space: pre-wrap">{{ message }}</div>
          </v-tooltip>
        </template>
      </v-file-input>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import {
  getApiErrorMessage,
  reportFilesClient,
  toFileParameter,
} from '../api/fileReaderApi'
import type { UploadFileResponse } from '../api/client'
import { FileUploadType } from './reports/reportTypes'

const props = defineProps<{
  label?: string
  uploadType: FileUploadType
}>()

const selectedFile = ref<File | File[] | null>(null)
const isBadFileSelected = ref(false)
const isUploaded = ref(false)
const isUploading = ref(false)
const uploadPromise = ref<Promise<void> | null>(null)
const uploadRequestId = ref(0)
const message = ref('')
const alertType = ref<'info' | 'success' | 'warning' | 'error'>('info')
const shortMessage = computed(() => {
  return message.value.length > 50 ? message.value.slice(0, 50) + '…' : message.value
})

const isMessageLong = computed(() => {
  return message.value.length > 50
})

watch(selectedFile, () => {
  resetUploadState()

  if (getFiles().length) {
    void upload()
  }
})

function getFiles(): File[] {
  if (!selectedFile.value) return []
  return Array.isArray(selectedFile.value) ? selectedFile.value : [selectedFile.value]
}

async function upload() {
  const files = getFiles()
  if (!files.length) return
  if (isUploaded.value && !isBadFileSelected.value) return
  if (uploadPromise.value) return uploadPromise.value

  const requestId = ++uploadRequestId.value
  uploadPromise.value = uploadSelectedFiles(files, requestId)

  try {
    await uploadPromise.value
  } finally {
    if (requestId === uploadRequestId.value) {
      uploadPromise.value = null
    }
  }
}

async function uploadSelectedFiles(files: File[], requestId: number) {
  try {
    isUploading.value = true
    message.value = 'Загрузка файла...'
    alertType.value = 'info'
    const response = await uploadByType(props.uploadType, files)
    if (requestId !== uploadRequestId.value) return

    const count = response.count
    message.value = `Загружено ${count} строк(и).`
    alertType.value = 'success'
    isBadFileSelected.value = false
    isUploaded.value = true
  } catch (error: unknown) {
    if (requestId !== uploadRequestId.value) return

    message.value = getApiErrorMessage(error)
    alertType.value = 'error'
    isBadFileSelected.value = true
    isUploaded.value = false
  } finally {
    if (requestId === uploadRequestId.value) {
      isUploading.value = false
    }
  }
}

function uploadByType(uploadType: FileUploadType, files: File[]): Promise<UploadFileResponse> {
  const firstFile = files[0] ? toFileParameter(files[0]) : undefined

  switch (uploadType) {
    case FileUploadType.OzonV1Accrual:
      return reportFilesClient.readAccrualV1(firstFile)
    case FileUploadType.OzonV1Advertisement:
      return reportFilesClient.readAdvertisment(firstFile)
    case FileUploadType.OzonV1PrimeCost:
      return reportFilesClient.readPrimeCostModel(firstFile)
    case FileUploadType.OzonV2Accrual:
      return reportFilesClient.readAccrualV2(firstFile)
    case FileUploadType.OzonV2PrimeCost:
      return reportFilesClient.readPrimeCostModel(firstFile)
    case FileUploadType.WildberriesAccruals:
      return reportFilesClient.readAccrualsWb(files.map(toFileParameter))
    case FileUploadType.WildberriesPrimeCost:
      return reportFilesClient.primeCostModelWb(firstFile)
    case FileUploadType.WildberriesCancellations:
      return reportFilesClient.readWbCancellations(firstFile)
    default:
      throw new Error(`Unknown upload type: ${uploadType}`)
  }
}

function resetData() {
  selectedFile.value = null
  resetUploadState()
}

function resetUploadState() {
  uploadRequestId.value++
  isBadFileSelected.value = false
  isUploaded.value = false
  isUploading.value = false
  uploadPromise.value = null
  message.value = ''
  alertType.value = 'info'
}

defineExpose({
  upload,
  resetData,
  hasFile: computed(() => !!selectedFile.value),
  isBadFileSelected: computed(() => isBadFileSelected.value),
  isUploaded: computed(() => isUploaded.value),
  isUploading: computed(() => isUploading.value),
})
</script>
<style>
.transition-width {
  transition: width 0.3s ease;
}

.ellipsis-alert {
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: 100%;
}
</style>
