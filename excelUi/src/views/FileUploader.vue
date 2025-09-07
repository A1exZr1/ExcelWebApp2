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
/* eslint-disable @typescript-eslint/no-explicit-any */
/* eslint- */
import { ref, watch, computed } from 'vue'
import axios from 'axios'

const props = defineProps({
  label: String,
  endpoint: String,
})

const selectedFile = ref<File | File[] | null>(null)
const isBadFileSelected = ref(false)
const message = ref('')
const alertType = ref<'info' | 'success' | 'warning' | 'error'>('info')
const shortMessage = computed(() => {
  return message.value.length > 50 ? message.value.slice(0, 50) + '…' : message.value
})

const isMessageLong = computed(() => {
  return message.value.length > 50
})

watch(selectedFile, () => {
  message.value = ''
  isBadFileSelected.value = false
})

function getFiles(): File[] {
  if (!selectedFile.value) return []
  return Array.isArray(selectedFile.value) ? selectedFile.value : [selectedFile.value]
}

async function upload() {
  const files = getFiles()
  if (!files.length) return

  const formData = new FormData()
  for (const f of files) {
    formData.append('file', f, f.name)
  }

  console.log('selectedFile', selectedFile.value)

  try {
    const response = await axios.post(props.endpoint!, formData)
    const count = response.data.count
    message.value = `Загружено ${count} строк(и).`
    alertType.value = 'success'
    isBadFileSelected.value = false
  } catch (error: any) {
    message.value = `${error?.response?.data || error.message}`
    alertType.value = 'error'
    isBadFileSelected.value = true // Reset the file input
  }
}

function resetData() {
  selectedFile.value = null
  isBadFileSelected.value = false
  message.value = ''
  alertType.value = 'info'
}

defineExpose({
  upload,
  resetData,
  hasFile: computed(() => !!selectedFile.value),
  isBadFileSelected: computed(() => isBadFileSelected.value),
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
