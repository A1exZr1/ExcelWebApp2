using System.ComponentModel;

namespace ExcelWebApp2.Models
{
    public class AccrualRecordV1Model
    {
        [Description("Артикул")]
        public string ArticleName { get; set; } = string.Empty;

        [Description("SKU")]
        public string Sku { get; set; } = string.Empty;

        [Description("Тип начисления")]
        public string AccrualType { get; set; } = string.Empty;

        [Description("Цена продавца")]
        public string SellerCost { get; set; } = string.Empty;

        [Description("Количество")]
        public string Quantity { get; set; } = string.Empty;

        [Description("Сумма итого, руб")]
        public string SummaryValue { get; set; } = string.Empty;
    }
}
