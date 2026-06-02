export enum ReportType {
  OzonV1 = 'ozon1',
  OzonV2 = 'ozon2',
  Wildberries = 'wb',
}

export enum FileUploadType {
  OzonV1Accrual = 'ozon-v1-accrual',
  OzonV1Advertisement = 'ozon-v1-advertisement',
  OzonV1PrimeCost = 'ozon-v1-prime-cost',
  OzonV2Accrual = 'ozon-v2-accrual',
  OzonV2PrimeCost = 'ozon-v2-prime-cost',
  WildberriesAccruals = 'wildberries-accruals',
  WildberriesPrimeCost = 'wildberries-prime-cost',
  WildberriesCancellations = 'wildberries-cancellations',
}

export interface ReportComponentExpose {
  loadData: () => Promise<void>
  exportData: () => Promise<void>
  reset: () => void
}
