using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using LLU.Android.Controllers;
using LLU.Models;

namespace LLU.Android.Views;

/// <summary>
///     On creation - activity attempts to find the previous login info from the database and attempts to login using that.
///     Failure returns an error code. Server not available or wrong credentials.
///     On "Server not available", application warns user to try again later, and loads local data meanwhile.
///     On "Wrong credentials", application wipes data in the database and goes through the first login process again.
///     On first login app asks for credentials and attempts login at button press.
/// </summary>
[Activity(Label = "LLU e-pasts", MainLauncher = true)]
public class LoginActivity : Activity {
    private readonly AccountController _loginAttempt = new();

    protected override void OnCreate(Bundle? savedInstanceState) {
        base.OnCreate(savedInstanceState);
        var attempt = _loginAttempt.Login();
        if (attempt) {
            StartEmailActivity();
        }
        else {
            SetContentView(Resource.Layout.LoginActivity);
            _layout = FindViewById<LinearLayout>(Resource.Id.mainLoginLayout)!;
            _loginText = FindViewById<TextView>(Resource.Id.loginText)!;
            _usernameField = FindViewById<EditText>(Resource.Id.usernamefield)!;
            _passwordField = FindViewById<EditText>(Resource.Id.passwordfield)!;
            _loginButton = FindViewById<Button>(Resource.Id.loginButton)!;
            _loginButton.Click += DoLogin;
        }
    }

    private void StartEmailActivity() {
        Intent intent = new(Application.Context, typeof(EmailActivity));
        StartActivity(intent);
        Finish();
    }

    private void DoLogin(object sender, EventArgs e) {
        if (_usernameField.Text != null && _passwordField.Text != null) {
            var attempt = _loginAttempt.Login(new UserData {
                Username = _usernameField.Text, Password = _passwordField.Text
            });
            switch (attempt) {
                case true:
                    StartEmailActivity();
                    break;
                case false:
                    MessagingController.ShowConnectionError();
                    break;
            }
        }
    }

#region Declaration

    private EditText _usernameField = null!;
    private EditText _passwordField = null!;
    private Button _loginButton = null!;
    private LinearLayout _layout = null!;
    private TextView _loginText = null!;

#endregion
}