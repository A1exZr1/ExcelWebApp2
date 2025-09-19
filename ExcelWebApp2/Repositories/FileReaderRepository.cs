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
                else if (typeof(T) == typeof(AccrualRecordWbModel))
                {
                    rawResult = await Task.Run(() => ReadAccrualsWb(stream));
                }
                else if (typeof(T) == typeof(PrimeCostWbModel))
                {
                    rawResult = await Task.Run(() => ReadPrimeCostWb(stream));
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

        private static List<AccrualRecordWbModel> ReadAccrualsWb(Stream stream)
        {

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.First();

            var rows = (worksheet.RangeUsed()?.RowsUsed().ToList()) ?? throw new FileReaderException("Файл пуст");
            var headerRow = rows.First();

            var headerCheckingRow = "№;Номер поставки;Предмет;Код номенклатуры;Бренд;Артикул поставщика;Название;Размер;Баркод;Тип документа;Обоснование для оплаты;Дата заказа покупателем;Дата продажи;Кол-во;Цена розничная;Вайлдберриз реализовал Товар (Пр);Согласованный продуктовый дисконт, %;Промокод, %;Итоговая согласованная скидка, %;Цена розничная с учетом согласованной скидки;Размер снижения кВВ из-за рейтинга, %;Размер изменения кВВ из-за акции, %;Скидка постоянного Покупателя (СПП), %;Размер кВВ, %;Размер  кВВ без НДС, % Базовый;Итоговый кВВ без НДС, %;Вознаграждение с продаж до вычета услуг поверенного, без НДС;Возмещение за выдачу и возврат товаров на ПВЗ;Эквайринг/Комиссии за организацию платежей;Размер комиссии за эквайринг/Комиссии за организацию платежей, %;Тип платежа за Эквайринг/Комиссии за организацию платежей;Вознаграждение Вайлдберриз (ВВ), без НДС;НДС с Вознаграждения Вайлдберриз;К перечислению Продавцу за реализованный Товар;Количество доставок;Количество возврата;Услуги по доставке товара покупателю;Дата начала действия фиксации;Дата конца действия фиксации;Признак услуги платной доставки;Общая сумма штрафов;Корректировка Вознаграждения Вайлдберриз (ВВ);Виды логистики, штрафов и корректировок ВВ;Стикер МП;Наименование банка-эквайера;Номер офиса;Наименование офиса доставки;ИНН партнера;Партнер;Склад;Страна;Тип коробов;Номер таможенной декларации;Номер сборочного задания;Код маркировки;ШК;Srid;Возмещение издержек по перевозке/по складским операциям с товаром;Организатор перевозки;Хранение;Удержания;Платная приемка;Фиксированный коэффициент склада по поставке;Признак продажи юридическому лицу;Номер короба для платной приемки;Скидка по программе софинансирования;Скидка Wibes, %;Сумма удержанная за начисленные баллы программы лояльности;Компенсация скидки по программе лояльности";

            if (!IsHeadersCorrect([.. headerRow.Cells().Select(c => c.GetValue<string>())], headerCheckingRow))
                throw new FileReaderException("Ошибка: строка с заголовками в файле отличается от ожидаемой. Проверьте, что вы загрузили верный шаблон.");

            var headerIndexes = GetHeaderIndexes<AccrualRecordWbModel>(headerRow);

            var result = rows
                .Skip(1)
                .Select(row => new AccrualRecordWbModel
                {
                    ArticleName = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordWbModel.ArticleName)]),
                    Sku = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordWbModel.Sku)]),
                    AmountPayableToSeller = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordWbModel.AmountPayableToSeller)]),
                    DocumentType = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordWbModel.DocumentType)]),
                    Logistics = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordWbModel.Logistics)]),
                    PaymentReason = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordWbModel.PaymentReason)]),
                    Quantity = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordWbModel.Quantity)]),
                    RetailPrice = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordWbModel.RetailPrice)]),
                    SupplierArticleName = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordWbModel.SupplierArticleName)]),
                    TypesOfLogisticsPenaltiesAndAdjustments = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordWbModel.TypesOfLogisticsPenaltiesAndAdjustments)]),
                    PaidAcceptance = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordWbModel.PaidAcceptance)]),
                    TotalAmountOfFines = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordWbModel.TotalAmountOfFines)]),
                    StorageFee = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordWbModel.StorageFee)]),
                    Withholdings = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordWbModel.Withholdings)]),
                })
                .ToList();
            return result;
        }

        private static List<PrimeCostWbModel> ReadPrimeCostWb(Stream stream)
        {
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.First();

            var rows = (worksheet.RangeUsed()?.RowsUsed().ToList()) ?? throw new FileReaderException("Файл пуст.");
            var headerRow = rows.First();

            var headerIndexes = GetHeaderIndexes<PrimeCostWbModel>(headerRow);

            var result = rows
                .Skip(1)
                .Select(row => new PrimeCostWbModel
                {
                    Sku = row.GetFieldByIndex(headerIndexes[nameof(PrimeCostWbModel.Sku)]),
                    ArticleName = row.GetFieldByIndex(headerIndexes[nameof(PrimeCostWbModel.ArticleName)]),
                    MaterialCost = row.GetFieldByIndex(headerIndexes[nameof(PrimeCostWbModel.MaterialCost)]),
                    WorkCost = row.GetFieldByIndex(headerIndexes[nameof(PrimeCostWbModel.WorkCost)]),
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
