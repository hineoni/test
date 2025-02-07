﻿﻿using System.Text.RegularExpressions;
using pleifer.Models;
using pleifer.Services;
using Microsoft.AspNetCore.Mvc;

namespace pleifer.Routes;

public static class TextRoutes
{
    private static readonly TextService TextService = new TextService();

    public static void MapTextRoutes(this WebApplication app)
    {
        app.MapGet("/texts", (HttpContext context, AuthService authService) =>
        {
            var userId = authService.GetUserId(context);
            var texts = TextService.GetAllTexts(userId);
            return Results.Ok(new
            {
                data = texts
            });
        }).RequireAuthorization();

        app.MapGet("/texts/{id}", (HttpContext context, AuthService authService, int id) =>
        {
            try
            {
                var userId = authService.GetUserId(context);
                var text = TextService.GetText(userId, id);

                if (text == null) return Results.NotFound();

                return Results.Ok(new
                {
                    data = text
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                return Results.Problem(new ProblemDetails
                {
                    Status = 500,
                    Title = "Ошибка сервера",
                    Detail = e.Message
                });
            }

        }).RequireAuthorization();

        app.MapPost("/texts", (HttpContext context, AuthService authService, Text text) =>
        {
            if (string.IsNullOrEmpty(text?.Content))
            {
                return Results.BadRequest(new ProblemDetails
                {
                    Status = 400,
                    Title = "Некорректный запрос",
                    Detail = "поле content обязательно для заполнения"
                });
            }

            var userId = authService.GetUserId(context);
            TextService.AddText(text, userId);
            return Results.NoContent();
        }).RequireAuthorization();

        app.MapPatch("/texts/{id}", (HttpContext context, AuthService authService, int id, Text text) =>
        {
            if (string.IsNullOrEmpty(text?.Content))
            {
                return Results.BadRequest(new ProblemDetails
                {
                    Status = 400,
                    Title = "Некорректный запрос",
                    Detail = "поле content обязательно для заполнения"
                });
            }

            var userId = authService.GetUserId(context);
            var result = TextService.UpdateText(userId, id, text.Content);
            if (!result) return Results.NotFound();

            return Results.NoContent();
        }).RequireAuthorization();

        app.MapDelete("/texts/{id}", (HttpContext context, AuthService authService, int id) =>
        {
            var userId = authService.GetUserId(context);
            var result = TextService.DeleteText(userId, id);
            if (!result) return Results.NotFound();

            return Results.NoContent();
        }).RequireAuthorization();

        app.MapPost("/texts/{id}/encrypt", (HttpContext context, AuthService authService, EncryptedText encryptedText, int id) =>
        {
            var userId = authService.GetUserId(context);
            var text = TextService.GetTextWithMask(userId, id);

            if (text == null) return Results.NotFound();

            if (!string.IsNullOrEmpty(text.Mask)) return Results.Problem("Текст уже зашифрован");

            encryptedText.Content = text.Content;

            var validationResult = EncryptValidation(encryptedText);
            if (validationResult != null) return validationResult;

            try
            {
                var result = new PlayfairCipher().Encrypt(encryptedText.Content, encryptedText.Key);
                TextService.UpdateText(userId, id, result, TextService.GetTextMask(encryptedText.Content));
                return Results.Ok(new
                {
                    message = "Успех",
                    encryptedText = result
                });
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        }).RequireAuthorization();

        app.MapPost("/texts/{id}/decrypt", (HttpContext context, AuthService authService, EncryptedText encryptedText, int id) =>
        {
            var userId = authService.GetUserId(context);
            var text = TextService.GetTextWithMask(userId, id);

            if (text == null) return Results.NotFound();

            if (string.IsNullOrEmpty(text.Mask)) return Results.Problem("Текст не зашифрован");

            encryptedText.Content = text.Content;

            var validationResult = EncryptValidation(encryptedText);
            if (validationResult != null) return validationResult;

            try
            {
                var result = new PlayfairCipher().Decrypt(encryptedText.Content, encryptedText.Key);
                string restoredText = TextService.RestoreTextWithMask(result, text.Mask);

                TextService.UpdateText(userId, id, restoredText, "");

                return Results.Ok(new
                {
                    message = "Успех",
                    decryptedText = restoredText,
                });
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        }).RequireAuthorization();
    }

    private static IResult? EncryptValidation(EncryptedText encryptedText)
    {
        const string pattern = "^[A-Za-zА-Яа-яЁё ]+$";

        if (string.IsNullOrEmpty(encryptedText?.Content) || string.IsNullOrEmpty(encryptedText?.Key))
        {
            return Results.BadRequest(new ProblemDetails
            {
                Status = 400,
                Title = "Некорректный запрос",
                Detail = "поле content и key обязательно для заполнения"
            });
        }

        if (!Regex.IsMatch(encryptedText.Content, pattern) || !Regex.IsMatch(encryptedText.Key, pattern))
        {
            return Results.BadRequest(new ProblemDetails
            {
                Status = 400,
                Title = "Некорректный запрос",
                Detail = "content и key должны быть строками, состоящими только из букв"
            });
        }

        return null;
    }
}