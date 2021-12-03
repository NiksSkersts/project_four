using SQLite;
using SQLiteNetExtensions.Attributes;
using System;

namespace LLU.Models
{
    public class DatabaseData
    {
        [PrimaryKey]
        public string Id { get; set; }
        [ForeignKey(typeof(UserData))]
        public string UserID { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Subject { get; set; }
        public DateTimeOffset Time { get; set; }
        public string Body { get; set; }
        public bool IsHtmlBody { get; set; }
        public bool DeleteFlag { get; set; }
    }
    public class UserData
    {
        [PrimaryKey,OneToMany]
        public string UserID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
