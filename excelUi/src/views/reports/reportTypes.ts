export type ReportType = 'ozon1' | 'ozon2' | 'wb'

export interface ReportComponentExpose {
  loadData: () => Promise<void>
  exportData: () => Promise<void>
  reset: () => void
}
