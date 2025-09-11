using ExcelWebApp2.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ExcelWebApp2.Repositories
{
    public class ProcessorRepository
    {
        private List<AccrualRecordV1Model> _accrualsV1 = [];
        private List<AccrualRecordV2Model> _accrualsV2 = [];
        private List<AccrualRecordWbModel> _accrualsWb = [];
        private List<AdvertisingModel> _ads = [];
        private List<PrimeCostModel> _primeCosts = [];
        private List<PrimeCostWbModel> primeCostWbModels = [];
        private List<ProcessedOzonResultV1Model> _processedResultsV1 = [];
        private List<ProcessedOzonResultV2Model> _processedResultsV2 = [];
        private List<ProcessedWbResultModel> _processedResultsWb = [];
        private static readonly Regex decimalParceRegex = new(@"\d*\.\d*\.\d*", RegexOptions.Compiled);

        public void SetAccrualsV1(List<AccrualRecordV1Model> accruals) => _accrualsV1 = accruals;
        public void SetAccrualsV2(List<AccrualRecordV2Model> accruals) => _accrualsV2 = accruals;
        public void SetAccrualsWb(List<AccrualRecordWbModel> accruals) => _accrualsWb = accruals;
        public void SetAds(List<AdvertisingModel> ads) => _ads = ads;
        public void SetPrimeCosts(List<PrimeCostModel> costs) => _primeCosts = costs;
        public void SetPrimeCostsWb(List<PrimeCostWbModel> costs) => primeCostWbModels = costs;

        public bool HasAllInputs(ProcessingType processingType)
        {
            return processingType switch
            {
                ProcessingType.OzonV1 => _accrualsV1.Count != 0 && _ads.Count != 0 && _primeCosts.Count != 0,
                ProcessingType.OzonV2 => _accrualsV2.Count != 0 && _primeCosts.Count != 0,
                ProcessingType.Wildberries => _accrualsWb.Count != 0 && primeCostWbModels.Count != 0,
                _ => throw new NotImplementedException(),
            };
        }

        public void Clear()
        {
            _accrualsV1.Clear();
            _accrualsV2.Clear();
            _ads.Clear();
            _primeCosts.Clear();
            _processedResultsV1.Clear();
            _processedResultsV2.Clear();
            _accrualsWb.Clear();
            primeCostWbModels.Clear();
        }

        public string GetMissingInputs(ProcessingType processingType)
        {
            var result = string.Empty;
            if (_primeCosts.Count == 0) result += "Файл себестоимости отсутствует\n";

            if (processingType == ProcessingType.OzonV2)
            {
                if (_accrualsV2.Count == 0) result += "Файл отчёта по товарам отсутствует\n";
                return result;
            }
            else if (processingType == ProcessingType.OzonV1)
            {
                if (_accrualsV1.Count == 0) result += "Файл отчёта по товарам отсутствует\n";
                if (_ads.Count == 0) result += "Файл рекламы отсутствует\n";
                return result;
            }
            else if (processingType == ProcessingType.Wildberries)
            {
                if (_accrualsWb.Count == 0) result += "Файлы отчётов по товарам отсутствует\n";
                if (primeCostWbModels.Count == 0) result += "Файл себестоимости отсутствует\n";
                return result;
            }
            return result;
        }

        public List<ProcessedOzonResultV1Model> ProcessOzonV1()
        {
            var unlinkedExpense = _accrualsV1
                .Where(x => string.IsNullOrWhiteSpace(x.ArticleName)
                    && !x.AccrualType.Equals("трафареты", StringComparison.CurrentCultureIgnoreCase)
                    && !x.AccrualType.Equals("продвижение с оплатой за заказ", StringComparison.CurrentCultureIgnoreCase))
                .Sum(x => GetParsedDecimal(x.SummaryValue));

            var totalSellerCost = _accrualsV1
                .Where(x => !string.IsNullOrWhiteSpace(x.ArticleName)
                    && x.AccrualType.Equals("выручка", StringComparison.CurrentCultureIgnoreCase))
                .Sum(x => GetParsedDecimal(x.SellerCost));

            var skuSellerCost = _accrualsV1
                .Where(x => !string.IsNullOrWhiteSpace(x.ArticleName)
                    && x.AccrualType.Equals("выручка", StringComparison.CurrentCultureIgnoreCase))
                .GroupBy(x => x.Sku)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(x => GetParsedDecimal(x.SellerCost))
                );

            var result = _accrualsV1
                .Where(x => !string.IsNullOrEmpty(x.ArticleName))
                .GroupBy(x => new { x.ArticleName, x.Sku })
                .Select(group =>
                {
                    var articleName = group.Key.ArticleName;
                    var sku = group.Key.Sku;

                    skuSellerCost.TryGetValue(sku, out decimal skuSellerCostValue);
                    decimal? proportionalUnlinkedExpense = null;
                    if (totalSellerCost != 0 && skuSellerCostValue > 0)
                    {
                        proportionalUnlinkedExpense = -Math.Round((skuSellerCostValue / totalSellerCost) * Math.Abs(unlinkedExpense), 3);
                    }

                    var summary = group.Sum(x => GetParsedDecimal(x.SummaryValue));
                    var revenue = group
                        .Where(x => x.AccrualType.Equals("выручка", StringComparison.OrdinalIgnoreCase))
                        .Sum(x => GetParsedDecimal(x.SellerCost));

                    var quantity = group
                        .Where(x => x.AccrualType.Equals("выручка", StringComparison.OrdinalIgnoreCase))
                        .Sum(x => GetParsedDecimal(x.Quantity));

                    var adCost = _ads
                        .Where(x => x.Sku == sku)
                        .Sum(x => GetParsedDecimal(x.Cost));

                    var (workCost, materialCost) = GetPrimeCosts(articleName, quantity);

                    var netProfit = Math.Round(summary - adCost - (workCost ?? 0) - (materialCost ?? 0) + proportionalUnlinkedExpense?? 0, 3);

                    return new ProcessedOzonResultV1Model
                    {
                        ArticleName = articleName,
                        Sku = sku,
                        TotalSumm = summary,
                        Quantity = quantity,
                        Revenue = revenue,
                        AdvertisingCost = adCost,
                        WorkCost = workCost,
                        MaterialCost = materialCost,
                        NetProfit = netProfit,
                        UnlinkedExpenses = proportionalUnlinkedExpense ?? 0,
                        ProfitPercent = summary != 0 ? Math.Round((netProfit / Math.Abs(summary)) * 100, 2) : null
                    };
                })
                .ToList();

            _processedResultsV1 = result;
            return _processedResultsV1;
        }

        public List<ProcessedOzonResultV2Model> ProcessOzonV2()
        {
            var unlinkedExpense = _accrualsV2
                .Where(x => string.IsNullOrWhiteSpace(x.ArticleName))
                .Sum(x => GetParsedDecimal(x.TotalAmount));

            var filterTypes = _accrualsV2
                .Select(x => x.AccrualType)
                .Where(x => !string.IsNullOrWhiteSpace(x) && x != "Доставка покупателю")
                .Distinct()
                .ToList();

            var totalPreComissionCost = _accrualsV2
                .Where(x => !string.IsNullOrWhiteSpace(x.ArticleName)
                    && x.AccrualType.Equals("Доставка покупателю", StringComparison.CurrentCultureIgnoreCase))
                .Sum(x => GetParsedDecimal(x.PreCommissionAmount));

            var skuPreComissionCost = _accrualsV2
                .Where(x => !string.IsNullOrWhiteSpace(x.ArticleName)
                    && x.AccrualType.Equals("Доставка покупателю", StringComparison.CurrentCultureIgnoreCase))
                .GroupBy(x => x.Sku)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(x => GetParsedDecimal(x.PreCommissionAmount))
                );

            var result = _accrualsV2
                .Where(x => !string.IsNullOrEmpty(x.ArticleName))
                .GroupBy(x => new { x.ArticleName, x.Sku })
                .Select(group =>
                {
                    var articleName = group.Key.ArticleName;
                    var sku = group.Key.Sku;

                    skuPreComissionCost.TryGetValue(sku, out decimal skuSellerCostValue);
                    decimal? proportionalUnlinkedExpense = null;
                    
                    if (totalPreComissionCost != 0 && skuSellerCostValue > 0)
                    {
                        proportionalUnlinkedExpense = -Math.Round((skuSellerCostValue / totalPreComissionCost) * Math.Abs(unlinkedExpense), 3);
                    }

                    var quantity = group
                        .Where(x => x.AccrualType.Equals("Доставка покупателю", StringComparison.OrdinalIgnoreCase))
                        .Count();
                    
                    var (workCost, materialCost) = GetPrimeCosts(articleName, quantity);

                    var ozonFee = group
                        .Where(x => x.AccrualType.Equals("Доставка покупателю", StringComparison.OrdinalIgnoreCase))
                        .Sum(x => GetParsedDecimal(x.OzonFee));

                    var handlingFee = group
                        .Where(x => x.AccrualType.Equals("Доставка покупателю", StringComparison.OrdinalIgnoreCase))
                        .Sum(x => GetParsedDecimal(x.HandlingFee));

                    var lastMileFee = group
                        .Where(x => x.AccrualType.Equals("Доставка покупателю", StringComparison.OrdinalIgnoreCase))
                        .Sum(x => GetParsedDecimal(x.LastMileFee));

                    var logisticsFee = group
                        .Where(x => x.AccrualType.Equals("Доставка покупателю", StringComparison.OrdinalIgnoreCase))
                        .Sum(x => GetParsedDecimal(x.LogisticsFee));

                    var additionalFees = new Dictionary<string, decimal>();

                    foreach (var type in filterTypes)
                    {
                        var feeSum = group
                            .Where(x => x.AccrualType.Equals(type, StringComparison.OrdinalIgnoreCase))
                            .Sum(x => GetParsedDecimal(x.TotalAmount));

                        if (feeSum != 0)
                        {
                            additionalFees[type] = feeSum;
                        }
                    }

                    var additionalFeesTotal = additionalFees.Values.Sum();

                    var netProfit = Math.Round(skuSellerCostValue - (workCost ?? 0) - (materialCost ?? 0) + (proportionalUnlinkedExpense ?? 0) + ozonFee + handlingFee + lastMileFee + logisticsFee + additionalFeesTotal, 3);

                    return new ProcessedOzonResultV2Model
                    {
                        ArticleName = articleName,
                        Sku = sku,
                        Warehouse = group.FirstOrDefault(x => x.AccrualType == "Доставка покупателю")?.Warehouse ?? "Не определён",
                        PreCommissionAmount = skuSellerCostValue,
                        Quantity = quantity,
                        WorkCost = workCost,
                        MaterialCost = materialCost,
                        NetProfit = netProfit,
                        OzonFee = ozonFee,
                        HandlingFee = handlingFee,
                        LastMileFee = lastMileFee,
                        LogisticsFee = logisticsFee,
                        UnlinkedExpenses = proportionalUnlinkedExpense ?? 0,
                        ProfitPercent = skuSellerCostValue != 0 ? Math.Round((netProfit / Math.Abs(skuSellerCostValue)) * 100, 2) : null,
                        AdditionalFees = additionalFees
                    };
                })
                .ToList();

            _processedResultsV2 = result;
            return _processedResultsV2;
        }


        public List<ProcessedWbResultModel> ProcessWb()
        {
            var result = _accrualsWb
                .Where(x => !string.IsNullOrEmpty(x.SupplierArticleName))
                .GroupBy(x => new { x.SupplierArticleName, x.Sku })
                .Select(group =>
                {
                    var supplierArticleName = group.Key.SupplierArticleName;
                    var sku = group.Key.Sku;
                    var articleName = group.FirstOrDefault()?.ArticleName ?? string.Empty;

                    // цена розничная
                    var retailPriceSumm = group
                        .Where(x => x.DocumentType.Equals("Продажа", StringComparison.OrdinalIgnoreCase) && x.PaymentReason.Equals("Продажа", StringComparison.OrdinalIgnoreCase))
                        .Sum(x => GetParsedDecimal(x.RetailPrice));

                    var quantity = group
                        .Where(x => x.DocumentType.Equals("Продажа", StringComparison.OrdinalIgnoreCase) && x.PaymentReason.Equals("Продажа", StringComparison.OrdinalIgnoreCase))
                        .Sum(x => GetParsedDecimal(x.Quantity));

                    //К перечислению Продавцу за реализованный Товар
                    var amountPayableToSellerSumm = group
                        .Where(x => x.DocumentType.Equals("Продажа", StringComparison.OrdinalIgnoreCase) && x.PaymentReason.Equals("Продажа", StringComparison.OrdinalIgnoreCase))
                        .Sum(x => GetParsedDecimal(x.AmountPayableToSeller));

                    var logisticSumm = group
                        .Where(x => string.IsNullOrWhiteSpace(x.DocumentType) && x.PaymentReason.Equals("Логистика", StringComparison.OrdinalIgnoreCase))
                        .Sum(x => GetParsedDecimal(x.Logistics));

                    var cancellationSumm = group
                        .Where(x => x.TypesOfLogisticsPenaltiesAndAdjustments.Equals("К клиенту при отмене", StringComparison.OrdinalIgnoreCase))
                        .Sum(x => GetParsedDecimal(x.Logistics));

                    var cancellationQuantity = group
                        .Count(x => x.TypesOfLogisticsPenaltiesAndAdjustments.Equals("К клиенту при отмене", StringComparison.OrdinalIgnoreCase));

                    //Платная приёмка
                    var paidAcceptanceSumm = group
                        .Where(x => string.IsNullOrWhiteSpace(x.DocumentType) && x.PaymentReason.Equals("Платная приемка", StringComparison.OrdinalIgnoreCase))
                        .Sum(x => GetParsedDecimal(x.PaidAcceptance));

                    //Штрафы
                    var payableFinesSumm = group
                        .Where(x => string.IsNullOrWhiteSpace(x.DocumentType) && x.PaymentReason.Equals("Штраф", StringComparison.OrdinalIgnoreCase))
                        .Sum(x => GetParsedDecimal(x.TotalAmountOfFines));

                    var returnSumm = group
                        .Where(x => x.DocumentType.Equals("Возврат", StringComparison.OrdinalIgnoreCase) && x.PaymentReason.Equals("Возврат", StringComparison.OrdinalIgnoreCase))
                        .Sum(x => GetParsedDecimal(x.AmountPayableToSeller));

                    var returnQuantity = group
                        .Count(x => x.DocumentType.Equals("Возврат", StringComparison.OrdinalIgnoreCase) && x.PaymentReason.Equals("Возврат", StringComparison.OrdinalIgnoreCase));

                    var (workCost, materialCost) = GetPrimeWbCosts(supplierArticleName, sku, quantity);

                    var netProfit = Math.Round(amountPayableToSellerSumm - (workCost ?? 0) - (materialCost ?? 0) - logisticSumm - (cancellationSumm * cancellationQuantity) - paidAcceptanceSumm -
                        payableFinesSumm - (returnSumm * returnQuantity) + (returnQuantity * (materialCost ?? 0)), 3);

                    return new ProcessedWbResultModel
                    {
                        SupplierArticleName = supplierArticleName,
                        ArticleName = articleName,
                        Sku = sku,
                        RetailPriceSumm = retailPriceSumm,
                        Quantity = quantity,
                        AmountPayableToSellerSumm = amountPayableToSellerSumm,
                        LogisticsFee = logisticSumm,
                        CancelledSumm = cancellationSumm,
                        CancelledQuantity = cancellationQuantity,
                        PaidAcceptanceSumm = paidAcceptanceSumm,
                        TotalAmountOfFines = payableFinesSumm,
                        ReturnedSumm = returnSumm,
                        ReturnedQuantity = returnQuantity,
                        WorkCost = workCost,
                        MaterialCost = materialCost,
                        NetProfit = netProfit,
                        ProfitPercent = amountPayableToSellerSumm != 0 ? Math.Round((netProfit / Math.Abs(amountPayableToSellerSumm)) * 100, 2) : null
                    };
                })
                .ToList();

            _processedResultsWb = result;
            return _processedResultsWb;
        }


        private (decimal? WorkCost, decimal? MaterialCost) GetPrimeCosts(string articleName, decimal quantity)
        {
            var primeCost = _primeCosts
                .Where(x => x.ArticleName == articleName)
                .FirstOrDefault();

            return primeCost is null ? (null, null) : (GetParsedDecimal(primeCost.WorkCost) * quantity, GetParsedDecimal(primeCost.MaterialCost) * quantity);
        }

        private (decimal? WorkCost, decimal? MaterialCost) GetPrimeWbCosts(string articleName, string sku, decimal quantity)
        {
            var primeCost = primeCostWbModels
                .Where(x => x.ArticleName == articleName || x.Sku == sku)
                .FirstOrDefault();

            return primeCost is null ? (null, null) : (GetParsedDecimal(primeCost.WorkCost) * quantity, GetParsedDecimal(primeCost.MaterialCost) * quantity);
        }

        protected static decimal GetParsedDecimal(string textPrice, bool isPriceInverted = false)
        {
            var processedTextPrice = textPrice.Replace(" ", "").Replace(",", ".");
            if (decimalParceRegex.Match(processedTextPrice).Success)
            {
                var stringNumbers = processedTextPrice.Split(".");
                processedTextPrice = string.Join(".", string.Join("", stringNumbers.SkipLast(1)), stringNumbers.Last());
            }

            var result = decimal.TryParse(processedTextPrice, NumberStyles.Any, new NumberFormatInfo() { NumberDecimalSeparator = "." }, out var decimalPrice)
                ? decimalPrice
                : throw new Exception($"Значение '{textPrice}'не может быть конвертировано в цену");

            return isPriceInverted ? -Math.Round(result, 2) : Math.Round(result, 2);
        }
    }
}
