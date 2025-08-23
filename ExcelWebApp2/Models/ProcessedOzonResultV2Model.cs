namespace ExcelWebApp2.Models
{
    public class ProcessedOzonResultV2Model
    {
        public string ArticleName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string Warehouse { get; set; } = string.Empty;
        public decimal Quantity { get; set; }

        public decimal PreCommissionAmount { get; set; }

        public decimal OzonFee { get; set; }
        public decimal HandlingFee { get; set; }
        public decimal LastMileFee { get; set; }
        public decimal LogisticsFee { get; set; }

        public decimal? WorkCost { get; set; }
        public decimal? MaterialCost { get; set; }
        public decimal UnlinkedExpenses { get; set; }
        public decimal NetProfit { get; set; }
        public decimal? ProfitPercent { get; set; }
        public Dictionary<string, decimal> AdditionalFees { get; set; } = [];
    }
}
