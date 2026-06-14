using ExcelWebApp2.Infrastructure;
using ExcelWebApp2.Models;
using ExcelWebApp2.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ExcelWebApp2.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]/[action]")]
    public class AuthController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IProcessingSessionStore sessionStore) : ControllerBase
    {
        [HttpPost]
        [AllowAnonymous]
        public async Task<CurrentUserResponse> Login([FromBody] LoginRequest request)
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                throw UnauthorizedLogin();
            }

            var result = await signInManager.PasswordSignInAsync(
                user,
                request.Password,
                request.RememberMe,
                lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                throw UnauthorizedLogin();
            }

            sessionStore.Clear(user.Id);

            return new CurrentUserResponse
            {
                IsAuthenticated = true,
                Email = user.Email
            };
        }

        [HttpPost]
        [Authorize]
        public async Task<ApiMessageResponse> Logout()
        {
            var userId = userManager.GetUserId(User);
            if (!string.IsNullOrEmpty(userId))
            {
                sessionStore.Clear(userId);
            }

            await signInManager.SignOutAsync();
            return new ApiMessageResponse { Message = "Вы вышли из системы." };
        }

        [HttpGet]
        [AllowAnonymous]
        public CurrentUserResponse CurrentUser()
        {
            return new CurrentUserResponse
            {
                IsAuthenticated = User.Identity?.IsAuthenticated == true,
                Email = User.Identity?.IsAuthenticated == true ? User.Identity.Name : null
            };
        }

        private static ApiException UnauthorizedLogin()
        {
            return new ApiException(ApiExceptionCategory.Unauthorized, "Неверный логин или пароль.");
        }
    }
}
