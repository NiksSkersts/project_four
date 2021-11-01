using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using LLU.Android.LLU.Models;
using System;

namespace LLU.Android.Views
{
    [Activity(Label = "Login")]
    public class Login : Activity
    {
        
        EditText usernamefield;
        EditText passwordfield;
        Button loginButton;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            SetContentView(Resource.Layout.Login);
            usernamefield = FindViewById<EditText>(Resource.Id.usernamefield);
            passwordfield = FindViewById<EditText>(Resource.Id.passwordfield);
            loginButton = FindViewById<Button>(Resource.Id.loginButton);
            loginButton.Click += DoLogin;
            base.OnCreate(savedInstanceState);
            // Create your application here
        }

        private void DoLogin(object sender, EventArgs args)
        {
            Intent intent = new(Application.Context,typeof(EmailActivity));
            LoginPart();
            StartActivity(intent);
            Finish();
        }
        private void LoginPart()
        {
            EmailUser user = new(usernamefield.Text.ToLower(), passwordfield.Text);
            MainActivity.EmailUserData = user;
            //string folder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            // string path = Path.Combine(folder, "login");
            //temp
            
        }
    }
}