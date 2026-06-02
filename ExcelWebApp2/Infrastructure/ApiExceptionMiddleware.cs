using System.Net;

namespace ExcelWebApp2.Infrastructure
{
    public class ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (ApiException exception)
            {
                await WriteErrorResponse(context, GetStatusCode(exception.Category), exception.Message);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unhandled API error");
                await WriteErrorResponse(context, HttpStatusCode.InternalServerError, "Internal server error");
            }
        }

        private static HttpStatusCode GetStatusCode(ApiExceptionCategory category)
        {
            return category switch
            {
                ApiExceptionCategory.BadRequest => HttpStatusCode.BadRequest,
                ApiExceptionCategory.NotFound => HttpStatusCode.NotFound,
                ApiExceptionCategory.Forbidden => HttpStatusCode.Forbidden,
                ApiExceptionCategory.Unauthorized => HttpStatusCode.Unauthorized,
                ApiExceptionCategory.Conflict => HttpStatusCode.Conflict,
                _ => HttpStatusCode.InternalServerError
            };
        }

        private static async Task WriteErrorResponse(HttpContext context, HttpStatusCode statusCode, string message)
        {
            if (context.Response.HasStarted)
            {
                return;
            }

            context.Response.Clear();
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "text/plain; charset=utf-8";
            await context.Response.WriteAsync(message);
        }
    }
}