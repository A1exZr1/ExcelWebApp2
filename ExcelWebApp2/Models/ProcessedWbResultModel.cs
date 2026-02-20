namespace ExcelWebApp2.Models
{
    public class ProcessedWbResultModel
    {
        public string ArticleName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string SupplierArticleName { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        
        public decimal Quantity { get; set; }
        public decimal RetailPriceSumm { get; set; }
        public decimal AmountPayableToSellerSumm { get; set; }

        public decimal LogisticsFee { get; set; }
        public int CancelledQuantity { get; set; }
        public decimal CancelledSumm { get; set; }
        public decimal PaidAcceptanceSumm { get; set; }
        public decimal TotalAmountOfFines { get; set; }
        public int ReturnedQuantity { get; set; }
        public decimal ReturnedSumm { get; set; }
        public decimal AdvertisingCost { get; set; }
        public decimal ReviewPointsCost { get; set; }

        public decimal? WorkCost { get; set; }
        public decimal? MaterialCost { get; set; }
        public decimal NetProfit { get; set; }
        public decimal? ProfitPercent { get; set; }
    }
}
