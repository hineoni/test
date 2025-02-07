namespace pleifer.Models;

public class User(int id, string username, string password)
{
    public int Id { get; } = id;
    public string Username { get; } = username;
    public string Password { get; } = password;
}

public class ChangePassword(string username, string oldPassword, string newPassword)
{
    public string Username { get; } = username;
    public string OldPassword { get; } = oldPassword;
    public string NewPassword { get; } = newPassword;
}
