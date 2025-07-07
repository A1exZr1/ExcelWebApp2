using ExcelWebApp2.Models;
using ExcelWebApp2.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ExcelWebApp2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileReaderController(FileReaderRepository repository) : ControllerBase
    {
        [HttpPost("ReadAccrual")]
        public async Task<IActionResult> ReadAccrual( IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var result = await repository.ReadExcelFile<AccrualRecordModel>(file);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(new { count = result.Data.Count, message = result.Message });
        }

        [HttpPost("ReadAdvertisment")]
        public async Task<IActionResult> ReadAdvertisment( IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var result = await repository.ReadExcelFile<AdvertisingModel>(file);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(new { count = result.Data.Count, message = result.Message });
        }

        [HttpPost("PrimeCostModel")]
        public async Task<IActionResult> ReadPrimeCostModel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var result = await repository.ReadExcelFile<PrimeCostModel>(file);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(new { count = result.Data.Count, message = result.Message });
        }

    }
}
