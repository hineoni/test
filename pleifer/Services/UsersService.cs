﻿using pleifer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;

namespace pleifer.Services;

public class UsersService : DbService
{
    private readonly SqliteConnection? _db = null;
    private readonly PasswordHasher<object> _passwordHasher;


    public UsersService(SqliteConnection? connection = null) : base("../users.sqlite")
    {
        _db = connection ?? GetConnection();
        _passwordHasher = new PasswordHasher<object>();
    }

    public User? AddUser(string username, string password)
    {
        if (HasUserByUsername(username)) throw new Exception("Пользователь уже существует");

        try
        {
            var hashedPassword = HashPassword(password);

            using var command =
                new SqliteCommand(@"
                    INSERT INTO Users (username, password) VALUES (@username, @password);
                    SELECT last_insert_rowid();
                ", _db);
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@password", hashedPassword);

            var newUserId = (long)command.ExecuteScalar()!;

            return newUserId != 0 ? GetUserById(newUserId) : null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding user: {ex.Message}");
            throw new Exception("Произошла ошибка при добавлении пользователя");
        }
    }

    public User? ValidateUser(string username, string password)
    {
        try
        {
            using var command = new SqliteCommand("SELECT id, username, password FROM Users WHERE username = @username", _db);
            command.Parameters.AddWithValue("@username", username);

            using var reader = command.ExecuteReader();
            if (!reader.Read()) return null;

            var storedHashedPassword = reader.GetString(2);
            if (VerifyPassword(storedHashedPassword, password))
            {
                return new User(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    string.Empty
                );
            }
            return null;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception("Пользователь не найден");
        }
    }

    public User? ChangePassword(string username, string oldPassword, string newPassword)
    {
        try
        {
            using var command = new SqliteCommand("SELECT id, password FROM Users WHERE username = @username", _db);
            command.Parameters.AddWithValue("@username", username);

            using var reader = command.ExecuteReader();
            if (!reader.Read()) return null;

            var user = new User(
                reader.GetInt32(0),
                reader.GetString(1),
                string.Empty
            );

            var storedHashedPassword = reader.GetString(1);
            if (!VerifyPassword(storedHashedPassword, oldPassword)) throw new Exception("Пароли не совпадают");

            var newHashedPassword = HashPassword(newPassword);
            using var updateCommand = new SqliteCommand("UPDATE Users SET password = @newPassword WHERE username = @username", _db);
            updateCommand.Parameters.AddWithValue("@username", username);
            updateCommand.Parameters.AddWithValue("@newPassword", newHashedPassword);

            var rowsAffected = updateCommand.ExecuteNonQuery();
            return rowsAffected > 0
                ? user
                : null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error changing password: {ex.Message}");
            throw new Exception("Произошла ошибка при смене пароля");
        }
    }

    private string HashPassword(string password)
    {
        return _passwordHasher.HashPassword(null, password);
    }

    private bool VerifyPassword(string storedHashedPassword, string enteredPassword)
    {
        var result = _passwordHasher.VerifyHashedPassword(null, storedHashedPassword, enteredPassword);
        return result == PasswordVerificationResult.Success;
    }

    private bool HasUserByUsername(string username)
    {
        try
        {
            using var command = new SqliteCommand("SELECT COUNT(*) FROM Users WHERE username = @username", _db);
            command.Parameters.AddWithValue("@username", username);
            var count = Convert.ToInt32(command.ExecuteScalar());

            return count > 0;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private User? GetUserById(long id)
    {
        try
        {
            using var command = new SqliteCommand("SELECT id, username FROM Users WHERE id = @id",
                _db);
            command.Parameters.AddWithValue("@id", id);

            using var reader = command.ExecuteReader();
            return reader.Read()
                ? new User(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    string.Empty
                    )
                : null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting user by ID: {ex.Message}");
            throw;
        }
    }

    public void AddUserRequestLog(int userId, string requestDetails)
    {
        try
        {
            using var command =
                new SqliteCommand(@"
                    INSERT INTO user_logs (user_id, request, created_at) VALUES (@user_id, @request, @created_at)
                ", _db);
            command.Parameters.AddWithValue("@user_id", userId);
            command.Parameters.AddWithValue("@request", requestDetails);
            command.Parameters.AddWithValue("@created_at", DateTimeOffset.Now.ToUnixTimeSeconds());

            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            throw new Exception("Произошла ошибка при сохранение запроса");
        }
    }

    public List<string> GetUserRequestHistory(int userId)
    {
        var requestHistory = new List<string>();

        try
        {
            using var command =
                new SqliteCommand("SELECT request FROM user_logs WHERE user_id = @user_id ORDER BY created_at DESC", _db);
            command.Parameters.AddWithValue("@user_id", userId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                requestHistory.Add(reader.GetString(0));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            throw new Exception("Произошла ошибка при получении истории запросов пользователя");
        }

        return requestHistory;
    }

    public void DeleteUserRequestLog(int userId)
    {
        try
        {
            using var command =
                new SqliteCommand(@"DELETE FROM user_logs WHERE user_id = @user_id", _db);
            command.Parameters.AddWithValue("@user_id", userId);

            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            throw new Exception("Произошла ошибка при удалении истории запросов пользователя");
        }
    }
}