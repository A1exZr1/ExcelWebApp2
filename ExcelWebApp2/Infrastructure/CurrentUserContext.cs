using System.Security.Claims;

namespace ExcelWebApp2.Infrastructure
{
    public interface ICurrentUserContext
    {
        string UserId { get; }
    }

    public class CurrentUserContext(IHttpContextAccessor httpContextAccessor) : ICurrentUserContext
    {
        public string UserId
        {
            get
            {
                var userId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(userId))
                {
                    throw new ApiException(ApiExceptionCategory.Unauthorized, "User is not authenticated.");
                }

                return userId;
            }
        }
    }
}
