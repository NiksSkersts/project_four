using Android.App;
using Android.OS;
using Android.Content;
using LLU.Android.Views;
using LLU.Android.Controllers;
using System.IO;
using LLU.Android.LLU.Models;
using System.Collections.Generic;
using LLU.Models;

#nullable enable
namespace LLU.Android
{
    [Activity(Label = "LLU", MainLauncher = true)]
    public class MainActivity : Activity
    {
        static DatabaseManagement database;
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
        public static EmailUser EmailUserData { get; set; }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
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