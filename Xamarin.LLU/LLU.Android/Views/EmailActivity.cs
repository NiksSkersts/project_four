using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using AndroidX.DrawerLayout.Widget;
using AndroidX.RecyclerView.Widget;
using Google.Android.Material.Navigation;
using JoanZapata.XamarinIconify;
using JoanZapata.XamarinIconify.Fonts;
using LLU.Android.Controllers;
using LLU.Android.LLU.Models;
using MimeKit;
using System;
using System.Collections.Generic;
using Xamarin.Essentials;

namespace LLU.Android.Views
{
    [Activity(Label = "EmailActivity")]
    public class EmailActivity : Activity
    {
        #region Declaration
        List<int> SelectedMessages = new();
        private RecyclerView _recyclerView = new(Application.Context);
        private RecyclerView.LayoutManager mLayoutManager;
        private EmailsViewAdapter adapter;
        private List<MimeMessage> _messages = new();
        private DrawerLayout drawerLayout;
        private NavigationView navigationView;
        private Button ExitButton;
        private Button HamburgerMenu;
        private Button WriteButton;
        DisplayInfo _displayInfo = DeviceDisplay.MainDisplayInfo;
        #endregion
        // todo fix xiconify
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);
            Iconify.With(new MaterialModule());
            Iconify.With(new FontAwesomeModule());
            SetContentView(Resource.Layout.EmailActivity);

            var toolbar = FindViewById<Toolbar>(Resource.Id.NavToolbar);
            var ExitButton = FindViewById<Button>(Resource.Id.LogoutButton);
            HamburgerMenu = FindViewById<Button>(Resource.Id.HamburgerButton);
            WriteButton = FindViewById<Button>(Resource.Id.WriteEmailButton);
            drawerLayout = FindViewById<DrawerLayout>(Resource.Id.EmailDrawer);
            navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);

            HamburgerMenu.Background = new IconDrawable(this, FontAwesomeIcons.fa_bug.ToString()).WithColor(Color.Red);
            WriteButton.Background = new IconDrawable(this, FontAwesomeIcons.fa_500px.ToString()).WithColor(Color.Red);
            HamburgerMenu.Click += MenuClick;
            WriteButton.Click += WriteButton_Click;
            ExitButton.Click += ExitButton_Click;
        }

        private void WriteButton_Click(object sender, EventArgs e)
        {
            Intent WriteEmail = new Intent(this, typeof(WriteEmailActivity));
            StartActivity(WriteEmail);
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            EmailUser.Database.WipeDatabase();
            EmailUser.EmailUserData.Dispose();
            Intent BackToStart = new Intent(this, typeof(LoginActivity));
            StartActivity(BackToStart);
            Finish();
        }

        private void MenuClick(object sender, EventArgs e)
        {
            if (!drawerLayout.IsOpen)
            {
                drawerLayout.Open();

            }
            else
            {
                drawerLayout.Close();
            }
        }

        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);

            _messages.AddRange(EmailUser.EmailUserData.Messages);

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
            mLayoutManager = new LinearLayoutManager(this);
            _recyclerView.SetLayoutManager(mLayoutManager);
            layout.AddView(_recyclerView);
        }
        protected override void OnStop()
        {
            base.OnStop();
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            EmailUser.EmailUserData.Dispose();
        }
        private void OnItemLongClick(object sender, int e)
        {
            SelectedMessages.Add(e);
        }
        private void ExecuteDeleteAll(object sender, EventArgs e)
        {
            List<string> uids = new();
            foreach (var message in _messages)
            {
                uids.Add(message.MessageId);
            }
            //EmailUser.EmailUserData.DeleteMessages(uids);
            _messages.Clear();
        }
        private void ExecuteDelete(object sender, EventArgs e)
        {
            List<string> uids = new();
            foreach (var position in SelectedMessages)
            {
                uids.Add(_messages[position].MessageId);
                SelectedMessages.Remove(position);
            }
            //EmailUser.EmailUserData.DeleteMessages(uids);
        }

        private void ExecuteSeen(object sender, EventArgs e)
        {

        }
        private void ExecuteSeenAll(object sender, EventArgs e)
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

            var filepaths = DataController.SaveAttachments(_messages[num]);
            if (filepaths != null)
                intent.PutExtra("AttachmentLocationOnDevice", filepaths);
            StartActivity(intent);
        }
    }
}