﻿﻿using pleifer.Models;
using Microsoft.Data.Sqlite;

namespace pleifer.Services;

public class TextService : DbService
{
    private readonly SqliteConnection? _db = null;

    public TextService(SqliteConnection? connection = null) : base("../users.sqlite")
    {
        _db = connection ?? GetConnection();
    }

    public void AddText(Text text, int userId)
    {
        try
        {
            using var command =
                new SqliteCommand(@"
                    INSERT INTO texts (user_id, content) VALUES (@user_id, @content)
                ", _db);
            command.Parameters.AddWithValue("@user_id", userId);
            command.Parameters.AddWithValue("@content", text.Content);

            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            throw new Exception("Произошла ошибка при добавлении текста");
        }
    }

    public bool UpdateText(int userId, int id, string newText, string? mask = null)
    {
        try
        {
            using var command =
                new SqliteCommand(@"
                    UPDATE texts
                    SET content = @content, mask = @mask
                    WHERE user_id = @user_id AND id = @id
                ", _db);
            command.Parameters.AddWithValue("@user_id", userId);
            command.Parameters.AddWithValue("@content", newText);
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@mask", mask ?? string.Empty);

            var rowsAffected = command.ExecuteNonQuery();
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            throw new Exception("Произошла ошибка при изменении текста");
        }
    }

    public bool DeleteText(int userId, int id)
    {
        try
        {
            using var command =
                new SqliteCommand(@"
                    DELETE FROM texts WHERE user_id = @user_id AND id = @id
                ", _db);
            command.Parameters.AddWithValue("@user_id", userId);
            command.Parameters.AddWithValue("@id", id);

            var rowsAffected = command.ExecuteNonQuery();
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            throw new Exception("Произошла ошибка при удалении текста");
        }
    }

    public string? GetText(int userId, int id)
    {
        try
        {
            using var command =
                new SqliteCommand(@"
                    SELECT content FROM texts WHERE user_id = @user_id AND id = @id
                ", _db);
            command.Parameters.AddWithValue("@user_id", userId);
            command.Parameters.AddWithValue("@id", id);

            using var reader = command.ExecuteReader();

            return reader.Read() ? reader.GetString(0) : null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            throw new Exception("Произошла ошибка при получении текста");
        }
    }

    public TextWithMask? GetTextWithMask(int userId, int id)
    {
        try
        {
            using var command =
                new SqliteCommand(@"
                    SELECT content, mask FROM texts WHERE user_id = @user_id AND id = @id
                ", _db);
            command.Parameters.AddWithValue("@user_id", userId);
            command.Parameters.AddWithValue("@id", id);

            using var reader = command.ExecuteReader();

            return reader.Read() ? new TextWithMask
                (
                    reader.GetString(0),
                    reader.IsDBNull(1) ? string.Empty : reader.GetString(1)
                )
            : null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            throw new Exception("Произошла ошибка при получении текста");
        }
    }

    public List<Text> GetAllTexts(int userId)
    {
        var texts = new List<Text>();

        try
        {
            using var command =
                new SqliteCommand(@"
                    SELECT id, content, user_id FROM texts WHERE user_id = @user_id ORDER BY id DESC
                ", _db);
            command.Parameters.AddWithValue("@user_id", userId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                texts.Add(new Text(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2)
                ));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            throw new Exception("Произошла ошибка при получении всех текстов");
        }

        return texts;
    }

    public string GetTextMask(string input)
    {
        string result = "";
        foreach (char c in input)
        {
            result += char.IsUpper(c) ? '1' : char.IsWhiteSpace(c) ? '2' : '0';
        }
        return result;
    }

    public string RestoreTextWithMask(string decryptedText, string mask)
    {
        if (decryptedText.Length != mask.Replace("2", "").Length)
        {
            throw new Exception("Длина маски не совпадает с длиной расшифрованного текста.");
        }

        char[] restoredChars = new char[mask.Length];
        int textIndex = 0;

        for (int i = 0; i < mask.Length; i++)
        {
            char maskChar = mask[i];

            if (maskChar == '1')
            {
                restoredChars[i] = char.ToUpper(decryptedText[textIndex]);
                textIndex++;
            }
            else if (maskChar == '2')
            {
                restoredChars[i] = ' ';
            }
            else
            {
                restoredChars[i] = char.ToLower(decryptedText[textIndex]);
                textIndex++;
            }
        }

        return new string(restoredChars);
    }
}