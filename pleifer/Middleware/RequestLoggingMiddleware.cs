using pleifer.Services;

namespace pleifer.Middleware;

public class RequestLoggingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, AuthService authService)
    {
        if (authService.IsUserAuthenticated(context))
        {
            var userId = authService.GetUserId(context);
            var requestDetails = $"[{DateTime.Now}] Request: {context.Request.Path}";

            var usersService = new UsersService();
            usersService.AddUserRequestLog(userId, requestDetails);
        }

        await next(context);
    }
}
