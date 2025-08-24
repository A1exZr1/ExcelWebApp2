using ClosedXML.Excel;
using ExcelWebApp2.Models;

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

                var range = worksheet.Range(row, 1, row, 10);

                if (item.WorkCost == null)
                {
                    range.Style.Fill.BackgroundColor = XLColor.LightPink;
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
    }
}
