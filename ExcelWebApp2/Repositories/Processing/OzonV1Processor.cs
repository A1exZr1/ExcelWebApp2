using ExcelWebApp2.Models;

namespace ExcelWebApp2.Repositories.Processing
{
    public class OzonV1Processor : ReportProcessorBase, IOzonV1Processor
    {
        public List<ProcessedOzonResultV1Model> Process(
            IReadOnlyCollection<AccrualRecordV1Model> accruals,
            IReadOnlyCollection<AdvertisingModel> ads,
            IReadOnlyCollection<PrimeCostModel> primeCosts)
        {
            var unlinkedExpense = accruals
                .Where(x => string.IsNullOrWhiteSpace(x.ArticleName)
                    && !x.AccrualType.Equals("трафареты", StringComparison.CurrentCultureIgnoreCase)
                    && !x.AccrualType.Equals("продвижение с оплатой за заказ", StringComparison.CurrentCultureIgnoreCase))
                .Sum(x => GetParsedDecimal(x.SummaryValue));

            var totalSellerCost = accruals
                .Where(x => !string.IsNullOrWhiteSpace(x.ArticleName)
                    && x.AccrualType.Equals("выручка", StringComparison.CurrentCultureIgnoreCase))
                .Sum(x => GetParsedDecimal(x.SellerCost));

            var skuSellerCost = accruals
                .Where(x => !string.IsNullOrWhiteSpace(x.ArticleName)
                    && x.AccrualType.Equals("выручка", StringComparison.CurrentCultureIgnoreCase))
                .GroupBy(x => x.Sku)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(x => GetParsedDecimal(x.SellerCost))
                );

            return accruals
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

                    var adCost = ads
                        .Where(x => x.Sku == sku)
                        .Sum(x => GetParsedDecimal(x.Cost));

                    var (workCost, materialCost) = GetPrimeCosts(primeCosts, articleName, quantity);

                    var netProfit = Math.Round(summary - adCost - (workCost ?? 0) - (materialCost ?? 0) + (proportionalUnlinkedExpense ?? 0), 3);

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
