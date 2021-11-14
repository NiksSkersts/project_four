using SQLite;
using System;

namespace LLU.Models
{
    public class DatabaseData
    {
        [PrimaryKey]
        public string Id { get; set; }
        public string UserID { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Subject { get; set; }
        public DateTimeOffset Time { get; set; }
        public string Body { get; set; }
        public bool DeleteFlag { get; set; }

    }
    public class UserData
    {
        [PrimaryKey]
        public string UserID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
    public class EmailIDs
    {
        public string UserID { get; set; }
        [PrimaryKey]
        public string MessageID { get; set; }
    }
    public class EmailHistory
    {
        [PrimaryKey]
        public string UserID { get; set; }
        public int Emails { get; set; }
    }
}
