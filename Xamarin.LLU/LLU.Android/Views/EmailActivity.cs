using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using LLU.Android.Controllers;
using MimeKit;
using System.Collections.Generic;
using Xamarin.Essentials;

#nullable enable
namespace LLU.Android.Views
{
    [Activity(Label = "EmailActivity")]
    public class EmailActivity : Activity
    {
        private RecyclerView _recyclerView = new(Application.Context);
        private RecyclerView.LayoutManager mLayoutManager;
        private EmailsViewAdapter adapter;
        private List<MimeMessage>? _messages;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            var mainDisplayInfo = DeviceDisplay.MainDisplayInfo;
            _messages = MainActivity.EmailUserData.ReturnMessages();

            //create a new instance of _messages to prevent adapter from crashing
            if (_messages == null)
                _messages = new List<MimeMessage>();
            SetContentView(Resource.Layout.Main);

            //Initialize adapter
            adapter = new EmailsViewAdapter(_messages);
            adapter.ItemClick += OnItemClick;

            //Initialize Recyclerview for listing emails
            LinearLayout layout = FindViewById<LinearLayout>(Resource.Id.mainLayout);
            _recyclerView.SetMinimumHeight((int)mainDisplayInfo.Height);
            _recyclerView.SetMinimumWidth(((int)mainDisplayInfo.Width));
            _recyclerView.SetBackgroundColor(color: new Color(0, 0, 0));
            _recyclerView.SetAdapter(adapter);

            // Plug in the linear layout manager:
            mLayoutManager = new LinearLayoutManager(Application.Context);
            _recyclerView.SetLayoutManager(mLayoutManager);
            layout?.AddView(_recyclerView);
        }
        private void OnItemClick(object sender, int position)
        {
            var num = position;
            var htmlbody = _messages[num].HtmlBody;
            var text = _messages[num].TextBody;
            var intent = new Intent(this, typeof(EmailBody));
            if(htmlbody!= null)
                intent.PutExtra("Body", new string[2] { _messages[num].HtmlBody,"html" });
            else intent.PutExtra("Body", new string[2] { _messages[num].TextBody, "txt"});

            intent.PutExtra("From", _messages[num].From.ToString());
            intent.PutExtra("To", _messages[num].To.ToString());
            intent.PutExtra("Subject", _messages[num].Subject);
            StartActivity(intent);
        }
    }
}