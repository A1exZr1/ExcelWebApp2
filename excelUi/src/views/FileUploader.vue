<template>
  <div class="d-flex" style="width: 100%;">
      <div class="pl-3 pr-2 d-flex transition-width" 
        :style="{ width: message ? '60%' : '100%' }">
        <v-file-input
          v-model="selectedFile"
          :label="'Выбрать '+ props.label"
          accept=".xlsx, .xls"
          prepend-icon="mdi-file-excel"
          hide-details: true
          density="compact"
        />
      </div>
    <div class="d-flex pl-2 pr-2 mb-5" style="width: 40%;" v-if="message">
          <template v-if="isMessageLong">
            <v-tooltip location="top">
              <template #activator="{ props: tooltipProps }">
                <v-alert
                  v-bind="tooltipProps"
                  :type="alertType"
                  variant="tonal"
                  density="compact"
                  hide-details
                  class="ellipsis-alert"
                >
                  {{ shortMessage }}
                </v-alert>
              </template>
              <span>{{ message }}</span>
            </v-tooltip>
          </template>
          <v-alert
            v-else
            :type="alertType"
            variant="tonal"
            density="compact"
            hide-details
          >
            {{ message }}
          </v-alert>
      </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import axios from 'axios'

const props = defineProps({
  label: String,
  endpoint: String
});

const selectedFile = ref<File | null>(null)
const isBadFileSelected = ref(false)
const message = ref('')
const alertType = ref<'info' | 'success' | 'warning' | 'error'>('info')
const shortMessage = computed(() => {
  return message.value.length > 50
    ? message.value.slice(0, 50) + '…'
    : message.value
})

const isMessageLong = computed(() => {
  return message.value.length > 50
})

watch(selectedFile, () => {
    message.value = ''
    isBadFileSelected.value = false
});

async function upload() {
  if (!selectedFile.value) 
    return

  const formData = new FormData();
  formData.append('file', selectedFile.value);
  try {
    const response = await axios.post(props.endpoint!, formData);
    const count = response.data.count;
    message.value = `Загружено ${count} строк(и).`;
    alertType.value = 'success';
    isBadFileSelected.value = false;
  } catch (error: any) {
    message.value = `${error?.response?.data || error.message}`
    alertType.value = 'error'
    isBadFileSelected.value = true; // Reset the file input
  }
}

function resetData() {
  selectedFile.value = null;
  isBadFileSelected.value = false;
  message.value = '';
  alertType.value = 'info';
}

defineExpose({
  upload,
  resetData,
  hasFile: computed(() => !!selectedFile.value),
  isBadFileSelected: computed(() => isBadFileSelected.value)
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