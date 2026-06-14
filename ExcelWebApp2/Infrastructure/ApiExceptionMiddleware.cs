using System.Net;
using Npgsql;

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
            catch (Exception exception) when (IsDatabaseException(exception))
            {
                logger.LogWarning(exception, "Database is unavailable");
                await WriteErrorResponse(context, HttpStatusCode.ServiceUnavailable, "Database is unavailable.");
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
                ApiExceptionCategory.ServiceUnavailable => HttpStatusCode.ServiceUnavailable,
                _ => HttpStatusCode.InternalServerError
            };
        }

        private static bool IsDatabaseException(Exception exception)
        {
            for (var current = exception; current != null; current = current.InnerException)
            {
                if (current is NpgsqlException)
                {
                    return true;
                }
            }

            return false;
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