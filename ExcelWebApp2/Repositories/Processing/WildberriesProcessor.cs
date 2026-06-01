using ExcelWebApp2.Models;

namespace ExcelWebApp2.Repositories.Processing
{
    public class WildberriesProcessor : ReportProcessorBase, IWildberriesProcessor
    {
        public List<ProcessedWbResultModel> Process(
            IReadOnlyCollection<AccrualRecordWbModel> accruals,
            IReadOnlyCollection<PrimeCostWbModel> primeCosts)
        {
            var primeBySku = primeCosts
                .Where(x => !string.IsNullOrWhiteSpace(x.Sku))
                .GroupBy(x => x.Sku.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var primeByArticle = primeCosts
                .Where(x => !string.IsNullOrWhiteSpace(x.ArticleName))
                .GroupBy(x => x.ArticleName.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            string GetBrand(string supplierArticleName, string sku)
            {
                if (!string.IsNullOrWhiteSpace(sku) && primeBySku.TryGetValue(sku.Trim(), out var bySku))
                    return bySku.Brand?.Trim() ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(supplierArticleName) && primeByArticle.TryGetValue(supplierArticleName.Trim(), out var byArticle))
                    return byArticle.Brand?.Trim() ?? string.Empty;

                return string.Empty;
            }

            var fullRetailPriceSumm = accruals
                .Where(x => x.DocumentType.Equals("Продажа", StringComparison.OrdinalIgnoreCase) && x.PaymentReason.Equals("Продажа", StringComparison.OrdinalIgnoreCase))
                .Sum(x => GetParsedDecimal(x.RetailPrice, LabelOf<AccrualRecordWbModel>(nameof(AccrualRecordWbModel.RetailPrice))));

            var fullRetailBrandPriceSumm = accruals
                .Where(x => x.DocumentType.Equals("Продажа", StringComparison.OrdinalIgnoreCase) && x.PaymentReason.Equals("Продажа", StringComparison.OrdinalIgnoreCase)
                        && IsReviewBrand(GetBrand(x.SupplierArticleName, x.Sku)))
                .Sum(x => GetParsedDecimal(x.RetailPrice, LabelOf<AccrualRecordWbModel>(nameof(AccrualRecordWbModel.RetailPrice))));

            var fullAdvertisingCost = accruals
                .Where(w => string.IsNullOrEmpty(w.DocumentType)
                    && w.PaymentReason.Equals("удержание", StringComparison.CurrentCultureIgnoreCase)
                    && w.TypesOfLogisticsPenaltiesAndAdjustments.Contains("Оказание услуг «WB Продвижение»", StringComparison.CurrentCultureIgnoreCase)
                    && !string.IsNullOrEmpty(w.Withholdings))
                .Sum(w => GetParsedDecimal(w.Withholdings, LabelOf<AccrualRecordWbModel>(nameof(AccrualRecordWbModel.Withholdings))));

            var fullReviewPointsCost = accruals
                .Where(w => string.IsNullOrEmpty(w.DocumentType)
                    && w.PaymentReason.Equals("удержание", StringComparison.CurrentCultureIgnoreCase)
                    && w.TypesOfLogisticsPenaltiesAndAdjustments.Contains("Списание за отзыв", StringComparison.CurrentCultureIgnoreCase)
                    && !string.IsNullOrEmpty(w.Withholdings))
                .Sum(w => GetParsedDecimal(w.Withholdings, LabelOf<AccrualRecordWbModel>(nameof(AccrualRecordWbModel.Withholdings))));

            var result = accruals
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
                        var retailPriceSumm = group
                            .Where(x => x.DocumentType.Equals("Продажа", StringComparison.OrdinalIgnoreCase) && x.PaymentReason.Equals("Продажа", StringComparison.OrdinalIgnoreCase))
                            .Sum(x => GetParsedDecimal(x.RetailPrice, LabelOf<AccrualRecordWbModel>(nameof(AccrualRecordWbModel.RetailPrice))));

                        var quantity = group
                            .Where(x => x.DocumentType.Equals("Продажа", StringComparison.OrdinalIgnoreCase) && x.PaymentReason.Equals("Продажа", StringComparison.OrdinalIgnoreCase))
                            .Sum(x => GetParsedDecimal(x.Quantity, LabelOf<AccrualRecordWbModel>(nameof(AccrualRecordWbModel.Quantity))));

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

                        var paidAcceptanceSumm = group
                            .Where(x => string.IsNullOrWhiteSpace(x.DocumentType) && x.PaymentReason.Equals("Платная приемка", StringComparison.OrdinalIgnoreCase))
                            .Sum(x => GetParsedDecimal(x.PaidAcceptance, LabelOf<AccrualRecordWbModel>(nameof(AccrualRecordWbModel.PaidAcceptance))));

                        var payableFinesSumm = group
                            .Where(x => string.IsNullOrWhiteSpace(x.DocumentType) && x.PaymentReason.Equals("Штраф", StringComparison.OrdinalIgnoreCase))
                            .Sum(x => GetParsedDecimal(x.TotalAmountOfFines, LabelOf<AccrualRecordWbModel>(nameof(AccrualRecordWbModel.TotalAmountOfFines))));

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

                        var (workCost, materialCost) = GetPrimeCosts(primeCosts, supplierArticleName, sku);
                        var allWorkCost = (workCost ?? 0) * quantity;
                        var allMaterialCost = (materialCost ?? 0) * quantity;

                        var netProfit = Math.Round(amountPayableToSellerSumm - allWorkCost - allMaterialCost - logisticSumm - paidAcceptanceSumm -
                            payableFinesSumm - returnSumm + (returnQuantity * (materialCost ?? 0)), 3);

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

            var storageSum = accruals
                .Where(x => string.IsNullOrEmpty(x.ArticleName) && string.IsNullOrEmpty(x.DocumentType) && x.PaymentReason.Equals("Хранение", StringComparison.OrdinalIgnoreCase))
                .Sum(x => GetParsedDecimal(x.StorageFee));

            var withholdingsSum = accruals
                .Where(x => string.IsNullOrEmpty(x.ArticleName) && string.IsNullOrEmpty(x.DocumentType) && x.PaymentReason.Equals("Удержание", StringComparison.OrdinalIgnoreCase))
                .Sum(x => GetParsedDecimal(x.Withholdings));

            AddAdditionalResult(result, -storageSum, "～ ХРАНЕНИЕ");
            AddAdditionalResult(result, -withholdingsSum, "～ УДЕРЖАНИЯ");

            return result;
        }

        private static void AddAdditionalResult(List<ProcessedWbResultModel> result, decimal netProfit, string supplierArticleName) => result.Add(new ProcessedWbResultModel
        {
            SupplierArticleName = supplierArticleName,
            Brand = string.Empty,
            AdvertisingCost = 0,
            ReviewPointsCost = 0,
            NetProfit = netProfit,
            WorkCost = 0,
            MaterialCost = 0,
            ProfitPercent = 0
        });

        private static bool IsReviewBrand(string brand)
        {
            return brand.Equals("OLSON", StringComparison.OrdinalIgnoreCase)
                || brand.Equals("OLSON premiato", StringComparison.OrdinalIgnoreCase);
        }

        private static (decimal? WorkCost, decimal? MaterialCost) GetPrimeCosts(
            IReadOnlyCollection<PrimeCostWbModel> primeCosts,
            string articleName,
            string sku)
        {
            var primeCost = primeCosts
                .FirstOrDefault(x => x.ArticleName == articleName || x.Sku == sku);

            try
            {
                if (primeCost == null)
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
    }
}
