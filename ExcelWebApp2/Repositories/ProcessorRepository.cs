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
            var result = _accruals
                .GroupBy(x => new { x.ArticleName, x.Sku })
                .Select(group =>
                {
                    var articleName = group.Key.ArticleName;
                    var sku = group.Key.Sku;

                    decimal summary = group.Sum(x => GetParsedDecimal(x.SummaryValue));
                    decimal revenue = group
                        .Where(x => x.AccrualType.Equals("выручка", StringComparison.OrdinalIgnoreCase))
                        .Sum(x => GetParsedDecimal(x.SellerCost));

                    decimal adCost = _ads
                        .Where(x => x.Sku == sku)
                        .Sum(x => GetParsedDecimal(x.Cost));

                    decimal primeCost = _primeCosts
                        .Where(x => x.ArticleName == articleName)
                        .Sum(x => GetParsedDecimal(x.Total));

                    decimal netProfit = Math.Round(revenue - adCost - primeCost, 2);

                    return new ProcessedResultModel
                    {
                        ArticleName = articleName,
                        Sku = sku,
                        TotalSumm = summary,
                        Revenue = revenue,
                        AdvertisingCost = adCost,
                        PrimeCost = primeCost,
                        NetProfit = netProfit,
                        ProfitPercent = revenue != 0 ? Math.Round((netProfit / revenue) * 100, 2) : null
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
