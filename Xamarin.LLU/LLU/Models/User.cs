using System;
using System.IO;
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
    public static DatabaseController Database =>
        new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "data"));

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
    ///     Overrides any value that is currently assigned to the variables. Username is created as "protected" and its value
    ///     should not be allowed to be viewed in outside classes.
    ///     Setting a new value is fine as long as it doesn't leak the original value.
    /// </summary>
    /// <returns></returns>
    protected string Username { get; set; }

    /// <summary>
    ///     Overrides any value that is currently assigned to the variables. Password is created as "protected" and its value
    ///     should not be allowed to be viewed in outside classes.
    ///     Setting a new value is fine as long as it doesn't leak the original value.
    /// </summary>
    /// <returns></returns>
    protected string Password { get; set; }


    /// <summary>
    ///     Stores temporary data that gets deleted when the application terminates.
    ///     WARNING: all permanent data gets stored within the database!
    /// </summary>
    public static EmailUser? EmailUserData { get; set; }

    public void Dispose() { }
}