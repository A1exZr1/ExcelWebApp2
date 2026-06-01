using System.ComponentModel;

namespace ExcelWebApp2.Models
{
    public class WbCancellationModel
    {
        [Description("Артикул Wildberries")]
        public string Sku { get; set; } = string.Empty;

        [Description("Артикул продавца")]
        public string SupplierArticleName { get; set; } = string.Empty;

        [Description("Статус задания")]
        public string Status { get; set; } = string.Empty;
    }
}