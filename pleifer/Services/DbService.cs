using Microsoft.Data.Sqlite;

namespace pleifer.Services;

public class DbService
{
    private readonly SqliteConnection? _db = null;

    protected DbService(string? path)
    {
        if (path == null) return;

        try
        {
            _db = new SqliteConnection($"Data Source={path}");
            _db.Open();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public void Disconnect()
    {
        _db?.Close();
    }

    protected SqliteConnection? GetConnection()
    {
        return _db;
    }
}