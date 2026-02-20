using ExcelWebApp2.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
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
                        .Sum(x => GetParsedDecimal(x.Quantity, LabelOf<AccrualRecordWbModel>(nameof(AccrualRecordWbModel.Quantity))));

                    var adCost = _ads
                        .Where(x => x.Sku == sku)
                        .Sum(x => GetParsedDecimal(x.Cost));

                    var (workCost, materialCost) = GetPrimeOzonCosts(articleName, quantity);

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
                    
                    var (workCost, materialCost) = GetPrimeOzonCosts(articleName, quantity);

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
            var primeBySku = primeCostWbModels
                .Where(x => !string.IsNullOrWhiteSpace(x.Sku))
                .GroupBy(x => x.Sku, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var primeByArticle = primeCostWbModels
                .Where(x => !string.IsNullOrWhiteSpace(x.ArticleName))
                .GroupBy(x => x.ArticleName, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            string GetBrand(string supplierArticleName, string sku)
            {
                if (!string.IsNullOrWhiteSpace(sku) && primeBySku.TryGetValue(sku, out var bySku))
                    return bySku.Brand?.Trim() ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(supplierArticleName) && primeByArticle.TryGetValue(supplierArticleName, out var byArticle))
                    return byArticle.Brand?.Trim() ?? string.Empty;

                return string.Empty;
            }

            bool IsReviewBrand(string brand) => brand.Equals("OLSON", StringComparison.OrdinalIgnoreCase) || brand.Equals("OLSON premiato", StringComparison.OrdinalIgnoreCase);

            var fullRetailPriceSumm = _accrualsWb
                .Where(x => x.DocumentType.Equals("Продажа", StringComparison.OrdinalIgnoreCase) && x.PaymentReason.Equals("Продажа", StringComparison.OrdinalIgnoreCase))
                .Sum(x => GetParsedDecimal(x.RetailPrice, LabelOf<AccrualRecordWbModel>(nameof(AccrualRecordWbModel.RetailPrice))));

            var fullRetailBrandPriceSumm = _accrualsWb
                .Where(x => x.DocumentType.Equals("Продажа", StringComparison.OrdinalIgnoreCase) && x.PaymentReason.Equals("Продажа", StringComparison.OrdinalIgnoreCase) && IsReviewBrand(GetBrand(x.ArticleName, x.Sku)))
                .Sum(x => GetParsedDecimal(x.RetailPrice, LabelOf<AccrualRecordWbModel>(nameof(AccrualRecordWbModel.RetailPrice))));

            var fullAdvertisingCost = _accrualsWb
                .Where(w => string.IsNullOrEmpty(w.DocumentType)
                    && w.PaymentReason.Equals("удержание", StringComparison.CurrentCultureIgnoreCase)
                    && w.TypesOfLogisticsPenaltiesAndAdjustments.Contains("Оказание услуг «WB Продвижение»", StringComparison.CurrentCultureIgnoreCase)
                    && !string.IsNullOrEmpty(w.Withholdings))
                .Sum(w => GetParsedDecimal(w.Withholdings, LabelOf<AccrualRecordWbModel>(nameof(AccrualRecordWbModel.Withholdings))));

            var fullReviewPointsCost = _accrualsWb
                .Where(w => string.IsNullOrEmpty(w.DocumentType)
                    && w.PaymentReason.Equals("удержание", StringComparison.CurrentCultureIgnoreCase)
                    && w.TypesOfLogisticsPenaltiesAndAdjustments.Contains("Списание за отзыв", StringComparison.CurrentCultureIgnoreCase)
                    && !string.IsNullOrEmpty(w.Withholdings))
                .Sum(w => GetParsedDecimal(w.Withholdings, LabelOf<AccrualRecordWbModel>(nameof(AccrualRecordWbModel.Withholdings))));

            var result = _accrualsWb
                .Where(x => !string.IsNullOrEmpty(x.SupplierArticleName))
                .GroupBy(x => new { x.SupplierArticleName, x.Sku })
                .Select(group =>
                {
                    var supplierArticleName = group.Key.SupplierArticleName;
                    var sku = group.Key.Sku;
                    var articleName = group.FirstOrDefault()?.ArticleName ?? string.Empty;
                    var brand = GetBrand(supplierArticleName, sku);

                    try
                    {
                        // цена розничная
                        var retailPriceSumm = group
                            .Where(x => x.DocumentType.Equals("Продажа", StringComparison.OrdinalIgnoreCase) && x.PaymentReason.Equals("Продажа", StringComparison.OrdinalIgnoreCase))
                            .Sum(x => GetParsedDecimal(x.RetailPrice, LabelOf<AccrualRecordWbModel>(nameof(AccrualRecordWbModel.RetailPrice))));

                        var quantity = group
                            .Where(x => x.DocumentType.Equals("Продажа", StringComparison.OrdinalIgnoreCase) && x.PaymentReason.Equals("Продажа", StringComparison.OrdinalIgnoreCase))
                            .Sum(x => GetParsedDecimal(x.Quantity, LabelOf<AccrualRecordWbModel>(nameof(AccrualRecordWbModel.Quantity))));

                        //К перечислению Продавцу за реализованный Товар
                        var amountPayableToSellerSumm = group
                            .Where(x => x.DocumentType.Equals("Продажа", StringComparison.OrdinalIgnoreCase) && x.PaymentReason.Equals("Продажа", StringComparison.OrdinalIgnoreCase))
                            .Sum(x => GetParsedDecimal(x.AmountPayableToSeller, LabelOf<AccrualRecordWbModel>(nameof(AccrualRecordWbModel.AmountPayableToSeller))));

                        var logisticSumm = group
                            .Where(x => string.IsNullOrWhiteSpace(x.DocumentType) && x.PaymentReason.Equals("Логистика", StringComparison.OrdinalIgnoreCase))
                            .Sum(x => GetParsedDecimal(x.Logistics, LabelOf<AccrualRecordWbModel>(nameof(AccrualRecordWbModel.Logistics))));

                        var cancellationSumm = group
                            .Where(x => x.TypesOfLogisticsPenaltiesAndAdjustments.Equals("К клиенту при отмене", StringComparison.OrdinalIgnoreCase))
                            .Sum(x => GetParsedDecimal(x.Logistics, LabelOf<AccrualRecordWbModel>(nameof(AccrualRecordWbModel.Logistics))));

                        var cancellationQuantity = group
                            .Count(x => x.TypesOfLogisticsPenaltiesAndAdjustments.Equals("К клиенту при отмене", StringComparison.OrdinalIgnoreCase));

                        //Платная приёмка
                        var paidAcceptanceSumm = group
                            .Where(x => string.IsNullOrWhiteSpace(x.DocumentType) && x.PaymentReason.Equals("Платная приемка", StringComparison.OrdinalIgnoreCase))
                            .Sum(x => GetParsedDecimal(x.PaidAcceptance, LabelOf<AccrualRecordWbModel>(nameof(AccrualRecordWbModel.PaidAcceptance))));

                        //Штрафы
                        var payableFinesSumm = group
                            .Where(x => string.IsNullOrWhiteSpace(x.DocumentType) && x.PaymentReason.Equals("Штраф", StringComparison.OrdinalIgnoreCase))
                            .Sum(x => GetParsedDecimal(x.TotalAmountOfFines, LabelOf<AccrualRecordWbModel>(nameof(AccrualRecordWbModel.TotalAmountOfFines))));

                        //реклама по соотношению от суммы
                        decimal? proportionalAdvertisingCost = null;
                        if (fullRetailPriceSumm != 0 && retailPriceSumm > 0)
                        {
                            proportionalAdvertisingCost = Math.Round((retailPriceSumm / fullRetailPriceSumm) * Math.Abs(fullAdvertisingCost), 3);
                        }

                        decimal? proportionalreviewPointsCost = null;
                        if (fullRetailBrandPriceSumm != 0 && retailPriceSumm > 0 && IsReviewBrand(brand))
                        {
                            proportionalreviewPointsCost = Math.Round((retailPriceSumm / fullRetailBrandPriceSumm) * Math.Abs(fullReviewPointsCost), 3);
                        }

                        var returnSumm = group
                            .Where(x => x.DocumentType.Equals("Возврат", StringComparison.OrdinalIgnoreCase) && x.PaymentReason.Equals("Возврат", StringComparison.OrdinalIgnoreCase))
                            .Sum(x => GetParsedDecimal(x.AmountPayableToSeller, LabelOf<AccrualRecordWbModel>(nameof(AccrualRecordWbModel.AmountPayableToSeller))));

                        var returnQuantity = group
                            .Count(x => x.DocumentType.Equals("Возврат", StringComparison.OrdinalIgnoreCase) && x.PaymentReason.Equals("Возврат", StringComparison.OrdinalIgnoreCase));

                        var (workCost, materialCost) = GetPrimeWbCosts(supplierArticleName, sku);
                        var allWorkCost = (workCost ?? 0) * quantity;
                        var allMaterialCost = (materialCost ?? 0) * quantity;

                        var netProfit = Math.Round(amountPayableToSellerSumm - allWorkCost - allMaterialCost - logisticSumm - paidAcceptanceSumm -
                            payableFinesSumm - returnSumm - (proportionalAdvertisingCost ?? 0) - (proportionalreviewPointsCost ?? 0) + (returnQuantity * (materialCost ?? 0)), 3);

                        return new ProcessedWbResultModel
                        {
                            SupplierArticleName = supplierArticleName,
                            ArticleName = articleName,
                            Sku = sku,
                            Brand = brand,
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
                            WorkCost = workCost is null ? null : allWorkCost,
                            MaterialCost = materialCost is null ? null : allMaterialCost,
                            AdvertisingCost = proportionalAdvertisingCost ?? 0,
                            ReviewPointsCost = proportionalreviewPointsCost ?? 0,
                            NetProfit = netProfit,
                            ProfitPercent = amountPayableToSellerSumm != 0 ? Math.Round((netProfit / Math.Abs(amountPayableToSellerSumm)) * 100, 2) : null
                        };
                    }
                    catch (Exception e)
                    {
                        return new ProcessedWbResultModel
                        {
                            ArticleName = articleName,
                            Sku = sku,
                            SupplierArticleName = "Ошибка при чтении данных: " + e.Message,
                            WorkCost = null,
                            MaterialCost = null,
                            Brand = brand,
                            ProfitPercent = null
                        };
                    }
                })
                .ToList();


            var storageSum = _accrualsWb
                .Where(x => string.IsNullOrEmpty(x.ArticleName) && string.IsNullOrEmpty(x.DocumentType) && x.PaymentReason.Equals("Хранение", StringComparison.OrdinalIgnoreCase))
                .Sum(x => GetParsedDecimal(x.StorageFee));

            var withholdingsSum = _accrualsWb
                .Where(x => string.IsNullOrEmpty(x.ArticleName) && string.IsNullOrEmpty(x.DocumentType) && x.PaymentReason.Equals("Удержание", StringComparison.OrdinalIgnoreCase))
                .Sum(x => GetParsedDecimal(x.Withholdings));

            result.Add(new ProcessedWbResultModel
            {
                SupplierArticleName = "～ ХРАНЕНИЕ",
                Brand = string.Empty,
                AdvertisingCost = 0,
                ReviewPointsCost = 0,
                NetProfit = -storageSum,
                WorkCost = 0,
                MaterialCost = 0,
                ProfitPercent = 0
            });
            result.Add(new ProcessedWbResultModel
            {
                SupplierArticleName = "～ УДЕРЖАНИЯ",
                Brand = string.Empty,
                AdvertisingCost = 0,
                ReviewPointsCost = 0,
                NetProfit = -withholdingsSum,
                WorkCost = 0,
                MaterialCost = 0,
                ProfitPercent = 0
            });

            _processedResultsWb = result;
            return _processedResultsWb;
        }


        private (decimal? WorkCost, decimal? MaterialCost) GetPrimeOzonCosts(string articleName, decimal quantity)
        {
            var primeCost = _primeCosts
                .Where(x => x.ArticleName == articleName)
                .FirstOrDefault();

            return primeCost is null ? (null, null) : (GetParsedDecimal(primeCost.WorkCost, LabelOf<PrimeCostModel>(nameof(PrimeCostModel.WorkCost)) ) * quantity, GetParsedDecimal(primeCost.MaterialCost, LabelOf<PrimeCostModel>(nameof(PrimeCostModel.MaterialCost))) * quantity);
        }

        private (decimal? WorkCost, decimal? MaterialCost) GetPrimeWbCosts(string articleName, string sku)
        {
            var primeCost = primeCostWbModels
                .Where(x => x.ArticleName == articleName || x.Sku == sku)
                .FirstOrDefault();

            try
            {
                if(primeCost == null) 
                    return (null, null);

                var parsedWorkCost = GetParsedDecimal(primeCost.WorkCost, LabelOf<PrimeCostWbModel>(nameof(PrimeCostWbModel.WorkCost)));
                var parsedMaterialCost = GetParsedDecimal(primeCost.MaterialCost, LabelOf<PrimeCostWbModel>(nameof(PrimeCostWbModel.MaterialCost)));
                return (parsedWorkCost, parsedMaterialCost);
            }
            catch (Exception)
            {
                return (null, null);
            }
        }

        protected static decimal GetParsedDecimal(string textPrice, string fieldName = "", bool isPriceInverted = false)
        {
            var processedTextPrice = textPrice.Replace(" ", "").Replace(",", ".");
            if (decimalParceRegex.Match(processedTextPrice).Success)
            {
                var stringNumbers = processedTextPrice.Split(".");
                processedTextPrice = string.Join(".", string.Join("", stringNumbers.SkipLast(1)), stringNumbers.Last());
            }

            var result = decimal.TryParse(processedTextPrice, NumberStyles.Any, new NumberFormatInfo() { NumberDecimalSeparator = "." }, out var decimalPrice)
                ? decimalPrice
                : throw new Exception($"Значение '{textPrice}' поля '{fieldName}' не может быть конвертировано в цену");

            return isPriceInverted ? -Math.Round(result, 2) : Math.Round(result, 2);
        }

        private static string LabelOf<T>(string propertyName)
        {
            return typeof(T).GetProperty(propertyName)?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? propertyName;
        }
    }
}
