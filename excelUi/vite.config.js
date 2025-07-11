import vue from '@vitejs/plugin-vue';
import vuetify from 'vite-plugin-vuetify';
import { defineConfig } from 'vite';
import { fileURLToPath, URL } from 'node:url';
export default defineConfig({
    plugins: [
        vue(),
        vuetify({ autoImport: true })
    ],
    resolve: {
        alias: {
            '@': fileURLToPath(new URL('./src', import.meta.url))
        },
        extensions: ['.js', '.ts', '.vue']
    },
    server: {
        port: 8080,
        proxy: {
            '/api': {
                target: 'https://localhost:7093',
                changeOrigin: true,
                secure: false, //
            }
        }
    }
});
