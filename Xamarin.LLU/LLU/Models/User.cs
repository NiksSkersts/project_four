using Android.App;
using Android.Content.Res;
using LLU.Android.Controllers;
using LLU.Android.LLU.Models;
using Newtonsoft.Json;
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
        private static Secrets secret;
        protected static Secrets Secrets
        {
            get
            {
                if(secret !=null)
                    return secret;

                AssetManager assets = Application.Context.Assets;
                using var streamReader = new StreamReader(assets.Open("secrets"));
                var stream = streamReader.ReadToEnd();
                var secrets = JsonConvert.DeserializeObject<Secrets>(stream);
                secret = secrets;
                return secrets;
            }
        }
        protected string Host { get; set; }
        protected int Port { get; set; }
        protected string Userid { get; set; }
        protected string Username { get; set; }
        protected string Password { get; set; }

        //Stores temporary data that gets deleted when the application terminates.
        //WARNING: all permanent data gets stored within the database!
        public static EmailUser EmailUserData { get; set; }
    }
}
