import './assets/main.css'

import { createApp } from 'vue'
import App from './App.vue'
import vuetify from './plugins/vuetify'

import { AllCommunityModule, ModuleRegistry } from 'ag-grid-community'

ModuleRegistry.registerModules([AllCommunityModule])

const app = createApp(App)
app.use(vuetify)
app.mount('#app')
