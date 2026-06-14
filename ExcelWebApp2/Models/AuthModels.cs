namespace ExcelWebApp2.Models
{
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }

    public class CurrentUserResponse
    {
        public bool IsAuthenticated { get; set; }
        public string? Email { get; set; }
    }
}
