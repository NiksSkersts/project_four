using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using JoanZapata.XamarinIconify;
using JoanZapata.XamarinIconify.Fonts;
using LLU.Android.Controllers;
using LLU.Android.LLU.Models;
using LLU.Models;
using MimeKit;
using System;
using System.Collections.Generic;
using Xamarin.Essentials;

namespace LLU.Android.Views
{
    [Activity(Label = "EmailActivity")]
    public class EmailActivity : Activity
    {
        List<int> SelectedMessages = new();
        private RecyclerView _recyclerView = new(Application.Context);
        private RecyclerView.LayoutManager mLayoutManager;
        private EmailsViewAdapter adapter;
        private List<MimeMessage> _messages = new();
        DisplayInfo _displayInfo = DeviceDisplay.MainDisplayInfo;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);
            Iconify.With(new MaterialModule());
            SetContentView(Resource.Layout.EmailActivity);
            Button Seen = FindViewById<Button>(Resource.Id.ToolbarMainSeen);
            Button SeenAll = FindViewById<Button>(Resource.Id.ToolbarMainSeenAll);
            Button Delete = FindViewById<Button>(Resource.Id.ToolbarMainDelete);
            Button DeleteAll = FindViewById<Button>(Resource.Id.ToolbarMainDeleteAll);
            Seen.Click += ExecuteSeen;
            SeenAll.Click += ExecuteSeenAll;
            Delete.Click += ExecuteDelete;
            DeleteAll.Click += ExecuteDeleteAll;
        }
        protected override void OnPostCreate(Bundle? savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);

            _messages.AddRange(EmailUser.EmailUserData.GetMessages());
            //create a new instance of _messages to prevent adapter from crashing
            if (_messages == null)
                _messages = new List<MimeMessage>();

            //Initialize adapter
            adapter = new EmailsViewAdapter(_messages);
            adapter.ItemClick += OnItemClick;
            adapter.ItemLongClick += OnItemLongClick;

            //Initialize Recyclerview for listing emails
            LinearLayout layout = FindViewById<LinearLayout>(Resource.Id.mainLayout);
            _recyclerView.SetMinimumHeight((int)_displayInfo.Height);
            _recyclerView.SetMinimumWidth(((int)_displayInfo.Width));
            _recyclerView.SetBackgroundColor(color: new Color(0, 0, 0));
            _recyclerView.SetAdapter(adapter);

            // Plug in the linear layout manager:
            mLayoutManager = new LinearLayoutManager(Application.Context);
            _recyclerView.SetLayoutManager(mLayoutManager);
            layout?.AddView(_recyclerView);
        }
        private void OnItemLongClick(object sender, int e)
        {
            SelectedMessages.Add(e);
        }
        private void ExecuteDeleteAll(object sender, EventArgs e)
        {
            List<string> uids = new();
            foreach(var message in _messages)
            {
                uids.Add(message.MessageId);
            }
            EmailUser.EmailUserData.DeleteMessages(uids);
            _messages.Clear();
        }
        private void ExecuteDelete(object sender, EventArgs e)
        {
            List<string> uids = new();
            foreach (var position in SelectedMessages)
            {
                uids.Add(_messages[position].MessageId); 
            }
            EmailUser.EmailUserData.DeleteMessages(uids);
        }

        private void ExecuteSeen(object sender, System.EventArgs e)
        {

        }
        private void ExecuteSeenAll(object sender, System.EventArgs e)
        {

        }

        private void OnItemClick(object sender, int position)
        {
            //Create an intent to launch a new activity.
            //This is the time to bundle up extra message data and pass it down to the next activity!
            var intent = new Intent(this, typeof(EmailBody));

            var num = position;
            var htmlbody = _messages[num].HtmlBody;
            intent.PutExtra("PositionInDb", num);

            if (htmlbody != null)
                intent.PutExtra("Body", new string[2] { _messages[num].HtmlBody, "html" });
            else intent.PutExtra("Body", new string[2] { _messages[num].TextBody, "txt" });

            intent.PutExtra("From", _messages[num].From.ToString());
            intent.PutExtra("To", _messages[num].To.ToString());
            intent.PutExtra("Subject", _messages[num].Subject);
            if (_messages[num].Attachments != null)
                intent.PutExtra("Attachments", true);

            var filepaths = EmailController.SaveAttachments(_messages[num]);
            if (filepaths != null)
                intent.PutExtra("AttachmentLocationOnDevice", filepaths);
            StartActivity(intent);
        }
    }
}