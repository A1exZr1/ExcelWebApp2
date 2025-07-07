using DocumentFormat.OpenXml.Drawing.Diagrams;
using System.ComponentModel;

namespace ExcelWebApp2.Models
{
    public class AdvertisingModel
    {
        [Description("SKU")]
        public string Sku { get; set; } = string.Empty;

        [Description("Тип продвижения")]
        public string PromotionType { get; set; } = string.Empty;

        [Description("Расход, ₽, с НДС")]
        public string Cost { get; set; } = string.Empty;
    }
}
