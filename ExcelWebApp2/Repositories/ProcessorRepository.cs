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
        private static readonly Regex decimalParceRegex = new(@"\d*\.\d*\.\d*", RegexOptions.Compiled);

        public void SetAccruals(List<AccrualRecordModel> accruals) => _accruals = accruals;
        public void SetAds(List<AdvertisingModel> ads) => _ads = ads;
        public void SetPrimeCosts(List<PrimeCostModel> costs) => _primeCosts = costs;

        public bool HasAllInputs()
        {
            return _accruals.Count != 0 && _ads.Count != 0 && _primeCosts.Count != 0;
        }

        public void Clear()
        {
            _accruals.Clear();
            _ads.Clear();
            _primeCosts.Clear();
        }

        public string? GetMissingInputs()
        {
            if (_accruals.Count == 0) return "Accrual file is missing";
            if (_ads.Count == 0) return "Advertising file is missing";
            if (_primeCosts.Count == 0) return "Prime cost file is missing";
            return null;
        }

        public List<ProcessedResultModel> Process()
        {
            var result = _accruals
                .GroupBy(x => new { x.ArticleName, x.Sku })
                .Select(group =>
                {
                    var articleName = group.Key.ArticleName;
                    var sku = group.Key.Sku;

                    decimal summary = group.Sum(x => decimal.TryParse(x.SummaryValue, out var v) ? v : 0);
                    decimal revenue = group
                        .Where(x => x.AccrualType.Equals("выручка", StringComparison.OrdinalIgnoreCase))
                        .Sum(x => GetParsedDecimal(x.SellerCost));

                    decimal adCost = _ads
                        .Where(x => x.Sku == sku)
                        .Sum(x => GetParsedDecimal(x.Cost));

                    decimal primeCost = _primeCosts
                        .Where(x => x.ArticleName == articleName)
                        .Sum(x => GetParsedDecimal(x.MaterialCost) + GetParsedDecimal(x.WorkCost)
                        );

                    return new ProcessedResultModel
                    {
                        ArticleName = articleName,
                        Sku = sku,
                        Revenue = revenue,
                        AdvertisingCost = adCost,
                        PrimeCost = primeCost
                    };
                })
                .ToList();

            return result;
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
                : throw new Exception($"Value '{textPrice}' can't be parsed to the price amount");

            return isPriceInverted ? -Math.Round(result, 2) : Math.Round(result, 2);
        }
    }
}
