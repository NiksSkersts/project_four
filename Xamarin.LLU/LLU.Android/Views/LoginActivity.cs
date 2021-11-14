using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using LLU.Android.Controllers;
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
            //Assumption is that later on in development, I will add more than one email user.
            //If there's no data - request that user does login.
            var userdata = AccountManager.UserData;
        sec_check:
            if (userdata == null)
            {
                SetContentView(Resource.Layout.Login);
                usernamefield = FindViewById<EditText>(Resource.Id.usernamefield);
                passwordfield = FindViewById<EditText>(Resource.Id.passwordfield);
                loginButton = FindViewById<Button>(Resource.Id.loginButton);
                loginButton.Click += DoLogin;
                base.OnCreate(savedInstanceState);
            }
            else
            // WARNING:  else statement is required because the execution of method continues regardless of StartActivity() being launched. Thus it could attempt to create a user that doesn't even exist yet. NULL ref expected.
            {
                var attempt = AccountManager.Login(userdata);
                if (attempt == false)
                {
                    MessagingManager.ShowConnnectionErrorInternalFailure();
                    userdata = null;
                    goto sec_check;
                }
                else
                {
                    Intent intent = new(Application.Context, typeof(EmailActivity));
                    StartActivity(intent);
                }
            }
            Finish();
        }

        private void DoLogin(object sender, EventArgs e)
        {
            var temp = loginButton.Text;
            loginButton.Text = "{ md_cached spin }";
            //loginButton.Background = new JoanZapata.XamarinIconify.IconDrawable(Application.Context, JoanZapata.XamarinIconify.Fonts.MaterialIcons.md_cached);

            var attempt = AccountManager.Login(usernamefield.Text.ToString(), passwordfield.Text.ToString());
            if (attempt == false)
            {
                MessagingManager.ShowConnnectionError();
                loginButton.Text = temp;
            }
            else
            {
                Intent intent = new(Application.Context, typeof(EmailActivity));
                StartActivity(intent);
                Finish();
            }

        }
    }
}