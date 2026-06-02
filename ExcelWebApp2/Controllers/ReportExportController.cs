using ExcelWebApp2.Infrastructure;
using ExcelWebApp2.Models;
using ExcelWebApp2.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ExcelWebApp2.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]/[action]")]
    public class ReportExportController(ExcelExportService excelExportService) : ControllerBase
    {
        [HttpPost]
        public IActionResult ExportProcessedResultsV1([FromBody] List<ProcessedOzonResultV1Model> rows)
        {
            RequireRows(rows);
            return CreateFileResult(excelExportService.Export(rows));
        }

        [HttpPost]
        public IActionResult ExportProcessedResultsV2([FromBody] List<ProcessedOzonResultV2Model> rows)
        {
            RequireRows(rows);
            return CreateFileResult(excelExportService.Export(rows));
        }

        [HttpPost]
        public IActionResult ExportProcessedResultsWb([FromBody] List<ProcessedWbResultModel> rows)
        {
            RequireRows(rows);
            return CreateFileResult(excelExportService.Export(rows));
        }

        private static void RequireRows<T>(List<T>? rows)
        {
            if (rows == null || rows.Count == 0)
            {
                throw new ApiException(ApiExceptionCategory.BadRequest, "Нет обработанных данных");
            }
        }

        private IActionResult CreateFileResult(MemoryStream stream)
        {
            return File(
                stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "processed_results.xlsx"
            );
        }
    }
}
