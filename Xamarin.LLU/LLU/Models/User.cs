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
        static DatabaseController database;

        //Getter for database connection. if null  = create a new connection
        //Backend uses SQLITE database that is integrated within the application
        public static DatabaseController Database
        {
            get
            {
                if (database == null)
                {
                    database = new DatabaseController(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "data"));
                }
                return database;
            }
        }
        protected static Secrets Secrets
        {
            get
            {
                AssetManager assets = Application.Context.Assets;
                using var streamReader = new StreamReader(assets.Open("secrets"));
                var stream = streamReader.ReadToEnd();
                var secrets = JsonConvert.DeserializeObject<Secrets>(stream);
                return secrets;
            }
        }

        protected UserData UserData { get; set; }

        //Stores temporary data that gets deleted when the application terminates.
        //WARNING: all permanent data gets stored within the database!
        public static EmailUser EmailUserData { get; set; }
    }
}
