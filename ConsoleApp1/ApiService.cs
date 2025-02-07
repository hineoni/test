﻿﻿using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ConsoleApp1;

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(string baseUrl)
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    public async Task<bool> RegisterUser(User user)
    {
        var jsonContent = JsonSerializer.Serialize(user);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/signup", content);

        if (!response.IsSuccessStatusCode)
        {
            LogError("Registration failed.");
            return false;
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonSerializer.Deserialize<AuthResponse>(responseContent);
        SetAuthorizationHeader(jsonResponse?.Token);

        LogSuccess("User registered successfully.");

        return true;
    }

    public async Task<bool> LoginUser(User user)
    {
        var jsonContent = JsonSerializer.Serialize(user);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("/login", content);

        if (!response.IsSuccessStatusCode)
        {
            LogError("Login failed.");
            return false;
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonSerializer.Deserialize<AuthResponse>(responseContent);
        SetAuthorizationHeader(jsonResponse?.Token);

        LogSuccess("User logged in successfully.");

        return true;
    }

    public Task UnloginUser()
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;
        return Task.CompletedTask;
    }

    public async Task<bool> ChangePassword(ChangePassword changePassword)
    {
        var jsonContent = JsonSerializer.Serialize(changePassword);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        var response = await _httpClient.PatchAsync("/change-password", content);

        if (!response.IsSuccessStatusCode)
        {
            LogError("Password change failed.");
            return false;
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonSerializer.Deserialize<AuthResponse>(responseContent);
        SetAuthorizationHeader(jsonResponse?.Token);

        LogSuccess("Password changed successfully.");

        return true;
    }

    private void SetAuthorizationHeader(string? token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task GetUserRequests()
    {
        var response = await _httpClient.GetAsync("/user-requests");
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonSerializer.Deserialize<DefaultArrayJsonResponse>(responseContent);

            LogSuccess($"User requests:\n{string.Join("\n", jsonResponse.Data)}", false);
        }
        else
        {
            LogError("Failed to fetch user requests.");
        }
    }

    public async Task DeleteUserRequests()
    {
        var response = await _httpClient.DeleteAsync("/user-requests");
        if (response.IsSuccessStatusCode)
        {
            LogSuccess("User requests deleted successfully.");
        }
        else
        {
            LogError("Failed to delete user requests.");
        }
    }

    public async Task GetAllTexts(bool idOnly = false)
    {
        var response = await _httpClient.GetAsync("/texts");
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonSerializer.Deserialize<AllTextsResponse>(responseContent);

            if (jsonResponse.Data.Length > 0)
            {
                var resultString = "";
                foreach (var item in jsonResponse.Data)
                {
                    resultString += idOnly ? $"{item.Id}\n" : $"[ID: {item.Id}] {item.Content}\n";
                }

                LogSuccess($"Texts{(idOnly ? " IDs" : "")}:\n{resultString}", false);
            }
            else
            {
                LogError("Тексты не найдены. 😞");
            }
        }
        else
        {
            LogError("Failed to fetch texts.");
        }
    }

    public async Task GetTextById(int id)
    {
        var response = await _httpClient.GetAsync($"/texts/{id}");
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonSerializer.Deserialize<DefaultJsonResponse>(responseContent);

            LogSuccess( $"Text: {jsonResponse?.Data}", false);
        }
        else
        {
            LogError("Failed to fetch text.");
        }
    }

    public async Task UpdateText(int id, Text text)
    {
        var jsonContent = JsonSerializer.Serialize(text);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        var response = await _httpClient.PatchAsync($"/texts/{id}", content);

        if (response.IsSuccessStatusCode)
        {
            LogSuccess("Text updated successfully.");
        }
        else
        {
            LogError("Failed to update text.");
        }
    }

    public async Task DeleteText(int id)
    {
        var response = await _httpClient.DeleteAsync($"/texts/{id}");
        if (response.IsSuccessStatusCode)
        {
            LogSuccess("Text deleted successfully.");
        }
        else
        {
            LogError("Failed to delete text.");
        }
    }

    public async Task AddText(Text text)
    {
        var jsonContent = JsonSerializer.Serialize(text);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("/texts", content);
        if (response.IsSuccessStatusCode)
        {
            LogSuccess("Text posted successfully.");
        }
        else
        {
            LogError("Text posting failed.");
        }
    }

    public async Task EncryptText(int id, string key)
    {
        var jsonContent = JsonSerializer.Serialize(new { Key = key });
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"/texts/{id}/encrypt", content);
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonSerializer.Deserialize<EncryptedTextResponse>(responseContent);

            LogSuccess( $"Encrypted Text: {jsonResponse?.EncryptedText}", false);
        }
        else
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonSerializer.Deserialize<EncryptedTextFailureResponse>(responseContent);

            LogError(jsonResponse?.Detail);
        }
    }

    public async Task DecryptText(int id, string key)
    {
        var jsonContent = JsonSerializer.Serialize(new { Key = key });
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"/texts/{id}/decrypt", content);
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonSerializer.Deserialize<DecryptedTextResponse>(responseContent);

            LogSuccess( $"Decrypted Text: {jsonResponse?.DecryptedText}", false);
        }
        else
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonSerializer.Deserialize<EncryptedTextFailureResponse>(responseContent);
            
            LogError(jsonResponse?.Detail ?? "Decryption failed.");
        }
    }

    private static void LogSuccess(string? message = null, bool? prefix = true)
    {
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine(prefix == true ? $"Success: {message}" : message);
        Console.ResetColor();
    }

    private static void LogError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {message}");
        Console.ResetColor();
    }
}