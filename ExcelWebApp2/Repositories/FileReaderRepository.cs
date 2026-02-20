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
            var headerIndexes = GetHeaderIndexes<AccrualRecordV2Model>(headerRow);

            var result = rows
                .Skip(1)
                .Select(row => new AccrualRecordV2Model
                {
                    ArticleName = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordV2Model.ArticleName)], nameof(AccrualRecordV2Model.ArticleName)),
                    Sku = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordV2Model.Sku)], nameof(AccrualRecordV2Model.Sku)),
                    AccrualType = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordV2Model.AccrualType)], nameof(AccrualRecordV2Model.AccrualType)),
                    Warehouse = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordV2Model.Warehouse)], nameof(AccrualRecordV2Model.Warehouse)),
                    Quantity = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordV2Model.Quantity)], nameof(AccrualRecordV2Model.Quantity)),
                    PreCommissionAmount = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordV2Model.PreCommissionAmount)], nameof(AccrualRecordV2Model.PreCommissionAmount)),
                    OzonFee = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordV2Model.OzonFee)], nameof(AccrualRecordV2Model.OzonFee)),
                    HandlingFee = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordV2Model.HandlingFee)], nameof(AccrualRecordV2Model.HandlingFee)),
                    LastMileFee = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordV2Model.LastMileFee)], nameof(AccrualRecordV2Model.LastMileFee)),
                    LogisticsFee = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordV2Model.LogisticsFee)], nameof(AccrualRecordV2Model.LogisticsFee)),
                    TotalAmount = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordV2Model.TotalAmount)], nameof(AccrualRecordV2Model.TotalAmount))
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
            var headerIndexes = GetHeaderIndexes<AccrualRecordV1Model>(headerRow);

            var result = rows
                .Skip(1)
                .Select(row => new AccrualRecordV1Model
                {
                    ArticleName = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordV1Model.ArticleName)], nameof(AccrualRecordV1Model.ArticleName)),
                    Sku = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordV1Model.Sku)], nameof(AccrualRecordV1Model.Sku)),
                    AccrualType = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordV1Model.AccrualType)], nameof(AccrualRecordV1Model.AccrualType)),
                    SellerCost = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordV1Model.SellerCost)], nameof(AccrualRecordV1Model.SellerCost)),
                    Quantity = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordV1Model.Quantity)], nameof(AccrualRecordV1Model.Quantity)),
                    SummaryValue = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordV1Model.SummaryValue)], nameof(AccrualRecordV1Model.SummaryValue)),
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
            var headerIndexes = GetHeaderIndexes<AdvertisingModel>(headerRow);

            var result = rows
                .Skip(1)
                .Select(row => new AdvertisingModel
                {
                    Sku = row.GetFieldByIndex(headerIndexes[nameof(AdvertisingModel.Sku)], nameof(AdvertisingModel.Sku)),
                    PromotionType = row.GetFieldByIndex(headerIndexes[nameof(AdvertisingModel.PromotionType)], nameof(AdvertisingModel.PromotionType)),
                    Cost = row.GetFieldByIndex(headerIndexes[nameof(AdvertisingModel.Cost)], nameof(AdvertisingModel.Cost))
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
            var headerIndexes = GetHeaderIndexes<PrimeCostModel>(headerRow);

            var result = rows
                .Skip(1)
                .Select(row => new PrimeCostModel
                {
                    ArticleName = row.GetFieldByIndex(headerIndexes[nameof(PrimeCostModel.ArticleName)], nameof(PrimeCostModel.ArticleName)),
                    MaterialCost = row.GetFieldByIndex(headerIndexes[nameof(PrimeCostModel.MaterialCost)], nameof(PrimeCostModel.MaterialCost)),
                    WorkCost = row.GetFieldByIndex(headerIndexes[nameof(PrimeCostModel.WorkCost)], nameof(PrimeCostModel.WorkCost)),
                    Total = row.GetFieldByIndex(headerIndexes[nameof(PrimeCostModel.Total)], nameof(PrimeCostModel.Total))
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
            var headerIndexes = GetHeaderIndexes<AccrualRecordWbModel>(headerRow);

            var result = rows
                .Skip(1)
                .Select(row => new AccrualRecordWbModel
                {
                    ArticleName = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordWbModel.ArticleName)], nameof(AccrualRecordWbModel.ArticleName)),
                    Sku = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordWbModel.Sku)], nameof(AccrualRecordWbModel.Sku)),
                    AmountPayableToSeller = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordWbModel.AmountPayableToSeller)], nameof(AccrualRecordWbModel.AmountPayableToSeller)),
                    DocumentType = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordWbModel.DocumentType)], nameof(AccrualRecordWbModel.DocumentType)),
                    Logistics = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordWbModel.Logistics)], nameof(AccrualRecordWbModel.Logistics)),
                    PaymentReason = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordWbModel.PaymentReason)], nameof(AccrualRecordWbModel.PaymentReason)),
                    Quantity = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordWbModel.Quantity)], nameof(AccrualRecordWbModel.Quantity)),
                    RetailPrice = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordWbModel.RetailPrice)], nameof(AccrualRecordWbModel.RetailPrice)),
                    SupplierArticleName = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordWbModel.SupplierArticleName)], nameof(AccrualRecordWbModel.SupplierArticleName)),
                    TypesOfLogisticsPenaltiesAndAdjustments = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordWbModel.TypesOfLogisticsPenaltiesAndAdjustments)], nameof(AccrualRecordWbModel.TypesOfLogisticsPenaltiesAndAdjustments)),
                    PaidAcceptance = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordWbModel.PaidAcceptance)], nameof(AccrualRecordWbModel.PaidAcceptance)),
                    TotalAmountOfFines = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordWbModel.TotalAmountOfFines)], nameof(AccrualRecordWbModel.TotalAmountOfFines)),
                    StorageFee = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordWbModel.StorageFee)], nameof(AccrualRecordWbModel.StorageFee)),
                    Withholdings = row.GetFieldByIndex(headerIndexes[nameof(AccrualRecordWbModel.Withholdings)], nameof(AccrualRecordWbModel.Withholdings)),
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
                    Sku = row.GetFieldByIndex(headerIndexes[nameof(PrimeCostWbModel.Sku)], nameof(PrimeCostWbModel.Sku)),
                    ArticleName = row.GetFieldByIndex(headerIndexes[nameof(PrimeCostWbModel.ArticleName)], nameof(PrimeCostWbModel.ArticleName)),
                    Brand = row.FindFieldByIndex(headerIndexes[nameof(PrimeCostWbModel.Brand)], nameof(PrimeCostWbModel.Brand)) ?? string.Empty,
                    MaterialCost = row.GetFieldByIndex(headerIndexes[nameof(PrimeCostWbModel.MaterialCost)], nameof(PrimeCostWbModel.MaterialCost)),
                    WorkCost = row.GetFieldByIndex(headerIndexes[nameof(PrimeCostWbModel.WorkCost)], nameof(PrimeCostWbModel.WorkCost)),
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
