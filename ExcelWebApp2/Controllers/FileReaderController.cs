using ExcelWebApp2.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ExcelWebApp2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileReaderController(FileReaderRepository repository) : ControllerBase
    {
        [HttpPost("ReadAccrual")]
        public async Task<IActionResult> ReadAccrual([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            using var stream = file.OpenReadStream();
            var result = await repository.ReadAccruals(stream);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(new { count = result.Data.Count, message = result.Message });
        }

    }
}
