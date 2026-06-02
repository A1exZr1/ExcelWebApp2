namespace ExcelWebApp2.Models
{
    public class UploadFileResponse
    {
        public int Count { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class ApiMessageResponse
    {
        public string Message { get; set; } = string.Empty;
    }
}
