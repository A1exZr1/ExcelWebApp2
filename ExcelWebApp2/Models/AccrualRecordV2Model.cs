using System.ComponentModel;

namespace ExcelWebApp2.Models
{
    public class AccrualRecordV2Model
    {
        [Description("Артикул")]
        public string ArticleName { get; set; } = string.Empty;

        [Description("SKU")]
        public string Sku { get; set; } = string.Empty;

        [Description("Тип начисления")]
        public string AccrualType { get; set; } = string.Empty;

        [Description("Склад отгрузки")]
        public string Warehouse { get; set; } = string.Empty;

        [Description("Количество")]
        public string Quantity { get; set; } = string.Empty;

        [Description("За продажу или возврат до вычета комиссий и услуг")]
        public string PreCommissionAmount { get; set; } = string.Empty;
        
        [Description("Вознаграждение Ozon")]
        public string OzonFee { get; set; } = string.Empty;

        [Description("Обработка отправления (Drop-off/Pick-up) (разбивается по товарам пропорционально количеству в отправлении)")]
        public string HandlingFee { get; set; } = string.Empty;

        [Description("Последняя миля (разбивается по товарам пропорционально доле цены товара в сумме отправления)")]
        public string LastMileFee { get; set; } = string.Empty;

        [Description("Логистика")]
        public string LogisticsFee { get; set; } = string.Empty;

        [Description("Итого, руб.")]
        public string TotalAmount { get; set; } = string.Empty;
    }
}
