import {
  AuthClient,
  ReportExportClient,
  ReportFilesClient,
  ReportResultsClient,
  SwaggerException,
  type FileParameter,
  type FileResponse,
} from './client'

export const authClient = new AuthClient()
export const reportExportClient = new ReportExportClient()
export const reportFilesClient = new ReportFilesClient()
export const reportResultsClient = new ReportResultsClient()

export function toFileParameter(file: File): FileParameter {
  return {
    data: file,
    fileName: file.name,
  }
}

export function downloadFile(response: FileResponse, fallbackFileName: string) {
  const url = URL.createObjectURL(response.data)
  const link = document.createElement('a')
  link.href = url
  link.download = response.fileName || fallbackFileName
  link.click()
  URL.revokeObjectURL(url)
}

export function getApiErrorMessage(error: unknown) {
  if (SwaggerException.isSwaggerException(error)) {
    return error.response || error.message
  }

  if (error instanceof Error) {
    return error.message
  }

  return String(error)
}
