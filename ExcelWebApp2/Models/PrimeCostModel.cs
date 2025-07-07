using System.ComponentModel;

namespace ExcelWebApp2.Models
{
    public class PrimeCostModel
    {
        [Description("Артикул")]
        public string ArticleName { get; set; } = string.Empty;

        [Description("Себестоимость материалов")]
        public string MaterialCost { get; set; } = string.Empty;

        [Description("Цена работы")]
        public string WorkCost { get; set; } = string.Empty;

        [Description("Итого")]
        public string Total { get; set; } = string.Empty;
    }
}
