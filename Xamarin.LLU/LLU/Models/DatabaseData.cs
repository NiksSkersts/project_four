using System;
using System.Collections.Generic;
using System.Linq;
using LLU.Android.Models;
using SQLite;

namespace LLU.Models;

/// <summary>
///     Custom class that models the internal database structure.
/// </summary>
public class DatabaseData {
    [PrimaryKey] public string Id { get; set; }
    public string UniqueId { get; set; }
    public string Folder { get; set; }
    public string From { get; set; }
    public string To { get; set; }
    public string Subject { get; set; }
    public long Time { get; set; }
    public string Body { get; set; }
    public bool IsHtmlBody { get; set; }
    public bool DeleteFlag { get; set; }
    public bool NewFlag { get; set; }
    public bool PriorityFlag { get; set; }
}

/// <summary>
///     Defines the data that's associated with the user.
/// </summary>
public class UserData {
    public string Username { get; set; }
    public string Password { get; set; }
}