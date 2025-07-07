namespace ExcelWebApp2.Models
{
    public class ProcessedResultModel
    {
        public string ArticleName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;

        public decimal Revenue { get; set; }
        public decimal AdvertisingCost { get; set; }
        public decimal PrimeCost { get; set; }

        public decimal NetProfit => Revenue - AdvertisingCost - PrimeCost; 
        public decimal ProfitPercent => Revenue > 0 ? NetProfit / Revenue * 100 : 0;
    }
}
