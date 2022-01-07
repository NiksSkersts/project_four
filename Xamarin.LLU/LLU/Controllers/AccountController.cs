using System;
using Android.Content;
using LLU.Android.LLU.Models;
using LLU.Android.Views;
using LLU.Controllers;
using LLU.Models;

namespace LLU.Android.Controllers;

/// <summary>
///     AccountController is responsible for anything that is related to account management e.g Login, local login account
///     creation.
/// </summary>
internal class AccountController : IController {
    public object ClientAuth(UserData temp, object? client) => (EmailUser) Connect(temp);

    public object Connect(object data) {
        var temp = (UserData) data;
        var user = new EmailUser(temp.Username, temp.Password);
        return user;
    }

    public void Dispose() { }
    public bool Login() => User.UserData != null && Login(User.UserData);

    public bool Login(UserData userData) {
        var status = false;
        if (string.IsNullOrEmpty(userData.Username) || string.IsNullOrEmpty(userData.Password))
            return status;
        try {
            User.EmailUserData = (EmailUser) ClientAuth(userData, null);
            status = true;
        }
        catch (Exception e) {
            status = false;
        }

        return status;
    }

    public static bool LogOut(Context context) {
        RuntimeController.Instance.WipeDatabase();
        RuntimeController.Instance.Dispose();
        User.UserData = null;
        var backToStart = new Intent(context, typeof(LoginActivity));
        context.StartActivity(backToStart);
        return true;
    }
}