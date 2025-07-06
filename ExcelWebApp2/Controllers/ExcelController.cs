using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;

namespace ExcelWebApp2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExcelController : ControllerBase
    {
        [HttpPost("read")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult ReadFirstRow([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Файл не выбран.");

            try
            {
                using var stream = file.OpenReadStream();
                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheet(1);

                var firstRow = worksheet.Row(1)
                    .CellsUsed()
                    .Select(c => c.GetValue<string>())
                    .ToList();

                return Ok(firstRow);
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка при обработке файла: {ex.Message}");
            }
        }

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("pong");
        }
    }
}
