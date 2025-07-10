<template>
  <div class="d-flex-column" style="width: 100%;">
    <div class="d-flex-column" style="width: 100%;">
      <div class="pl-3 pr-2 d-flex" style="width: 100%; height: 100px;">
        <v-file-input
          v-model="selectedFile"
          :label="'Выбрать '+ props.label"
          accept=".xlsx, .xls"
          prepend-icon="mdi-file-excel"
          hide-details: true
          density="compact"
        />
      </div>
      <div class="pl-2 pr-2 d-flex" style="width: 100%;">
        <v-btn
          color="primary"
          :loading="isLoading"
          :disabled="!selectedFile || isWrongFileSelected"
          hide-details: true
          @click="upload"
          block
        >
          {{ 'Загрузить '+props.label }}
        </v-btn>
      </div>
      <div class="d-flex pl-2 pt-2 pr-2" style="width: 100%;">
        <v-alert
          v-if="message"
          :type="alertType"
          variant="tonal"
          density="compact"
          hide-details
        >
          {{ message }}
        </v-alert>
      </div>
    </div>

  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import axios from 'axios'


const props = defineProps({
  label: String,
  endpoint: String
});

// Emits
const emit = defineEmits(['uploaded', 'upload-complete'])
const isLoading = ref(false)
const selectedFile = ref<File | null>(null)
const isWrongFileSelected = ref(false)
const message = ref('')
const alertType = ref<'info' | 'success' | 'warning' | 'error'>('info')

watch(selectedFile, () => {
    message.value = ''
    isWrongFileSelected.value = false
});

async function upload() {
  if (!selectedFile.value) 
    return

  isLoading.value = true;
  const formData = new FormData();
  formData.append('file', selectedFile.value);
  try {
    const response = await axios.post(props.endpoint!, formData);
    const count = response.data.count;
    message.value = `Загружено ${count} строк(и).`;
    emit('upload-complete', count);
    alertType.value = 'success';
    isWrongFileSelected.value = false;
    emit('uploaded');
  } catch (error: any) {
    message.value = `Ошибка: ${error?.response?.data || error.message}`
    alertType.value = 'error'
    isWrongFileSelected.value = true; // Reset the file input
  }
  isLoading.value = false;
}
</script>
