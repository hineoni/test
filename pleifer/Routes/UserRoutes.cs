using pleifer.Models;
using pleifer.Services;
using Microsoft.AspNetCore.Mvc;

namespace pleifer.Routes;

public static class UserRoutes
{
    public static void MapUserRoutes(this WebApplication app)
    {
        app.MapPost("/signup", (AuthService service, User user) =>
        {
            if (string.IsNullOrEmpty(user?.Username) || string.IsNullOrEmpty(user?.Password))
            {
                return Results.BadRequest(new ProblemDetails
                {
                    Status = 400,
                    Title = "Некорректный запрос",
                    Detail = "username и password обязательны для заполнения"
                });
            }

            try
            {
                var usersService = new UsersService();
                var createdUser = usersService.AddUser(user.Username, user.Password);

                return createdUser != null
                    ? Results.Ok(new
                    {
                        id = createdUser.Id,
                        username = createdUser.Username,
                        token = service.Authenticate(createdUser),
                    })
                    : Results.Problem("Произошла ошибка, попробуйте tomorrow");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Ошибка регистрации: {e}");

                return Results.Problem(new ProblemDetails
                {
                    Status = 500,
                    Title = "Ошибка сервера",
                    Detail = e.Message
                });
            }
        });

        app.MapPost("/login", (AuthService service, User user) =>
        {
            var usersService = new UsersService();
            try
            {
                var authUser = usersService.ValidateUser(user.Username, user.Password);
                if (authUser == null) return Results.Unauthorized();

                return Results.Ok(new
                {
                    token = service.Authenticate(authUser),
                    username = authUser.Username
                });
            }
            catch (Exception e)
            {
                Console.WriteLine($"Ошибка аутентификации: {e}");
                return Results.Problem(new ProblemDetails
                {
                    Status = 500,
                    Title = "Ошибка сервера",
                    Detail = e.Message
                });
            }
        });

        app.MapPatch("/change-password", (AuthService authService, ChangePassword changePassword) =>
        {
            if (
                string.IsNullOrEmpty(changePassword.Username)
                || string.IsNullOrEmpty(changePassword.OldPassword)
                || string.IsNullOrEmpty(changePassword.NewPassword)
            )
            {
                return Results.BadRequest(new ProblemDetails
                {
                    Status = 400,
                    Title = "Некорректный запрос",
                    Detail = "username, oldPassword и newPassword обязательны для заполнения"
                });
            }

            try
            {
                var usersService = new UsersService();
                var user = usersService.ChangePassword(
                    changePassword.Username,
                    changePassword.OldPassword,
                    changePassword.NewPassword
                );

                if (user == null)
                {
                    return Results.BadRequest(new ProblemDetails
                    {
                        Status = 400,
                        Title = "Некорректный запрос",
                        Detail = "Произошла ошибка при смене пароля"
                    });
                }

                return Results.Ok(new
                {
                    message = "Пароль успешно изменен",
                    token = authService.Authenticate(user)
                });
            }
            catch (Exception e)
            {
                Console.WriteLine($"Ошибка смены пароля: {e}");

                return Results.Problem(new ProblemDetails
                {
                    Status = 500,
                    Title = "Ошибка сервера",
                    Detail = e.Message
                });
            }
        })
        .RequireAuthorization();

        app.MapGet("/user-requests", (HttpContext context, AuthService authService) =>
        {
            var userId = authService.GetUserId(context);
            var usersService = new UsersService();
            var requestHistory = usersService.GetUserRequestHistory(userId);

            return Results.Ok(new
            {
                data = requestHistory
            });
        }).RequireAuthorization();

        app.MapDelete("/user-requests", (HttpContext context, AuthService authService) =>
        {
            var userId = authService.GetUserId(context);
            var usersService = new UsersService();
            usersService.DeleteUserRequestLog(userId);

            return Results.Ok();
        }).RequireAuthorization();
    }
}