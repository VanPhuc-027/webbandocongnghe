using _2280613193_webdocongnghe.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace _2280613193_webdocongnghe.Midleware
{
    public class CheckAccountStatusMiddleware
    {
        private readonly RequestDelegate _next;
        public CheckAccountStatusMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user != null && user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.Now)
                {
                    await context.SignOutAsync(IdentityConstants.ApplicationScheme);

                    context.Response.Redirect("Identity/Account/Login?message=T%C3%A0i+kho%E1%BA%A3n+c%E1%BB%A7a+b%E1%BA%A1n+%C4%91%C3%A3+b%E1%BB%8B+kh%C3%B3a.\");");
                    return;
                }
            }
            await _next(context);
        }
    }
    public static class CheckAccountStatusMiddlewareExtensions
    {
        public static IApplicationBuilder UseCheckAccountStatus(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CheckAccountStatusMiddleware>();
        }
    }
}
