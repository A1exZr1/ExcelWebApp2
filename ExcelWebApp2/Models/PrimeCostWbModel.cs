using System.ComponentModel;

namespace ExcelWebApp2.Models
{
    public class PrimeCostWbModel
    {
        [Description("Артикул WB")]
        public string Sku { get; set; } = string.Empty;

        [Description("Артикул продавца")]
        public string ArticleName { get; set; } = string.Empty;

        [Description("Бренд")]
        public string Brand { get; set; } = string.Empty;

        [Description("Материалы")]
        public string MaterialCost { get; set; } = string.Empty;

        [Description("Работа")]
        public string WorkCost { get; set; } = string.Empty;

    }
}
