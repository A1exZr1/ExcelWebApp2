using ExcelWebApp2.Models;
using ExcelWebApp2.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ExcelWebApp2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileReaderController(FileReaderRepository fileReaderRepository, ProcessorRepository processorRepository, ExcelExportService excelExportService) : ControllerBase
    {
        [HttpPost("ReadAccrualV1")]
        public async Task<IActionResult> ReadAccrualV1( IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Файл не загружен.");

            var result = await fileReaderRepository.ReadExcelFile<AccrualRecordV1Model>(file);

            if (!result.Success)
                return BadRequest(result.Message);

            processorRepository.SetAccrualsV1(result.Data);
            return Ok(new { count = result.Data.Count, message = result.Message });
        }

        [HttpPost("ReadAccrualV2")]
        public async Task<IActionResult> ReadAccrualV2(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Файл не загружен.");

            var result = await fileReaderRepository.ReadExcelFile<AccrualRecordV2Model>(file);

            if (!result.Success)
                return BadRequest(result.Message);

            processorRepository.SetAccrualsV2(result.Data);
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

        [HttpPost("ReadAccrualsWb")]
        public async Task<IActionResult> ReadAccrualsWb(IFormFile[] file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Файл не загружен.");

            var result = new List<AccrualRecordWbModel>();
            var messages = new List<string>();

            foreach (var f in file)
            {
                var temp = await fileReaderRepository.ReadExcelFile<AccrualRecordWbModel>(f);

                if (!temp.Success)
                    return BadRequest(temp.Message);

                result.AddRange(temp.Data);
                if (!string.IsNullOrEmpty(temp.Message))
                    messages.Add(temp.Message);
            }

            processorRepository.SetAccrualsWb(result);
            return Ok(new { count = result.Count, message = string.Join("; ", messages) });
        }

        [HttpPost("PrimeCostModelWb")]
        public async Task<IActionResult> PrimeCostModelWb(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Файл не загружен.");

            var result = await fileReaderRepository.ReadExcelFile<PrimeCostWbModel>(file);

            if (!result.Success)
                return BadRequest(result.Message);

            processorRepository.SetPrimeCostsWb(result.Data);
            return Ok(new { count = result.Data.Count, message = result.Message });
        }

        [HttpGet("GetOzonV1Results")]
        public IActionResult GetOzonV1Results()
        {
            if (!processorRepository.HasAllInputs(ProcessingType.OzonV1))
            {
                var missing = processorRepository.GetMissingInputs(ProcessingType.OzonV1);
                if (!string.IsNullOrEmpty(missing)) 
                    return BadRequest("Не все входные файлы были загружены.\n" + missing);
            }

            var results = processorRepository.ProcessOzonV1();
            return Ok(results);
        }

        [HttpGet("GetOzonV2Results")]
        public IActionResult GetOzonV2Results()
        {
            if (!processorRepository.HasAllInputs(ProcessingType.OzonV2))
            {
                var missing = processorRepository.GetMissingInputs(ProcessingType.OzonV2);
                if (!string.IsNullOrEmpty(missing))
                    return BadRequest("Не все входные файлы были загружены.\n" + missing);
            }

            var results = processorRepository.ProcessOzonV2();
            return Ok(results);
        }

        [HttpPost("Reset")]
        public IActionResult Reset()
        {
            processorRepository.Clear();
            return Ok(new { message = "Все загруженные данные были удалены." });
        }

        [HttpPost("ExportProcessedResultsV1")]
        public IActionResult ExportProcessedResultsV1([FromBody] List<ProcessedOzonResultV1Model> rows)
        {
            if (rows == null || rows.Count == 0)
                return BadRequest("Нет обработанных данных");

            var stream = excelExportService.Export(rows);
            return File(
                stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "processed_results.xlsx"
            );
        }

        [HttpPost("ExportProcessedResultsV2")]
        public IActionResult ExportProcessedResultsV2([FromBody] List<ProcessedOzonResultV2Model> rows)
        {
            if (rows == null || rows.Count == 0)
                return BadRequest("Нет обработанных данных");

            var stream = excelExportService.Export(rows);
            return File(
                stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "processed_results.xlsx"
            );
        }
    }
}
