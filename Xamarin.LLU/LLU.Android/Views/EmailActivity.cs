using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.DrawerLayout.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Google.Android.Material.Navigation;
using JoanZapata.XamarinIconify;
using JoanZapata.XamarinIconify.Fonts;
using LLU.Android.Controllers;
using LLU.Android.LLU.Models;
using LLU.Android.Models;
using LLU.Models;
using MailKit;
using Unity;
using Xamarin.Essentials;

namespace LLU.Android.Views;

/// <summary>
/// </summary>
[Activity(Label = "EmailActivity",
    LaunchMode = LaunchMode.SingleTop)]
public class EmailActivity : Activity {
    private readonly DisplayInfo _displayInfo = DeviceDisplay.MainDisplayInfo;
    private readonly RecyclerView _recyclerView = new(Application.Context);
    private EmailsViewAdapter _adapter = null!;
    private DrawerLayout _drawerLayout = null!;
    private SwipeRefreshLayout _eaRefresher = null!;
    private Button _exitButton = null!;
    private Button _hamburgerMenu = null!;
    private List<DatabaseData> _messages = new();
    private RecyclerView.LayoutManager _mLayoutManager = null!;
    private NavigationView _navigationView = null!;
    private Button _writeButton = null!;

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
        _eaRefresher = FindViewById<SwipeRefreshLayout>(Resource.Id.EA_Refresher)!;

