using System;
using System.IO;
using System.Linq;
using System.Text;
using Android.App;
using LLU.Android.Controllers;
using LLU.Android.LLU.Models;
using Newtonsoft.Json;

namespace LLU.Models;

internal abstract class User : IDisposable {
    /// <summary>
    ///     Getter for database connection.
    ///     Backend uses SQLITE database that is integrated within the application.
    ///     <para>Disconnect after use!</para>
    /// </summary>
    protected static DatabaseController Database =>
        DatabaseController.DbController;

    protected static Secrets Secrets {
        get {
            var assets = Application.Context.Assets;
            using var streamReader = new StreamReader(assets.Open("secrets"));
            var stream = streamReader.ReadToEnd();
            var secrets = JsonConvert.DeserializeObject<Secrets>(stream);
            return secrets;
        }
    }
    /// <summary>
    ///     Normally a connection to server would fail if you supply a bad username or password,
    ///     but the goal is to make as little as possible connections to the server to preserve cycles both on server and on
    ///     smartphone.
    ///     So let's add a safety check that makes sure we don't make an authentication query with useless credentials.
    /// </summary>
    protected internal static UserData? UserData {
        get {
            UserData user = new UserData();
            var path = DataController.GetFilePath("user");
            if (File.Exists(path)) {
                string[] lines = File.ReadAllLines(path);
                user.Username = lines[0];
                user.Password = lines[1];
            }
            if ((!string.IsNullOrEmpty(user.Username) 
                 || !string.IsNullOrEmpty(user.Password)))
                return user;
            Database.WipeDatabase();
            return null;
        }
        set {
            var path = DataController.GetFilePath("user");
            string[] lines = File.ReadAllLines(path);
            if (value == null) {
                lines[0] = string.Empty;
                lines[1] = string.Empty;
            }
            else {
                lines[0] = value.Username;
                lines[1] = value.Password;
            }
            File.WriteAllLines(path, lines);
            }
    }
    /// <summary>
    ///     Stores temporary data that gets deleted when the application terminates.
    ///     WARNING: all permanent data gets stored within the database!
    /// </summary>
    public static EmailUser? EmailUserData { get; set; }

    public void Dispose() { }
}