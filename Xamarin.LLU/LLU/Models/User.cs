using LLU.Android.Controllers;
using LLU.Android.LLU.Models;
using System;
using System.IO;

namespace LLU.Models
{
    public abstract class User
    {
        static DatabaseManagement database;

        //Getter for database connection. if null  = create a new connection
        //Backend uses SQLITE database that is integrated within the application
        public static DatabaseManagement Database
        {
            get
            {
                if (database == null)
                {
                    database = new DatabaseManagement(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "data"));
                }
                return database;
            }
        }
        //todo move out data that could be interpreted as secret
        //DISCLAIMER: this data is publicly available for everyone to see.
        protected string Host => "mail.llu.lv";
        protected int Port => 993;
        //END OF DISCLAIMER
        protected string Userid { get; set; }
        protected string Username { get; set; }
        protected string Password { get; set; }

        //Stores temporary data that gets deleted when the application terminates.
        //WARNING: all permanent data gets stored within the database!
        public static EmailUser EmailUserData { get; set; }
    }
}
