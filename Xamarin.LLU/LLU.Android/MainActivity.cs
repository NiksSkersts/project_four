using Android.App;
using Android.OS;
using LLU.Android.Views;
using LLU.Android.Controllers;
using System.IO;
using LLU.Android.LLU.Models;
using System.Collections.Generic;
using LLU.Models;
using Android.Content;
using Android.Content.PM;
using JoanZapata.XamarinIconify;
using JoanZapata.XamarinIconify.Fonts;

#nullable enable
namespace LLU.Android
{
    [Activity(Label = "LLU", MainLauncher = true)]
    public class MainActivity : Activity
    {
        static DatabaseManagement database;

        //Getter for database connection. if null  = create a new connection
        //Backend uses SQLITE database that is integrated within the application
        public static DatabaseManagement Database
        {
            get
            {
                if (database == null)
                {
                    database = new DatabaseManagement(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "data"));
                }
                return database;
            }
        }

        //Stores temporary data that gets deleted when the application terminates.
        //WARNING: all permanent data gets stored within the database!
        public static EmailUser? EmailUserData { get; set; }


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            Iconify.With(new MaterialModule());
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        protected override void OnPostCreate(Bundle? savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);
            List<UserData> userdata = Database.GetUserData().Result;

            //Assumption is that later on in development, I will add more than one email user.
            //If there's no data - request that user does login.
            if (userdata == null || userdata.Count == 0)
            {
                Intent login = new(this, typeof(Login));
                StartActivity(login);
            }
            else if (userdata.Count == 1)
            {
                EmailUserData = new EmailUser(userdata[0].Username, userdata[0].Password);
                Intent intent = new(Application.Context, typeof(EmailActivity));
                StartActivity(intent);
            }
            Finish();
        }
        protected override void OnResume()
        {
            base.OnResume();
        }
    }
    }