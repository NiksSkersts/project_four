using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using LLU.Android.Controllers;

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
    private readonly AccountController LoginAttempt = new();

    protected override void OnCreate(Bundle? savedInstanceState) {
        var userdata = LoginAttempt.Login();
        if (userdata) StartEmailActivity();
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.LoginActivity);
        _layout = FindViewById<LinearLayout>(Resource.Id.mainLoginLayout)!;
        _loginText = FindViewById<TextView>(Resource.Id.loginText)!;
        _usernamefield = FindViewById<EditText>(Resource.Id.usernamefield)!;
        _passwordfield = FindViewById<EditText>(Resource.Id.passwordfield)!;
        _loginButton = FindViewById<Button>(Resource.Id.loginButton)!;
        _loginButton.Click += DoLogin;
    }

    private void StartEmailActivity() {
        Intent intent = new(Application.Context, typeof(EmailActivity));
        StartActivity(intent);
        Finish();
    }

    private void DoLogin(object sender, EventArgs e) {
        _layout.RemoveAllViews();
        _layout.AddView(_loading);
        var temp = _loginButton.Text;
        _loginButton.Text = "{ fa-cog spin }";

        if (_usernamefield.Text != null && _passwordfield.Text != null) {
            var attempt = LoginAttempt.Login(_usernamefield.Text, _passwordfield.Text);
            switch (attempt) {
                case true:
                    StartEmailActivity();
                    break;
                case false:
                    MessagingController.ShowConnectionError();
                    break;
            }
        }

        _loginButton.Text = temp;
        _layout.RemoveAllViews();
        _layout.AddView(_loginText);
        _layout.AddView(_usernamefield);
        _layout.AddView(_passwordfield);
        _layout.AddView(_loginButton);
    }

    #region Declaration

    private EditText _usernamefield;
    private EditText _passwordfield;
    private Button _loginButton;
    private TextView _loading;
    private LinearLayout _layout;
    private TextView _loginText;

    #endregion
}