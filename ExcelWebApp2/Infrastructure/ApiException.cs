namespace ExcelWebApp2.Infrastructure
{
    public class ApiException(ApiExceptionCategory category, string message) : Exception(message)
    {
        public ApiExceptionCategory Category { get; } = category;
    }
}