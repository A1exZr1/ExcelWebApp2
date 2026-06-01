using ExcelWebApp2.Models;

namespace ExcelWebApp2.Repositories.Processing
{
    public class OzonV2Processor : ReportProcessorBase, IOzonV2Processor
    {
        public List<ProcessedOzonResultV2Model> Process(
            IReadOnlyCollection<AccrualRecordV2Model> accruals,
            IReadOnlyCollection<PrimeCostModel> primeCosts)
        {
            var unlinkedExpense = accruals
                .Where(x => string.IsNullOrWhiteSpace(x.ArticleName))
                .Sum(x => GetParsedDecimal(x.TotalAmount));

            var filterTypes = accruals
                .Select(x => x.AccrualType)
                .Where(x => !string.IsNullOrWhiteSpace(x) && x != "Доставка покупателю")
                .Distinct()
                .ToList();

            var totalPreComissionCost = accruals
                .Where(x => !string.IsNullOrWhiteSpace(x.ArticleName)
                    && x.AccrualType.Equals("Доставка покупателю", StringComparison.CurrentCultureIgnoreCase))
                .Sum(x => GetParsedDecimal(x.PreCommissionAmount));

            var skuPreComissionCost = accruals
                .Where(x => !string.IsNullOrWhiteSpace(x.ArticleName)
                    && x.AccrualType.Equals("Доставка покупателю", StringComparison.CurrentCultureIgnoreCase))
                .GroupBy(x => x.Sku)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(x => GetParsedDecimal(x.PreCommissionAmount))
                );

            return accruals
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
                        .Sum(x => GetParsedDecimal(x.Quantity, LabelOf<ProcessedOzonResultV2Model>(nameof(ProcessedOzonResultV2Model.Quantity))));

                    var (workCost, materialCost) = GetPrimeCosts(primeCosts, articleName, quantity);

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
        }

        private static (decimal? WorkCost, decimal? MaterialCost) GetPrimeCosts(
            IReadOnlyCollection<PrimeCostModel> primeCosts,
            string articleName,
            decimal quantity)
        {
            var primeCost = primeCosts
                .FirstOrDefault(x => x.ArticleName == articleName);

            return primeCost is null
                ? (null, null)
                : (
                    GetParsedDecimal(primeCost.WorkCost, LabelOf<PrimeCostModel>(nameof(PrimeCostModel.WorkCost))) * quantity,
                    GetParsedDecimal(primeCost.MaterialCost, LabelOf<PrimeCostModel>(nameof(PrimeCostModel.MaterialCost))) * quantity
                );
        }
    }
}
