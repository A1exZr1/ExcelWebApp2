using ExcelWebApp2.Infrastructure;
using ExcelWebApp2.Models;
using ExcelWebApp2.Repositories;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace ExcelWebApp2.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]/[action]")]
    public class ReportFilesController(FileReaderRepository fileReaderRepository, ProcessorRepository processorRepository) : ControllerBase
    {
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<UploadFileResponse> ReadAccrualV1([OpenApiFile] IFormFile file)
        {
            var result = await ReadFile<AccrualRecordV1Model>(file);
            processorRepository.SetAccrualsV1(result.Data);
            return CreateUploadResponse(result.Data.Count, result.Message);
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<UploadFileResponse> ReadAccrualV2([OpenApiFile] IFormFile file)
        {
            var result = await ReadFile<AccrualRecordV2Model>(file);
            processorRepository.SetAccrualsV2(result.Data);
            return CreateUploadResponse(result.Data.Count, result.Message);
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<UploadFileResponse> ReadAdvertisment([OpenApiFile] IFormFile file)
        {
            var result = await ReadFile<AdvertisingModel>(file);
            processorRepository.SetAds(result.Data);
            return CreateUploadResponse(result.Data.Count, result.Message);
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<UploadFileResponse> ReadPrimeCostModel([OpenApiFile] IFormFile file)
        {
            var result = await ReadFile<PrimeCostModel>(file);
            processorRepository.SetPrimeCosts(result.Data);
            return CreateUploadResponse(result.Data.Count, result.Message);
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<UploadFileResponse> ReadAccrualsWb([FromForm(Name = "file"), OpenApiFile] IFormFile[] files)
        {
            RequireFiles(files);

            var rows = new List<AccrualRecordWbModel>();
            var messages = new List<string>();
            foreach (var file in files)
            {
                var result = await ReadFile<AccrualRecordWbModel>(file);
                rows.AddRange(result.Data);

                if (!string.IsNullOrEmpty(result.Message))
                {
                    messages.Add(result.Message);
                }
            }

            processorRepository.SetAccrualsWb(rows);
            return CreateUploadResponse(rows.Count, string.Join("; ", messages));
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<UploadFileResponse> PrimeCostModelWb([OpenApiFile] IFormFile file)
        {
            var result = await ReadFile<PrimeCostWbModel>(file);
            processorRepository.SetPrimeCostsWb(result.Data);
            return CreateUploadResponse(result.Data.Count, result.Message);
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<UploadFileResponse> ReadWbCancellations([OpenApiFile] IFormFile file)
        {
            var result = await ReadFile<WbCancellationModel>(file);
            processorRepository.SetWbCancellations(result.Data);
            return CreateUploadResponse(result.Data.Count, result.Message);
        }

        private async Task<ReadResult<T>> ReadFile<T>(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw BadRequest("Файл не загружен.");
            }

            var result = await fileReaderRepository.ReadExcelFile<T>(file);
            if (!result.Success)
            {
                throw BadRequest(result.Message ?? "Не удалось прочитать файл.");
            }

            return result;
        }

        private static void RequireFiles(IFormFile[] files)
        {
            if (files == null || files.Length == 0)
            {
                throw BadRequest("Файлы не загружен.");
            }
        }

        private static UploadFileResponse CreateUploadResponse(int count, string? message)
        {
            return new UploadFileResponse
            {
                Count = count,
                Message = message ?? string.Empty
            };
        }

        private static ApiException BadRequest(string message)
        {
            return new ApiException(ApiExceptionCategory.BadRequest, message);
        }
    }
}
