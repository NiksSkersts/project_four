using System;
using System.Threading;
using Android.App;
using Android.Graphics;
using Android.Widget;
using Android.OS;
using AndroidX.RecyclerView.Widget;
using Xamarin.Essentials;
using LLU.Android.LLU.Models;
using MimeKit;
using System.Collections.Generic;
using LLU.Android.Controllers;

namespace LLU.Android
{
    [Activity(Label = "LLU", MainLauncher = true)]
    public class MainActivity : Activity
    {
        RecyclerView mRecyclerView;
        RecyclerView.LayoutManager mLayoutManager;
        EmailsViewAdapter adapter;
        int count = 1;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //Variables
            List<MimeMessage> _messages = new();
            var mainDisplayInfo = DeviceDisplay.MainDisplayInfo;


            EmailUser user = new EmailUser("", "");
            user.ReturnMessages();
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);


            // Plug in the linear layout manager:
            mLayoutManager = new LinearLayoutManager(this);
            mRecyclerView.SetLayoutManager(mLayoutManager);

            adapter = new EmailsViewAdapter(_messages);
            // Get our button from the layout resource,
            // and attach an event to it
            //Button button = FindViewById<Button>(Resource.Id.myButton);
            LinearLayout layout = FindViewById<LinearLayout>(Resource.Id.mainLayout);
            RecyclerView recyclerView = new RecyclerView(Application.Context);
            recyclerView.SetMinimumHeight((int)mainDisplayInfo.Height);
            recyclerView.SetMinimumWidth(((int)mainDisplayInfo.Width));
            recyclerView.SetBackgroundColor(color:new Color(0,0,0));
            recyclerView.SetAdapter(adapter);
            layout.AddView(recyclerView);
            //button.Click += delegate { button.Text = $"{count++} clicks!"; };

        }
            
        }
    }