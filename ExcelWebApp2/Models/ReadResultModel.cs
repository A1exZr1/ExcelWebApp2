namespace ExcelWebApp2.Models
{
    public class ReadResult<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<T> Data { get; set; } = [];
    }
}
