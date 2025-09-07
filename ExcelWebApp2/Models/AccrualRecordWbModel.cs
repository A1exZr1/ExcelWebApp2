using System.ComponentModel;

namespace ExcelWebApp2.Models
{
    public class    AccrualRecordWbModel
    {
        [Description("Предмет")]
        public string ArticleName { get; set; } = string.Empty;

        [Description("Код номенклатуры")]
        public string Sku { get; set; } = string.Empty;

        [Description("Артикул поставщика")]
        public string SupplierArticleName { get; set; } = string.Empty;

        [Description("Тип документа")]
        public string DocumentType { get; set; } = string.Empty;

        [Description("Кол-во")]
        public string Quantity { get; set; } = string.Empty;

        [Description("Обоснование для оплаты")]
        public string PaymentReason { get; set; } = string.Empty;

        [Description("Цена розничная")]
        public string RetailPrice { get; set; } = string.Empty;

        [Description("К перечислению Продавцу за реализованный Товар")]
        public string AmountPayableToSeller { get; set; } = string.Empty;

        [Description("Услуги по доставке товара покупателю")]
        public string Logistics { get; set; } = string.Empty;

        [Description("Виды логистики, штрафов и корректировок ВВ")]
        public string TypesOfLogisticsPenaltiesAndAdjustments { get; set; } = string.Empty;

        [Description("Платная приемка")]
        public string PaidAcceptance { get; set; } = string.Empty;  

        [Description("Общая сумма штрафов")]
        public string TotalAmountOfFines { get; set; } = string.Empty;

    }
}
