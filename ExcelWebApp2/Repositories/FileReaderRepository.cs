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

                if (typeof(T) == typeof(AccrualRecordV1Model))
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
                else if (typeof(T) == typeof(AccrualRecordV2Model))
                {
                    rawResult = await Task.Run(() => ReadAccrualsV2(stream));
                }
                else
                {
                    throw new NotSupportedException($"Не поддерживаемый тип {typeof(T).Name}");
                }



                if (rawResult is List<T> typedList)
                {
                    result.Data = typedList;
                    result.Success = true;
                    result.Message = $"Прочитано {typedList.Count} строк.";
                }
                else
                {
                    result.Success = false;
                    result.Message = $"Ошибка приведения результат к листу List<{typeof(T).Name}>";
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Ошибка во время чтения файла {typeof(T).Name}: {ex.Message}";
            }
            return result;
        }

        private static List<AccrualRecordV2Model> ReadAccrualsV2(Stream stream)
        {
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.First();

            var rows = (worksheet.RangeUsed()?.RowsUsed().ToList()) ?? throw new FileReaderException("Файл пуст");
            var headerRow = rows.First();

            var headerCheckingRow = "Дата начисления;Тип начисления;Номер отправления или идентификатор услуги;Дата принятия заказа в обработку или оказания услуги;Склад отгрузки;SKU;Артикул;Название товара или услуги;Количество;За продажу или возврат до вычета комиссий и услуг;Вознаграждение Ozon, %;Вознаграждение Ozon;Сборка заказа;Обработка отправления (Drop-off/Pick-up) (разбивается по товарам пропорционально количеству в отправлении);Магистраль;Последняя миля (разбивается по товарам пропорционально доле цены товара в сумме отправления);Обратная магистраль;Обработка возврата;Обработка отмененного или невостребованного товара (разбивается по товарам в отправлении в одинаковой пропорции);Обработка невыкупленного товара;Логистика;Индекс локализации;Среднее время доставки, часы;Обратная логистика;Итого, руб.";

            if (!IsHeadersCorrect([.. headerRow.Cells().Select(c => c.GetValue<string>())], headerCheckingRow))
                throw new FileReaderException("Ошибка: строка с заголовками в файле отличается от ожидаемой. Проверьте, что вы загрузили верный шаблон.");

            var headerIndexes = GetHeaderIndexes<AccrualRecordV2Model>(headerRow);

            var result = rows
                .Skip(1)
                .Select(row => new AccrualRecordV2Model
                {
                    ArticleName = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordV2Model.ArticleName)]),
                    Sku = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordV2Model.Sku)]),
                    AccrualType = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordV2Model.AccrualType)]),
                    Warehouse = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordV2Model.Warehouse)]),
                    Quantity = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordV2Model.Quantity)]),
                    PreCommissionAmount = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordV2Model.PreCommissionAmount)]),
                    OzonFee = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordV2Model.OzonFee)]),
                    HandlingFee = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordV2Model.HandlingFee)]),
                    LastMileFee = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordV2Model.LastMileFee)]),
                    LogisticsFee = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordV2Model.LogisticsFee)]),
                    TotalAmount = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordV2Model.TotalAmount)])
                })
                .ToList();

            return result;
        }   


        private static List<AccrualRecordV1Model> ReadAccruals(Stream stream)
        {

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.First();

            var rows = (worksheet.RangeUsed()?.RowsUsed().Skip(1).ToList()) ?? throw new FileReaderException("Файл пуст");
            var headerRow = rows.First();

            var headerCheckingRow = "ID начисления;Дата начисления;Группа услуг;Тип начисления;Артикул;SKU;Название товара;Количество;Цена продавца;Дата принятия заказа в обработку или оказания услуги;Схема работы;Вознаграждение Ozon, %;Индекс локализации, %;Среднее время доставки, часы;Сумма итого, руб";

            if (!IsHeadersCorrect([.. headerRow.Cells().Select(c => c.GetValue<string>())], headerCheckingRow))
                throw new FileReaderException("Ошибка: строка с заголовками в файле отличается от ожидаемой. Проверьте, что вы загрузили верный шаблон.");

            var headerIndexes = GetHeaderIndexes<AccrualRecordV1Model>(headerRow);

            var result = rows
                .Skip(1)
                .Select(row => new AccrualRecordV1Model
                {
                    ArticleName = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordV1Model.ArticleName)]),
                    Sku = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordV1Model.Sku)]),
                    AccrualType = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordV1Model.AccrualType)]),
                    SellerCost = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordV1Model.SellerCost)]),
                    Quantity = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordV1Model.Quantity)]),
                    SummaryValue = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordV1Model.SummaryValue)]),
                })
                .ToList();
            return result;
        }

        private static List<AdvertisingModel> ReadAdvertisment(Stream stream)
        {

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.First();

            var rows = (worksheet.RangeUsed()?.RowsUsed().Skip(1).ToList()) ?? throw new FileReaderException("Файл пуст");
            var headerRow = rows.First();

            var headerCheckingRow = "SKU;Тип продвижения;ID кампании;Расход, ₽, с НДС;ДРР, %;Продажи, ₽;Заказы, шт;CTR, %;Показы;Клики;Стоимость заказа, ₽;Стоимость клика, ₽;Корзины;Конверсия в корзину, %";

            if (!IsHeadersCorrect([.. headerRow.Cells().Select(c => c.GetValue<string>())], headerCheckingRow))
                throw new FileReaderException("Ошибка: строка с заголовками в файле отличается от ожидаемой. Проверьте, что вы загрузили верный шаблон.");

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

            var rows = (worksheet.RangeUsed()?.RowsUsed().ToList()) ?? throw new FileReaderException("Файл пуст.");
            var headerRow = rows.First();

            var headerCheckingRow = "Артикул;Себестоимость материалов;Цена работы;Итого";

            if (!IsHeadersCorrect([.. headerRow.Cells().Select(c => c.GetValue<string>())], headerCheckingRow))
                throw new FileReaderException("Ошибка: строка с заголовками в файле отличается от ожидаемой. Проверьте, что вы загрузили верный шаблон.");

            var headerIndexes = GetHeaderIndexes<PrimeCostModel>(headerRow);

            var result = rows
                .Skip(1)
                .Select(row => new PrimeCostModel
                {
                    ArticleName = row.GetFieldByIndex(headerIndexes[nameof(PrimeCostModel.ArticleName)]),
                    MaterialCost = row.GetFieldByIndex(headerIndexes[nameof(PrimeCostModel.MaterialCost)]),
                    WorkCost = row.GetFieldByIndex(headerIndexes[nameof(PrimeCostModel.WorkCost)]),
                    Total = row.GetFieldByIndex(headerIndexes[nameof(PrimeCostModel.Total)])
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

                var index = GetExactHeaderIndex(row, descriptionAttr.Description);
                headerIndexes[prop.Name] = index;
            }

            return headerIndexes;
        }

        private static int? GetExactHeaderIndex(IXLRangeRow headerRow, string header)
        {
            var wanted = Norm(header);

            foreach (var cell in headerRow.CellsUsed())
            {
                var text = Norm(cell.GetValue<string>());
                if (string.Equals(text, wanted, StringComparison.Ordinal)) 
                    return cell.Address.ColumnNumber;
            }

            // fallback
            foreach (var cell in headerRow.CellsUsed())
            {
                var text = Norm(cell.GetValue<string>());
                if (text.StartsWith(wanted, StringComparison.Ordinal))
                    return cell.Address.ColumnNumber;
            }

            return null;
        }
        private static string Norm(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            return s
                .Replace('\u00A0', ' ')   // NBSP -> обычный пробел
                .Trim();
        }
    }
}
