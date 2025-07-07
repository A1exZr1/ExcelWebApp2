using ExcelWebApp2.Models;
using ExcelWebApp2.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ExcelWebApp2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileReaderController(FileReaderRepository fileReaderRepository, ProcessorRepository processorRepository) : ControllerBase
    {
        [HttpPost("ReadAccrual")]
        public async Task<IActionResult> ReadAccrual( IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

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
                return BadRequest("No file uploaded.");

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
                return BadRequest("No file uploaded.");

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
                if (missing != null) 
                    return BadRequest("Not all input files have been uploaded. " + missing);
            }

            var results = processorRepository.Process();
            return Ok(results);
        }

        [HttpPost("Reset")]
        public IActionResult Reset()
        {
            processorRepository.Clear();
            return Ok(new { message = "All uploaded data has been cleared." });
        }
    }
}