        _hamburgerMenu.Background =
            new IconDrawable(this, FontAwesomeIcons.fa_navicon.ToString())
                .WithColor(Color.Red)
                .WithSizePx(50);
        _writeButton.Background =
            new IconDrawable(this, FontAwesomeIcons.fa_pencil.ToString())
                .WithColor(Color.Red)
                .WithSizePx(50);
        _hamburgerMenu.Click += MenuClick;
        _writeButton.Click += WriteButton_Click;
        _exitButton.Click += ExitButton_Click;
        _eaRefresher.SetColorSchemeColors(Resource.Color.material_grey_800,
            Resource.Color.material_blue_grey_800);
        _eaRefresher.Refresh += HandleRefresh;
        CreateNotificationFromIntent(Intent);
    }
    protected override void OnPostCreate(Bundle? savedInstanceState) {
        base.OnPostCreate(savedInstanceState);
        Button showPopupMenu = FindViewById<Button>(Resource.Id.AppBarMenu);
        showPopupMenu.Gravity = GravityFlags.Right;
        showPopupMenu.Background = new IconDrawable(this,FontAwesomeIcons.fa_bars.ToString());
        showPopupMenu.Click += (s, arg) => {
            PopupMenu menu = new PopupMenu(this, showPopupMenu);
            menu.Inflate(Resource.Menu.EmailActivityMenu);
            menu.MenuItemClick += (sender, args) => {
                var item = args.Item;
                if (item is null) return;
                if (item.ItemId == Resource.Id.delete) {
                    var list = new List<UniqueId>();
                    var position = _adapter.selectedPosition;
                    list.Add(UniqueId.Parse(_messages[position].UniqueId));
                    EmailUser.EmailUserData.DeleteMessage(list);
                    HandleRefresh(this,EventArgs.Empty);
                    _adapter.selectedPosition = -1;
                }
            };
            menu.Show();
        };
        
        //Initialize adapter
        _adapter = new EmailsViewAdapter(_messages);
        _adapter.ItemClick += OnItemClick;
        _adapter.ItemLongClick += OnItemLongClick;

        //Initialize Recyclerview for listing emails
        var layout = FindViewById<FrameLayout>(Resource.Id.container);
        _recyclerView.SetMinimumHeight((int) _displayInfo.Height);
        _recyclerView.SetMinimumWidth((int) _displayInfo.Width);
        _recyclerView.SetAdapter(_adapter);

        // Plug in the linear layout manager:
        _mLayoutManager = new LinearLayoutManager(this);
        _recyclerView.SetLayoutManager(_mLayoutManager);
        layout.AddView(_recyclerView);
        HandleRefresh(this, EventArgs.Empty);
    }


    protected override void OnNewIntent(Intent intent) {
        CreateNotificationFromIntent(intent);
    }

    private void CreateNotificationFromIntent(Intent intent) {
        if (intent?.Extras != null) {
            var title = intent.GetStringExtra(NotificationController.TitleKey);
            var message = intent.GetStringExtra(NotificationController.MessageKey);
            App.Container.Resolve<INotificationController>().ReceiveNotification(title, message);
        }
    }

    private void HandleRefresh(object sender, EventArgs e) {
        if (User.EmailUserData is not null) {
            var lists = User.EmailUserData.Messages;
            _messages = lists.ToList();
            _adapter._messages = _messages;
            _adapter.NotifyDataSetChanged();
        }

        _recyclerView.RefreshDrawableState();
        _eaRefresher.Refreshing = false;
    }

    private void WriteButton_Click(object sender, EventArgs e) {
        var writeEmail = new Intent(this, typeof(WriteEmailActivity));
        StartActivity(writeEmail);
    }

    private void ExitButton_Click(object sender, EventArgs e) {
        if (AccountController.LogOut(this)) Finish();
    }

    private void MenuClick(object sender, EventArgs e) {
        if (!_drawerLayout.IsOpen)
            _drawerLayout.Open();
        else
            _drawerLayout.Close();
    }
    protected override void OnDestroy() {
        base.OnDestroy();
        //User.EmailUserData?.Dispose();
    }

    /// <summary>
    ///     <para>
    ///         On item long click select a message.
    ///         Show a small menu that allows to delete or move a message
    ///     </para>
    /// </summary>
    /// <exception cref="NotImplementedException">NOT YET IMPLEMENTED.</exception>
    private void OnItemLongClick(object sender, int e) {
        _adapter.selectedPosition = e;
        _adapter.NotifyItemChanged(e);
        
    }

    public override bool OnCreateOptionsMenu(IMenu menu) {
        MenuInflater.Inflate(Resource.Menu.EmailActivityMenu,menu);
        return base.OnCreateOptionsMenu(menu); 
    }

    public override bool OnOptionsItemSelected(IMenuItem item) {
        if (item.ItemId == Resource.Id.delete) {
            var list = new List<UniqueId>();
            var position = _adapter.selectedPosition;
            list.Add(UniqueId.Parse(_messages[position].UniqueId));
            EmailUser.EmailUserData.DeleteMessage(list);
            _messages.RemoveAt(position);
            _adapter.NotifyItemRemoved(position);
            _adapter.selectedPosition = -1;
            return base.OnOptionsItemSelected(item);
        }

        return true;
    }

    /// <summary>
    ///     Create an intent to launch a new activity.
    ///     This is the time to bundle up extra message data and pass it down to the next activity!
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="position"></param>
    private void OnItemClick(object sender, int position) {
        var intent = new Intent(this, typeof(EmailBody));
        var body = _messages[position].Body;
        intent.PutExtra("PositionInDb", position);

        intent.PutExtra("Body",
            _messages[position].IsHtmlBody
                ? new[] {_messages[position].Body, "html"}
                : new[] {_messages[position].Body, "txt"});

        intent.PutExtra("From", _messages[position].From);
        intent.PutExtra("To", _messages[position].To);
        intent.PutExtra("Subject", _messages[position].Subject);
        var message = User.EmailUserData?.GetMessageFromServer(_messages[position].UniqueId);
        if (message?.Attachments != null) {
            intent.PutExtra("Attachments", true);
            var filePaths = DataController.SaveAttachments(message);
            intent.PutExtra("AttachmentLocationOnDevice", filePaths);
        }

        _messages[position].NewFlag = false;
        User.EmailUserData?.SetMessageFlags(_messages[position].UniqueId, MessageFlags.Seen);
        RuntimeController.Instance.UpdateDatabase(_messages[position]);
        _adapter.NotifyItemChanged(position);
        StartActivity(intent);
    }
}