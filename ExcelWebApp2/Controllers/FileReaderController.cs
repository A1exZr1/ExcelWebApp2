using ExcelWebApp2.Models;
using ExcelWebApp2.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ExcelWebApp2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileReaderController(FileReaderRepository fileReaderRepository, ProcessorRepository processorRepository, ExcelExportService excelExportService) : ControllerBase
    {
        [HttpPost("ReadAccrual")]
        public async Task<IActionResult> ReadAccrual( IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Файл не загружен.");

            var result = await fileReaderRepository.ReadExcelFile<AccrualRecordModel>(file);

            if (!result.Success)
                return BadRequest(result.Message);

            processorRepository.SetAccruals(result.Data);
            return Ok(new { count = result.Data.Count, message = result.Message });
        }

        [HttpPost("ReadAdvertisment")]
        public async Task<IActionResult> ReadAdvertisment( IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Файл не загружен.");

            var result = await fileReaderRepository.ReadExcelFile<AdvertisingModel>(file);

            if (!result.Success)
                return BadRequest(result.Message);

            processorRepository.SetAds(result.Data);
            return Ok(new { count = result.Data.Count, message = result.Message });
        }

        [HttpPost("PrimeCostModel")]
        public async Task<IActionResult> ReadPrimeCostModel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Файл не загружен.");

            var result = await fileReaderRepository.ReadExcelFile<PrimeCostModel>(file);

            if (!result.Success)
                return BadRequest(result.Message);

            processorRepository.SetPrimeCosts(result.Data);
            return Ok(new { count = result.Data.Count, message = result.Message });
        }

        [HttpGet("GetProcessedResults")]
        public IActionResult GetResults()
        {
            if (!processorRepository.HasAllInputs())
            {
                var missing = processorRepository.GetMissingInputs();
                if (!string.IsNullOrEmpty(missing)) 
                    return BadRequest("Не все входные файлы были загружены.\n" + missing);
            }

            var results = processorRepository.Process();
            return Ok(results);
        }

        [HttpPost("Reset")]
        public IActionResult Reset()
        {
            processorRepository.Clear();
            return Ok(new { message = "Все загруженные данные были удалены." });
        }

        [HttpGet("ExportProcessedResults")]
        public IActionResult ExportProcessedResults()
        {
            var results = processorRepository.GetLastProcessedResults();

            if (results == null || !results.Any())
                return BadRequest("Нет обработанных данных");

            var stream = excelExportService.Export(results);

            return File(stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "processed_results.xlsx");
        }
    }
}
