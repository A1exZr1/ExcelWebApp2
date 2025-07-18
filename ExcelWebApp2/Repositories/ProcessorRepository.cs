using ExcelWebApp2.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ExcelWebApp2.Repositories
{
    public class ProcessorRepository
    {
        private List<AccrualRecordModel> _accruals = [];
        private List<AdvertisingModel> _ads = [];
        private List<PrimeCostModel> _primeCosts = [];
        private List<ProcessedResultModel> _processedResults = [];
        private static readonly Regex decimalParceRegex = new(@"\d*\.\d*\.\d*", RegexOptions.Compiled);

        public void SetAccruals(List<AccrualRecordModel> accruals) => _accruals = accruals;
        public void SetAds(List<AdvertisingModel> ads) => _ads = ads;
        public void SetPrimeCosts(List<PrimeCostModel> costs) => _primeCosts = costs;

        public bool HasAllInputs()
        {
            return _accruals.Count != 0 && _ads.Count != 0 && _primeCosts.Count != 0;
        }
        public List<ProcessedResultModel> GetLastProcessedResults()
        {
            return _processedResults;
        }

        public void Clear()
        {
            _accruals.Clear();
            _ads.Clear();
            _primeCosts.Clear();
            _processedResults.Clear();
        }

        public string GetMissingInputs()
        {
            var result = string.Empty;
            if (_accruals.Count == 0) result = "Файл начислений отсутствует\n";
            if (_ads.Count == 0) result += "Файл рекламы отсутствует\n";
            if (_primeCosts.Count == 0) result += "Файл себестоимости отсутствует\n";
            return result;
        }

        public List<ProcessedResultModel> Process()
        {
            var unlinkedExpense = _accruals
                .Where(x => string.IsNullOrWhiteSpace(x.ArticleName)
                    && !x.AccrualType.Equals("трафареты", StringComparison.CurrentCultureIgnoreCase)
                    && !x.AccrualType.Equals("продвижение с оплатой за заказ", StringComparison.CurrentCultureIgnoreCase))
                .Sum(x => GetParsedDecimal(x.SummaryValue));

            var totalSellerCost = _accruals
                .Where(x => !string.IsNullOrWhiteSpace(x.ArticleName)
                    && x.AccrualType.Equals("выручка", StringComparison.CurrentCultureIgnoreCase))
                .Sum(x => GetParsedDecimal(x.SellerCost));

            var skuSellerCost = _accruals
                .Where(x => !string.IsNullOrWhiteSpace(x.ArticleName)
                    && x.AccrualType.Equals("выручка", StringComparison.CurrentCultureIgnoreCase))
                .GroupBy(x => x.Sku)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(x => GetParsedDecimal(x.SellerCost))
                );

            var result = _accruals
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

                    decimal? primeCost = _primeCosts
                        .Where(x => x.ArticleName == articleName)
                        .Select(x => (decimal?)GetParsedDecimal(x.Total) * quantity)
                        .FirstOrDefault();

                    var netProfit = Math.Round(summary - adCost - (primeCost?? 0) + proportionalUnlinkedExpense?? 0, 3);

                    return new ProcessedResultModel
                    {
                        ArticleName = articleName,
                        Sku = sku,
                        TotalSumm = summary,
                        Quantity = quantity,
                        Revenue = revenue,
                        AdvertisingCost = adCost,
                        PrimeCost = primeCost,
                        NetProfit = netProfit,
                        UnlinkedExpenses = proportionalUnlinkedExpense ?? 0,
                        ProfitPercent = summary != 0 ? Math.Round((netProfit / Math.Abs(summary)) * 100, 2) : null
                    };
                })
                .ToList();

            _processedResults = result;
            return _processedResults;
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
