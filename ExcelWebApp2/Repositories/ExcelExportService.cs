using ClosedXML.Excel;
using ExcelWebApp2.Models;
using System;

namespace ExcelWebApp2.Repositories
{
    public class ExcelExportService
    {
        public MemoryStream Export(List<ProcessedOzonResultV1Model> data)
        {
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Results");

            worksheet.Cell(1, 1).Value = "Имя артикула";
            worksheet.Cell(1, 2).Value = "SKU";
            worksheet.Cell(1, 3).Value = "Поступило на счёт";
            worksheet.Cell(1, 4).Value = "Количество продаж";
            worksheet.Cell(1, 5).Value = "Выручка";
            worksheet.Cell(1, 6).Value = "Расходы на рекламу";
            worksheet.Cell(1, 7).Value = "Нераспределённые расходы";
            worksheet.Cell(1, 8).Value = "Расходы на работу";
            worksheet.Cell(1, 9).Value = "Расходы на материалы";
            worksheet.Cell(1, 10).Value = "Чистая прибыль";
            worksheet.Cell(1, 11).Value = "% от выручки";

            for (int i = 0; i < data.Count; i++)
            {
                var row = i + 2;
                var item = data[i];

                

                if (item.WorkCost == null)
                {
                    var range = worksheet.Range(row, 1, row, 11);
                    range.Style.Fill.BackgroundColor = XLColor.LightPink;
                }

                var isTotal = item.ArticleName != null && item.ArticleName.StartsWith("Итого", StringComparison.CurrentCultureIgnoreCase);
                if (isTotal)
                {
                    var range = worksheet.Range(row, 1, row, 11);
                    range.Style.Font.Bold = true;
                    range.Style.Fill.BackgroundColor = XLColor.FromHtml("#f8f9fb");
                }

                worksheet.Cell(row, 1).Value = item.ArticleName;
                worksheet.Cell(row, 2).Value = item.Sku;
                worksheet.Cell(row, 3).Value = item.TotalSumm;
                worksheet.Cell(row, 4).Value = item.Quantity;
                worksheet.Cell(row, 5).Value = item.Revenue;
                worksheet.Cell(row, 6).Value = item.AdvertisingCost;
                worksheet.Cell(row, 7).Value = item.UnlinkedExpenses;
                worksheet.Cell(row, 8).Value = item.WorkCost;
                worksheet.Cell(row, 9).Value = item.MaterialCost;
                worksheet.Cell(row, 10).Value = item.NetProfit;
                worksheet.Cell(row, 11).Value = item.ProfitPercent;
            }

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            return stream;
        }

