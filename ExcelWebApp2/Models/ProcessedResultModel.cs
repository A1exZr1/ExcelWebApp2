namespace ExcelWebApp2.Models
{
    public class ProcessedResultModel
    {
        public string ArticleName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;

        public decimal TotalSumm { get; set; }

        public decimal Revenue { get; set; }
        public decimal AdvertisingCost { get; set; }
        public decimal PrimeCost { get; set; }

        public decimal NetProfit { get; set; }
        public decimal? ProfitPercent { get; set; }
    }
}
