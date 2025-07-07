using ClosedXML.Excel;
using ExcelWebApp2.Models;
using ExcelWebApp2.Repositories.Extensions;
using System.ComponentModel;
using System.Reflection;

namespace ExcelWebApp2.Repositories
{
    public class FileReaderRepository : FileReaderBase
    {

        public async Task<ReadResult<T>> ReadExcelFile<T>(IFormFile file)
        {
            var result = new ReadResult<T>();

            try
            {
                using var stream = file.OpenReadStream();
                object rawResult;

                if (typeof(T) == typeof(AccrualRecordModel))
                {
                    rawResult = await Task.Run(() => ReadAccruals(stream));
                }
                else if (typeof(T) == typeof(AdvertisingModel))
                {
                    rawResult = await Task.Run(() => ReadAdvertisment(stream));
                }
                else if (typeof(T) == typeof(PrimeCostModel))
                {
                    rawResult = await Task.Run(() => ReadPrimeCost(stream));
                }
                else
                {
                    throw new NotSupportedException($"Unsupported type {typeof(T).Name}");
                }



                if (rawResult is List<T> typedList)
                {
                    result.Data = typedList;
                    result.Success = true;
                    result.Message = $"Read {typedList.Count} rows.";
                }
                else
                {
                    result.Success = false;
                    result.Message = $"Failed to cast result to List<{typeof(T).Name}>";
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error due read file for {typeof(T).Name}: {ex.Message}";
            }
            return result;
        }


        private static List<AccrualRecordModel> ReadAccruals(Stream stream)
        {

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.First();

            var rows = (worksheet.RangeUsed()?.RowsUsed().Skip(1).ToList()) ?? throw new FileReaderException("File is empty.");
            var headerRow = rows.First();

            var headerCheckingRow = "ID начисления;Дата начисления;Группа услуг;Тип начисления;Артикул;SKU;Название товара;Количество;Цена продавца;Дата принятия заказа в обработку или оказания услуги;Схема работы;Вознаграждение Ozon, %;Индекс локализации, %;Среднее время доставки, часы;Сумма итого, руб";

            if (!IsHeadersCorrect([.. headerRow.Cells().Select(c => c.GetValue<string>())], headerCheckingRow))
                throw new FileReaderException("Header of file doesn't fit to configurations header");

            var headerIndexes = GetHeaderIndexes<AccrualRecordModel>(headerRow);

            var result = rows
                .Skip(1)
                .Select(row => new AccrualRecordModel
                {
                    ArticleName = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordModel.ArticleName)]),
                    Sku = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordModel.Sku)]),
                    AccrualType = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordModel.AccrualType)]),
                    SellerCost = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordModel.SellerCost)]),
                    Quantity = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordModel.Quantity)]),
                    SummaryValue = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordModel.SummaryValue)]),
                })
                .ToList();
            return result;
        }

        private static List<AdvertisingModel> ReadAdvertisment(Stream stream)
        {

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.First();

            var rows = (worksheet.RangeUsed()?.RowsUsed().Skip(1).ToList()) ?? throw new FileReaderException("File is empty.");
            var headerRow = rows.First();

            var headerCheckingRow = "SKU;Тип продвижения;ID кампании;Расход, ₽, с НДС;ДРР, %;Продажи, ₽;Заказы, шт;CTR, %;Показы;Клики;Стоимость заказа, ₽;Стоимость клика, ₽;Корзины;Конверсия в корзину, %";

            if (!IsHeadersCorrect([.. headerRow.Cells().Select(c => c.GetValue<string>())], headerCheckingRow))
                throw new FileReaderException("Header of file doesn't fit to configurations header");

            var headerIndexes = GetHeaderIndexes<AdvertisingModel>(headerRow);

            var result = rows
                .Skip(1)
                .Select(row => new AdvertisingModel
                {
                    Sku = row.GetFieldByIndex(headerIndexes[nameof(AdvertisingModel.Sku)]),
                    PromotionType = row.GetFieldByIndex(headerIndexes[nameof(AdvertisingModel.PromotionType)]),
                    Cost = row.GetFieldByIndex(headerIndexes[nameof(AdvertisingModel.Cost)])
                })
                .ToList();
            return result;
        }

        private static List<PrimeCostModel> ReadPrimeCost(Stream stream)
        {

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.First();

            var rows = (worksheet.RangeUsed()?.RowsUsed().ToList()) ?? throw new FileReaderException("File is empty.");
            var headerRow = rows.First();

            var headerCheckingRow = "Артикул;Себестоимость материалов;Цена работы;Итого";

            if (!IsHeadersCorrect([.. headerRow.Cells().Select(c => c.GetValue<string>())], headerCheckingRow))
                throw new FileReaderException("Header of file doesn't fit to configurations header");

            var headerIndexes = GetHeaderIndexes<PrimeCostModel>(headerRow);

            var result = rows
                .Skip(1)
                .Select(row => new PrimeCostModel
                {
                    ArticleName = row.GetFieldByIndex(headerIndexes[nameof(PrimeCostModel.ArticleName)]),
                    MaterialCost = row.GetFieldByIndex(headerIndexes[nameof(PrimeCostModel.MaterialCost)]),
                    WorkCost = row.GetFieldByIndex(headerIndexes[nameof(PrimeCostModel.WorkCost)])
                })
                .ToList();
            return result;
        }

        private static Dictionary<string, int?> GetHeaderIndexes<T>(IXLRangeRow row)
        {
            var headerIndexes = new Dictionary<string, int?>();

            foreach (var prop in typeof(T).GetProperties())
            {
                var descriptionAttr = prop.GetCustomAttribute<DescriptionAttribute>();
                if (descriptionAttr == null) 
                    continue;

                string description = descriptionAttr.Description;
                int? index = row.GetFieldIndex(description);

                headerIndexes[prop.Name] = index;
            }

            return headerIndexes;
        }

        public static string GetPropertyDescription<T>(string propertyName)
        {
            var prop = typeof(T).GetProperty(propertyName);
            if (prop == null) 
                return string.Empty;

            var descriptionAttr = prop.GetCustomAttribute<DescriptionAttribute>();
            return descriptionAttr?.Description ?? string.Empty;
        }
    }
}
