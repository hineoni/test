﻿﻿namespace pleifer.Models;

public class Text(int id, string content, string userId)
{
    public int Id { get; } = id;
    public string Content { get; } = content;
    public string UserId { get; } = userId;
}

public class TextWithMask(string content, string mask)
{
    public string Content { get; } = content;
    public string Mask { get; } = mask;
}

public class EncryptedText(string content, string key)
{
    public string Content { get; set; } = content;
    public string Key { get; } = key;
}