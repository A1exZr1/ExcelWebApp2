using ExcelWebApp2.Infrastructure;
using ExcelWebApp2.Models;
using ExcelWebApp2.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ExcelWebApp2.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]/[action]")]
    public class ReportResultsController(ProcessorRepository processorRepository) : ControllerBase
    {
        [HttpGet]
        public List<ProcessedOzonResultV1Model> GetOzonV1Results()
        {
            RequireInputs(ProcessingType.OzonV1);
            return processorRepository.ProcessOzonV1();
        }

        [HttpGet]
        public List<ProcessedOzonResultV2Model> GetOzonV2Results()
        {
            RequireInputs(ProcessingType.OzonV2);
            return processorRepository.ProcessOzonV2();
        }

        [HttpGet]
        public List<ProcessedWbResultModel> GetWbResults([FromQuery] decimal returnMaterialDamagePercent = 15)
        {
            RequireInputs(ProcessingType.Wildberries);
            return processorRepository.ProcessWb(returnMaterialDamagePercent);
        }

        [HttpPost]
        public ApiMessageResponse Reset()
        {
            processorRepository.Clear();
            return new ApiMessageResponse { Message = "Все загруженные данные были удалены." };
        }

        private void RequireInputs(ProcessingType processingType)
        {
            if (processorRepository.HasAllInputs(processingType))
            {
                return;
            }

            var missing = processorRepository.GetMissingInputs(processingType);
            if (!string.IsNullOrEmpty(missing))
            {
                throw new ApiException(
                    ApiExceptionCategory.BadRequest,
                    "Не все входные файлы были загружены.\n" + missing);
            }
        }
    }
}
