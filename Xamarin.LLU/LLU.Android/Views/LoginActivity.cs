using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using LLU.Android.Controllers;
using LLU.Models;
using System;
using Android.Views;
using Xamarin.Essentials;

namespace LLU.Android.Views
{
    [Activity(Label = "LLU e-pasts", MainLauncher = true)]
    public class LoginActivity : Activity
    {
        #region Declaration
        EditText usernamefield;
        EditText passwordfield;
        Button loginButton;
        TextView loading;
        LinearLayout layout;
        TextView loginText;
        #endregion

        // On creation - activity attempts to find the previous login info from the database and attempts to login using that.
        // Failure returns an error code. Server not available or wrong credentials.
        // On "Server not available", application warns user to try again later, and loads local data meanwhile.
        // On "Wrong credentials", application wipes data in the database and goes through the first login process again.
        // On first login app asks for crdentials and attempts login at button press.

        protected override void OnCreate(Bundle savedInstanceState)
        {

            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Login);
            layout = FindViewById<LinearLayout>(Resource.Id.mainLoginLayout);
            loading = new(this);
            loading.TextAlignment = TextAlignment.Center;
            loading.SetHeight((int)DeviceDisplay.MainDisplayInfo.Height);
            loading.SetWidth((int)DeviceDisplay.MainDisplayInfo.Width);
            loading.Text = "{md_cached spin}";
            layout.AddView(loading);

            var userdata = AccountController.UserData;
        sec_check:
            if (userdata == null)
            {
                layout.RemoveView(loading);
                loginText = FindViewById<TextView>(Resource.Id.loginText);
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
                    }
                    if (attempt == 2)
                    {
                        MessagingController.AuthentificationError();
                        userdata = null;
                        User.Database.WipeDatabase();
                        goto sec_check;
                    }
                }
                StartEmailActivity();
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
            layout.RemoveAllViews();
            layout.AddView(loading);
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
            layout.RemoveAllViews();
            layout.AddView(loginText);
            layout.AddView(usernamefield);
            layout.AddView(passwordfield);
            layout.AddView(loginButton);

        }
    }
}