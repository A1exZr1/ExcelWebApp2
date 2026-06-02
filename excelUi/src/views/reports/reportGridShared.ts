import type { GridApi, GridOptions, RowClassParams } from 'ag-grid-community'
import {
  downloadFile,
  reportExportClient,
} from '../../api/fileReaderApi'
import type {
  ProcessedOzonResultV1Model,
  ProcessedOzonResultV2Model,
  ProcessedWbResultModel,
} from '../../api/client'
import { ReportType } from './reportTypes'

export const reportGridOptions: GridOptions = {
  rowClassRules: {
    'red-row': (params: RowClassParams) => params.data?.materialCost == null,
  },
  defaultColDef: {
    sortable: true,
    filter: 'agTextColumnFilter',
    resizable: true,
    wrapHeaderText: true,
    autoHeaderHeight: true,
  },
  getRowStyle: (p) => (p.node.rowPinned ? { fontWeight: '600', background: '#f8f9fb' } : undefined),
  popupParent: document.body,
  rowModelType: 'clientSide',
  overlayNoRowsTemplate: '<span>Нет данных</span>',
  rowSelection: 'single',
  enableCellTextSelection: true,
  localeText: {
    equals: 'Равно',
    notEqual: 'Не равно',
    lessThan: 'Меньше',
    greaterThan: 'Больше',
    lessThanOrEqual: 'Меньше или равно',
    greaterThanOrEqual: 'Больше или равно',
    inRange: 'В диапазоне',
    contains: 'Содержит',
    notContains: 'Не содержит',
    startsWith: 'Начинается с',
    endsWith: 'Заканчивается на',
    filterOoo: 'Фильтр...',
    blanks: '(пусто)',
    apply: 'Применить',
    reset: 'Сбросить',
    clear: 'Очистить',
    cancel: 'Отмена',
    columns: 'Колонки',
    filters: 'Фильтры',
    noRowsToShow: 'Нет данных',
    blank: 'Пусто',
    notBlank: 'Не пусто',
  },
}

export function collectDisplayedRows(gridApi: GridApi) {
  const rows: any[] = []
  gridApi.forEachNodeAfterFilterAndSort((node) => {
    if (node.data) rows.push(node.data)
  })

  const pinned = gridApi.getPinnedBottomRow(0)?.data
  if (pinned) rows.push(pinned)

  return rows
}

export async function exportRows(reportType: ReportType, rows: any[]) {
  const plainRows = JSON.parse(JSON.stringify(rows))
  const fallbackFileName = `processed_results_${reportType}.xlsx`

  if (reportType === ReportType.OzonV1) {
    const response = await reportExportClient.exportProcessedResultsV1(
      plainRows as ProcessedOzonResultV1Model[],
    )
    downloadFile(response, fallbackFileName)
    return
  }

  if (reportType === ReportType.OzonV2) {
    const response = await reportExportClient.exportProcessedResultsV2(
      plainRows as ProcessedOzonResultV2Model[],
    )
    downloadFile(response, fallbackFileName)
    return
  }

  if (reportType === ReportType.Wildberries) {
    const response = await reportExportClient.exportProcessedResultsWb(
      plainRows as ProcessedWbResultModel[],
    )
    downloadFile(response, fallbackFileName)
    return
  }

  throw new Error(`Unknown report type: ${reportType}`)
}

export function sumRows(rows: any[], selector: (row: any) => number | undefined | null) {
  return Number(rows.reduce((acc, row) => acc + (Number(selector(row)) || 0), 0).toFixed(2))
}

export function percent(numerator: number, denominator: number) {
  return denominator !== 0 ? Number(((numerator / Math.abs(denominator)) * 100).toFixed(2)) : null
}
