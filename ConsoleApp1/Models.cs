using System.Text.Json.Serialization;

namespace ConsoleApp1;

public class User
{
    [JsonPropertyName("username")]
    public string Username { get; set; }
    [JsonPropertyName("password")]
    public string Password { get; set; }
}

public class ChangePassword
{
    [JsonPropertyName("username")]
    public string Username { get; set; }
    [JsonPropertyName("oldPassword")]
    public string OldPassword { get; set; }
    [JsonPropertyName("newPassword")]
    public string NewPassword { get; set; }
}

public class Text
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("content")]
    public string Content { get; set; }
    [JsonPropertyName("userId")]
    public string UserId { get; set; }
}

public class EncryptedText
{
    [JsonPropertyName("content")]
    public string Content { get; set; }
    [JsonPropertyName("key")]
    public string Key { get; set; }
}

public class EncryptedTextResponse
{
    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("encryptedText")]
    public string EncryptedText { get; set; }
}

public class EncryptedTextFailureResponse
{
    [JsonPropertyName("detail")]
    public string Detail { get; set; }
}

public class DecryptedTextResponse
{
    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("decryptedText")]
    public string DecryptedText { get; set; }
}

public class AuthResponse
{
    [JsonPropertyName("token")]
    public string Token { get; set; }
}

public class DefaultJsonResponse
{
    [JsonPropertyName("data")]
    public string Data { get; set; }
}

public class DefaultArrayJsonResponse
{
    [JsonPropertyName("data")]
    public string[] Data { get; set; }
}

public class AllTextsResponse
{
    [JsonPropertyName("data")]
    public Text[] Data { get; set; }
}