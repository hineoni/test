using pleifer.Models;
using pleifer.Services;
using Microsoft.Data.Sqlite;

namespace TestProject1;

public class TextsServiceTests
{
    private TextService _textService;
    private SqliteConnection _connection;

    public TextsServiceTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        InitializeDatabase();
        _textService = new TextService(_connection);

        var username = "testUser";
        var password = "testPassword";

        new UsersService(_connection).AddUser(username, password);
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
        ";
        command.ExecuteNonQuery();
    }

    [Fact]
    public void AddText_ShouldInsertText()
    {
        var text = new Text(0, "Hello World", "1");
        int userId = 1;

        _textService.AddText(text, userId);

        string? storedText = _textService.GetText(userId, 1);
        Assert.Equal("Hello World", storedText);
    }

    [Fact]
    public void UpdateText_ShouldModifyExistingText()
    {
        int userId = 1;
        var text = new Text(0, "Old Text", "1");
        _textService.AddText(text, userId);

        bool updated = _textService.UpdateText(userId, 1, "New Text");
        string? storedText = _textService.GetText(userId, 1);

        Assert.True(updated);
        Assert.Equal("New Text", storedText);
    }

    [Fact]
    public void DeleteText_ShouldRemoveText()
    {
        int userId = 1;
        var text = new Text(0, "Some Text", "1");
        _textService.AddText(text, userId);

        bool deleted = _textService.DeleteText(userId, 1);
        string? storedText = _textService.GetText(userId, 1);

        Assert.True(deleted);
        Assert.Null(storedText);
    }

    [Fact]
    public void GetAllTexts_ShouldReturnList()
    {
        int userId = 1;
        _textService.AddText(new Text(0, "Text 1", "1"), userId);
        _textService.AddText(new Text(0, "Text 2", "1"), userId);

        var texts = _textService.GetAllTexts(userId);

        Assert.Equal(2, texts.Count);
        Assert.Contains(texts, t => t.Content == "Text 1");
        Assert.Contains(texts, t => t.Content == "Text 2");
    }

    [Fact]
    public void GetTextMask_ShouldGenerateCorrectMask()
    {
        string mask = _textService.GetTextMask("Hello World");

        Assert.Equal("10000210000", mask);
    }

    [Fact]
    public void RestoreTextWithMask_ShouldReconstructText()
    {
        string restored = _textService.RestoreTextWithMask("helloworld", "10000210000");

        Assert.Equal("Hello World", restored);
    }
}