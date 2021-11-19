using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using LLU.Android.Controllers;
using LLU.Models;
using System;

namespace LLU.Android.Views
{
    [Activity(Label = "Login", MainLauncher = true)]
    public class LoginActivity : Activity
    {

        EditText usernamefield;
        EditText passwordfield;
        Button loginButton;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // On creation - activity attempts to find the previous login info from the database and attempts to login using that.
            // Failure returns an error code. Server not available or wrong credentials.
            // On "Server not available", application warns user to try again later, and loads local data meanwhile.
            // On "Wrong credentials", application wipes data in the database and goes through the first login process again.
            // On first login app asks for crdentials and attempts login at button press.
            var userdata = AccountController.UserData;
        sec_check:
            if (userdata == null)
            {
                SetContentView(Resource.Layout.Login);
                usernamefield = FindViewById<EditText>(Resource.Id.usernamefield);
                passwordfield = FindViewById<EditText>(Resource.Id.passwordfield);
                loginButton = FindViewById<Button>(Resource.Id.loginButton);
                loginButton.Click += DoLogin;
                
            }
            else
            {
                var attempt = AccountController.Login(userdata);
                if (attempt != 0)
                {
                    if (attempt == 1)
                    {
                        MessagingController.ShowConnnectionError();
                        StartEmailActivity();
                    }
                    if (attempt == 2)
                    {
                        MessagingController.AuthentificationError();
                        userdata = null;
                        User.Database.WipeDatabase();
                        goto sec_check;
                    }
                }
                else
                {
                    StartEmailActivity();
                }
            }
        }
        private void StartEmailActivity()
        {
            Intent intent = new(Application.Context, typeof(EmailActivity));
            StartActivity(intent);
            Finish();
        }

        private void DoLogin(object sender, EventArgs e)
        {
            var temp = loginButton.Text;
            loginButton.Text = "{ fa-cog spin }";
            //loginButton.Background = new JoanZapata.XamarinIconify.IconDrawable(Application.Context, JoanZapata.XamarinIconify.Fonts.MaterialIcons.md_cached);

            var attempt = AccountController.Login(usernamefield.Text.ToString(), passwordfield.Text.ToString());
            if (attempt == 0)
            {
                StartEmailActivity();
            }
            if (attempt == 1)
                MessagingController.ShowConnnectionError();
            else if( attempt == 2)
                MessagingController.AuthentificationError();
            loginButton.Text = temp;

        }
    }
}