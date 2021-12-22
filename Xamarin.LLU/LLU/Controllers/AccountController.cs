using LLU.Android.LLU.Models;
using LLU.Controllers;
using LLU.Models;
using static LLU.Models.User;

namespace LLU.Android.Controllers;

/// <summary>
///     AccountController is responsible for anything that is related to account management e.g Login, local login account
///     creation.
/// </summary>
internal class AccountController : IController {
    /// <summary>
    ///     Normally a connection to server would fail if you supply a bad username or password,
    ///     but the goal is to make as little as possible connections to the server to preserve cycles both on server and on
    ///     smartphone.
    ///     So let's add a safety check that makes sure we don't make an authentication query with useless credentials.
    /// </summary>
    private static UserData? UserData {
        get {
            var user = Database.GetUserData().Result;
            if (user != null && (!string.IsNullOrEmpty(user.Username) || !string.IsNullOrEmpty(user.Password)))
                return user;
            return null;
        }
    }

    public bool ClientAuth(UserData temp) =>
        Connect(temp) is 0 && EmailUserData!.IsClientAuthenticated;

    public byte Connect(object data) {
        var temp = (UserData) data;
        return Connect(temp.Username, temp.Password);
    }

    public void Dispose() { }

    private static byte Connect(string username, string password) {
        EmailUserData = new EmailUser(username, password);
        return EmailUserData.IsClientConnected ? (byte) 0 : (byte) 1;
    }

    public bool Login() => UserData != null && Login(UserData.Username, UserData.Password);

    public bool Login(string username, string password) {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            return false;
        UserData temp = new() {
            Username = username,
            Password = password
        };
        return ClientAuth(temp);
    }
}