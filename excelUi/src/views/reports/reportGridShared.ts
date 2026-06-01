import axios from 'axios'
import type { GridApi, GridOptions, RowClassParams } from 'ag-grid-community'

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

export async function exportRows(endpoint: string, rows: any[], fileSuffix: string) {
  const plainRows = JSON.parse(JSON.stringify(rows))
  const response = await axios.post(endpoint, plainRows, {
    responseType: 'blob',
    headers: { 'Content-Type': 'application/json' },
  })

  const blob = new Blob([response.data], {
    type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
  })

  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = `processed_results_${fileSuffix}.xlsx`
  link.click()
  URL.revokeObjectURL(url)
}

export function sumRows(rows: any[], selector: (row: any) => number | undefined | null) {
  return Number(rows.reduce((acc, row) => acc + (Number(selector(row)) || 0), 0).toFixed(2))
}

export function percent(numerator: number, denominator: number) {
  return denominator !== 0 ? Number(((numerator / Math.abs(denominator)) * 100).toFixed(2)) : null
}