        public MemoryStream Export(List<ProcessedOzonResultV2Model> data)
        {
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Results");

            var feeKeys = data
                .SelectMany(r => r.AdditionalFees?.Keys ?? Enumerable.Empty<string>())
                .Distinct()
                .OrderBy(k => k, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            int col = 1;
            ws.Cell(1, col++).Value = "Имя артикула";
            ws.Cell(1, col++).Value = "SKU";
            ws.Cell(1, col++).Value = "Склад";
            ws.Cell(1, col++).Value = "Цена продажи";
            ws.Cell(1, col++).Value = "Количество продаж";
            ws.Cell(1, col++).Value = "Стоимость работы";
            ws.Cell(1, col++).Value = "Стоимость материалов";
            ws.Cell(1, col++).Value = "Нераспределённые расходы";
            ws.Cell(1, col++).Value = "Комиссия Ozon";
            ws.Cell(1, col++).Value = "Комиссия за обработку";
            ws.Cell(1, col++).Value = "Последняя миля";
            ws.Cell(1, col++).Value = "Логистика";

            var feeColStart = col;
            foreach (var k in feeKeys)
                ws.Cell(1, col++).Value = k;

            ws.Cell(1, col++).Value = "Чистая прибыль";
            ws.Cell(1, col++).Value = "% от выручки";

            var headerRange = ws.Range(1, 1, 1, col - 1);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#f0f3f8");


            int row = 2;
            foreach (var item in data)
            {
                col = 1;

                bool isTotal = (item.ArticleName ?? "").StartsWith("Итого", StringComparison.CurrentCultureIgnoreCase);

                if (isTotal)
                {
                    var rng = ws.Range(row, 1, row, col - 1 + (feeKeys.Count + 14));
                    rng.Style.Font.Bold = true;
                    rng.Style.Fill.BackgroundColor = XLColor.FromHtml("#f8f9fb");
                }

                if (item.WorkCost == null)
                {
                    var rng = ws.Range(row, 1, row, col - 1 + (feeKeys.Count + 14));
                    rng.Style.Fill.BackgroundColor = XLColor.LightPink;
                }

                ws.Cell(row, col++).Value = item.ArticleName;
                ws.Cell(row, col++).Value = item.Sku;
                ws.Cell(row, col++).Value = item.Warehouse;

                ws.Cell(row, col).Value = item.PreCommissionAmount; ws.Cell(row, col++).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, col).Value = item.Quantity; ws.Cell(row, col++).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, col).Value = item.WorkCost; ws.Cell(row, col++).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, col).Value = item.MaterialCost; ws.Cell(row, col++).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, col).Value = item.UnlinkedExpenses; ws.Cell(row, col++).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, col).Value = item.OzonFee; ws.Cell(row, col++).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, col).Value = item.HandlingFee; ws.Cell(row, col++).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, col).Value = item.LastMileFee; ws.Cell(row, col++).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, col).Value = item.LogisticsFee; ws.Cell(row, col++).Style.NumberFormat.Format = "#,##0.00";


                foreach (var k in feeKeys)
                {
                    var v = (item.AdditionalFees != null && item.AdditionalFees.TryGetValue(k, out var feeVal))
                        ? feeVal : 0m;
                    ws.Cell(row, col).Value = v;
                    ws.Cell(row, col++).Style.NumberFormat.Format = "#,##0.00";
                }

                ws.Cell(row, col).Value = item.NetProfit; ws.Cell(row, col++).Style.NumberFormat.Format = "#,##0.00";

                if (item.ProfitPercent.HasValue)
                {
                    ws.Cell(row, col).Value = item.ProfitPercent.Value / 100m;
                    ws.Cell(row, col).Style.NumberFormat.Format = "0.00%";
                }
                else
                {
                    ws.Cell(row, col).Value = string.Empty;
                }
                col++;

                row++;
            }

            ws.Columns().AdjustToContents();

            var stream = new MemoryStream();
            wb.SaveAs(stream);
            stream.Position = 0;
            return stream;
        }

        public MemoryStream Export(List<ProcessedWbResultModel> data)
        {
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Results");
            
            int col = 1;
            ws.Cell(1, col++).Value = "Артикул поставщика";
            ws.Cell(1, col++).Value = "Бренд";
            ws.Cell(1, col++).Value = "Предмет";
            ws.Cell(1, col++).Value = "Код номенклатуры";
            ws.Cell(1, col++).Value = "Цена розничная";
            ws.Cell(1, col++).Value = "К перечислению продавцу за реализованный товар";
            ws.Cell(1, col++).Value = "Количество продаж";
            ws.Cell(1, col++).Value = "Логистика";
            ws.Cell(1, col++).Value = "Количество отмен";
            ws.Cell(1, col++).Value = "Отмены";
            ws.Cell(1, col++).Value = "Платная приёмка";
            ws.Cell(1, col++).Value = "Штрафы";
            ws.Cell(1, col++).Value = "Количество возвратов";
            ws.Cell(1, col++).Value = "Возвраты";
            ws.Cell(1, col++).Value = "Расходы работы при возвратах";
            ws.Cell(1, col++).Value = "15% стоимости материалов при возвратах";
            ws.Cell(1, col++).Value = "Реклама";
            ws.Cell(1, col++).Value = "Отзывы за баллы";
            ws.Cell(1, col++).Value = "Количество работ при отменах";
            ws.Cell(1, col++).Value = "Расходы работы при отменах";
            ws.Cell(1, col++).Value = "15% стоимости материалов при отменах";
            ws.Cell(1, col++).Value = "Стоимость работы";
            ws.Cell(1, col++).Value = "Стоимость материалов";
            ws.Cell(1, col++).Value = "Чистая прибыль";
            ws.Cell(1, col++).Value = "% от выручки";


            var headerRange = ws.Range(1, 1, 1, col - 1);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#f0f3f8");

            var allColumnsCount = col - 1;
            int row = 2;
            foreach (var item in data)
            {
                col = 1;

                bool isTotal = (item.ArticleName ?? "").StartsWith("Итого", StringComparison.CurrentCultureIgnoreCase);

                if (isTotal)
                {
                    var rng = ws.Range(row, 1, row, allColumnsCount);
                    rng.Style.Font.Bold = true;
                    rng.Style.Fill.BackgroundColor = XLColor.FromHtml("#f8f9fb");
                }

                if (item.WorkCost == null)
                {
                    var rng = ws.Range(row, 1, row, allColumnsCount);
                    rng.Style.Fill.BackgroundColor = XLColor.LightPink;
                }

                ws.Cell(row, col++).Value = item.SupplierArticleName;
                ws.Cell(row, col++).Value = item.Brand;
                ws.Cell(row, col++).Value = item.ArticleName;
                ws.Cell(row, col++).Value = item.Sku;
                ws.Cell(row, col).Value = item.RetailPriceSumm; ws.Cell(row, col++).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, col).Value = item.AmountPayableToSellerSumm; ws.Cell(row, col++).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, col).Value = item.Quantity; ws.Cell(row, col++).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, col).Value = item.LogisticsFee; ws.Cell(row, col++).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, col).Value = item.CancelledQuantity; ws.Cell(row, col++).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, col).Value = item.CancelledSumm; ws.Cell(row, col++).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, col).Value = item.PaidAcceptanceSumm; ws.Cell(row, col++).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, col).Value = item.TotalAmountOfFines; ws.Cell(row, col++).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, col).Value = item.ReturnedQuantity; ws.Cell(row, col++).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, col).Value = item.ReturnedSumm; ws.Cell(row, col++).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, col).Value = item.ReturnWorkCost; ws.Cell(row, col++).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, col).Value = item.ReturnMaterialDamageCost; ws.Cell(row, col++).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, col).Value = item.AdvertisingCost; ws.Cell(row, col++).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, col).Value = item.ReviewPointsCost; ws.Cell(row, col++).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, col).Value = item.CancellationWorkQuantity; ws.Cell(row, col++).Style.NumberFormat.Format = "#,##0";
                ws.Cell(row, col).Value = item.CancellationWorkCost; ws.Cell(row, col++).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, col).Value = item.CancellationMaterialDamageCost; ws.Cell(row, col++).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, col).Value = item.WorkCost; ws.Cell(row, col++).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, col).Value = item.MaterialCost; ws.Cell(row, col++).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, col).Value = item.NetProfit; ws.Cell(row, col++).Style.NumberFormat.Format = "#,##0.00";

                if (item.ProfitPercent.HasValue)
                {
                    ws.Cell(row, col).Value = item.ProfitPercent.Value / 100m;
                    ws.Cell(row, col).Style.NumberFormat.Format = "0.00%";
                }
                else
                {
                    ws.Cell(row, col).Value = string.Empty;
                }
                col++;

                row++;
            }

            ws.Columns().AdjustToContents();
            var stream = new MemoryStream();
            wb.SaveAs(stream);
            stream.Position = 0;
            return stream;
        }

    }
}
