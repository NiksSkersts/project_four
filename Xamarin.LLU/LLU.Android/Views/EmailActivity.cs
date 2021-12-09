using System;
using System.Collections.Generic;
using System.Linq;
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
using LLU.Android.Models;
using LLU.Models;
using MimeKit;
using Xamarin.Essentials;

namespace LLU.Android.Views;

[Activity(Label = "EmailActivity")]
public class EmailActivity : Activity {
    protected override void OnCreate(Bundle? savedInstanceState) {
        base.OnCreate(savedInstanceState);
        Platform.Init(this, savedInstanceState);
        Iconify.With(new MaterialModule());
        Iconify.With(new FontAwesomeModule());
        SetContentView(Resource.Layout.EmailActivity);

        _exitButton = FindViewById<Button>(Resource.Id.LogoutButton)!;
        _hamburgerMenu = FindViewById<Button>(Resource.Id.HamburgerButton)!;
        _writeButton = FindViewById<Button>(Resource.Id.WriteEmailButton)!;
        _drawerLayout = FindViewById<DrawerLayout>(Resource.Id.EmailDrawer)!;
        _navigationView = FindViewById<NavigationView>(Resource.Id.nav_view)!;

        _hamburgerMenu.Background = new IconDrawable(this, FontAwesomeIcons.fa_bug.ToString()).WithColor(Color.Red);
        _writeButton.Background = new IconDrawable(this, FontAwesomeIcons.fa_500px.ToString()).WithColor(Color.Red);
        _hamburgerMenu.Click += MenuClick;
        _writeButton.Click += WriteButton_Click;
        _exitButton.Click += ExitButton_Click;
    }

    private void WriteButton_Click(object sender, EventArgs e) {
        var writeEmail = new Intent(this, typeof(WriteEmailActivity));
        StartActivity(writeEmail);
    }

    private void ExitButton_Click(object sender, EventArgs e) {
        User.Database.WipeDatabase();
        User.EmailUserData.Dispose();
        var backToStart = new Intent(this, typeof(LoginActivity));
        StartActivity(backToStart);
        Finish();
    }

    private void MenuClick(object sender, EventArgs e) {
        if (!_drawerLayout.IsOpen)
            _drawerLayout.Open();
        else
            _drawerLayout.Close();
    }

    protected override void OnPostCreate(Bundle? savedInstanceState) {
        base.OnPostCreate(savedInstanceState);

        _messages.AddRange(User.EmailUserData.Messages);

        //Initialize adapter
        _adapter = new EmailsViewAdapter(_messages);
        _adapter.ItemClick += OnItemClick;
        _adapter.ItemLongClick += OnItemLongClick;

        //Initialize Recyclerview for listing emails
        var layout = FindViewById<LinearLayout>(Resource.Id.mainLayout);
        _recyclerView.SetMinimumHeight((int) _displayInfo.Height);
        _recyclerView.SetMinimumWidth((int) _displayInfo.Width);
        _recyclerView.SetBackgroundColor(new Color(0, 0, 0));
        _recyclerView.SetAdapter(_adapter);

        // Plug in the linear layout manager:
        _mLayoutManager = new LinearLayoutManager(this);
        _recyclerView.SetLayoutManager(_mLayoutManager);
        layout?.AddView(_recyclerView);
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        User.EmailUserData.Dispose();
    }

    private void OnItemLongClick(object sender, int e) {
        _selectedMessages.Add(e);
    }

    private void ExecuteDeleteAll(object sender, EventArgs e) {
        var uids = _messages.Select(message => message.MessageId).ToList();
        _messages.Clear();
    }

    private void ExecuteDelete(object sender, EventArgs e) {
        List<string> uids = new();
        foreach (var position in _selectedMessages) {
            uids.Add(_messages[position].MessageId);
            _selectedMessages.Remove(position);
        }
    }

    private void OnItemClick(object sender, int position) {
        //Create an intent to launch a new activity.
        //This is the time to bundle up extra message data and pass it down to the next activity!
        var intent = new Intent(this, typeof(EmailBody));

        var num = position;
        var htmlbody = _messages[num].HtmlBody;
        intent.PutExtra("PositionInDb", num);

        intent.PutExtra("Body",
            htmlbody != null
                ? new string[2] {_messages[num].HtmlBody, "html"}
                : new string[2] {_messages[num].TextBody, "txt"});

        intent.PutExtra("From", _messages[num].From.ToString());
        intent.PutExtra("To", _messages[num].To.ToString());
        intent.PutExtra("Subject", _messages[num].Subject);
        if (_messages[num].Attachments != null)
            intent.PutExtra("Attachments", true);

        var filepaths = DataController.SaveAttachments(_messages[num]);
        intent.PutExtra("AttachmentLocationOnDevice", filepaths);
        StartActivity(intent);
    }

    #region Declaration

    private readonly List<int> _selectedMessages = new();
    private readonly RecyclerView _recyclerView = new(Application.Context);
    private RecyclerView.LayoutManager _mLayoutManager;
    private EmailsViewAdapter _adapter;
    private readonly List<MimeMessage> _messages = new();
    private DrawerLayout _drawerLayout;
    private NavigationView _navigationView;
    private Button _exitButton;
    private Button _hamburgerMenu;
    private Button _writeButton;
    private readonly DisplayInfo _displayInfo = DeviceDisplay.MainDisplayInfo;

    #endregion
}