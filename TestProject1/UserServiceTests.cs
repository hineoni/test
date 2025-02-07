using pleifer.Services;
using Microsoft.Data.Sqlite;

namespace TestProject1;

public class UsersServiceTests
{
    private UsersService _service;
    private SqliteConnection _connection;

    public UsersServiceTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        InitializeDatabase();
        _service = new UsersService(_connection);
    }

    private void InitializeDatabase()
    {
        using var command = _connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE users (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                username TEXT UNIQUE NOT NULL,
                password TEXT NOT NULL
            );
            CREATE TABLE texts (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                content TEXT,
                user_id INTEGER REFERENCES users,
                mask TEXT
            );
            CREATE TABLE user_logs (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                request TEXT,
                user_id INTEGER REFERENCES users,
                created_at INTEGER NOT NULL
            );
        ";
        command.ExecuteNonQuery();
    }

    [Fact]
    public void AddUser_ShouldAddUser_WhenUserDoesNotExist()
    {
        var username = "testUser";
        var password = "testPassword";

        var user = _service.AddUser(username, password);

        Assert.NotNull(user);
        Assert.Equal(username, user.Username);
    }

    [Fact]
    public void AddUser_ShouldThrowException_WhenUserAlreadyExists()
    {
        var username = "existingUser";
        var password = "password123";
        _service.AddUser(username, password);

        Assert.Throws<Exception>(() => _service.AddUser(username, password));
    }

    [Fact]
    public void ValidateUser_ShouldReturnUser_WhenCredentialsAreCorrect()
    {
        var username = "validUser";
        var password = "validPass";
        _service.AddUser(username, password);

        var user = _service.ValidateUser(username, password);

        Assert.NotNull(user);
        Assert.Equal(username, user.Username);
    }

    [Fact]
    public void ValidateUser_ShouldReturnNull_WhenPasswordIsIncorrect()
    {
        var username = "wrongPassUser";
        var password = "correctPass";
        _service.AddUser(username, password);

        var user = _service.ValidateUser(username, "wrongPass");

        Assert.Null(user);
    }

    [Fact]
    public void ChangePassword_ShouldChangePassword_WhenOldPasswordIsCorrect()
    {
        var username = "changePassUser";
        var oldPassword = "oldPass";
        var newPassword = "newPass";
        _service.AddUser(username, oldPassword);

        var user = _service.ChangePassword(username, oldPassword, newPassword);

        Assert.NotNull(user);
        var validatedUser = _service.ValidateUser(username, newPassword);
        Assert.NotNull(validatedUser);
    }

    [Fact]
    public void ChangePassword_ShouldThrowException_WhenOldPasswordIsIncorrect()
    {
        var username = "invalidChangeUser";
        var oldPassword = "oldPass";
        var newPassword = "newPass";
        _service.AddUser(username, oldPassword);

        Assert.Throws<Exception>(() => _service.ChangePassword(username, "wrongOldPass", newPassword));
    }

    [Fact]
    public void AddUserRequestLog_ShouldNotThrowException()
    {
        var username = "testUser";
        var password = "testPassword";

        var user = _service.AddUser(username, password);
        var requestDetails = "Sample request log";

        var exception = Record.Exception(() => _service.AddUserRequestLog(user.Id, requestDetails));

        Assert.Null(exception);
    }

    [Fact]
    public void GetUserRequestHistory_ShouldReturnLogs()
    {
        var username = "testUser";
        var password = "testPassword";

        var user = _service.AddUser(username, password);

        var request1 = "Request 1";
        var request2 = "Request 2";
        _service.AddUserRequestLog(user.Id, request1);
        _service.AddUserRequestLog(user.Id, request2);

        List<string> history = _service.GetUserRequestHistory(user.Id);

        Assert.Contains(request1, history);
        Assert.Contains(request2, history);
    }

    [Fact]
    public void DeleteUserRequestLog_ShouldRemoveLogs()
    {
        var username = "testUser";
        var password = "testPassword";

        var user = _service.AddUser(username, password);

        var request = "Request to be deleted";
        _service.AddUserRequestLog(user.Id, request);
        _service.DeleteUserRequestLog(user.Id);

        List<string> history = _service.GetUserRequestHistory(user.Id);

        Assert.Empty(history);
    }
}