import { app, BrowserWindow } from 'electron'
import path from 'path'
import { exec } from 'child_process'
import { fileURLToPath } from 'url'
import http from 'http'

const __filename = fileURLToPath(import.meta.url)
const __dirname = path.dirname(__filename)

const PORT = 5000
const USE_HTTPS = false
const URL = `${USE_HTTPS ? 'https' : 'http'}://localhost:${PORT}`

// ✅ Путь к backend внутри запакованного .exe
const backendPath = path.join(process.resourcesPath, 'publish')

function waitForServerReady(retries = 30, interval = 1000) {
  return new Promise((resolve, reject) => {
    let attempts = 0

    const check = () => {
      const req = http.request(
        {
          method: 'HEAD',
          host: 'localhost',
          port: PORT,
          path: '/',
        },
        () => resolve(true),
      )

      req.on('error', () => {
        if (++attempts >= retries) return reject(new Error('Backend not responding'))
        setTimeout(check, interval)
      })

      req.end()
    }

    check()
  })
}

async function createWindow() {
  const win = new BrowserWindow({
    width: 1200,
    height: 800,
    webPreferences: {
      nodeIntegration: false,
    },
  })

  // ✅ Запускаем backend
  const backend = exec('ExcelWebApp2.exe', {
    cwd: backendPath,
  })

  backend.stdout.on('data', (data) => console.log(`[backend]: ${data}`))
  backend.stderr.on('data', (data) => console.error(`[backend error]: ${data}`))

  try {
    console.log('⏳ Waiting for backend to start...')
    await waitForServerReady()
    console.log('✅ Backend is ready, opening window...')
    win.loadURL(URL)
  } catch (err) {
    console.error('❌ Failed to start backend in time:', err)
    win.loadURL('data:text/html,<h1>Backend did not start</h1>')
  }
}

app.whenReady().then(() => {
  createWindow()
})
