﻿using Android.App;
using Android.Content;
using Android.OS;
using Android.Webkit;
using Android.Widget;

namespace LLU.Android
{
    [Activity(Label = "EmailBody")]
    public class EmailBody : Activity
    {
        public TextView Subject { get; set; }
        public TextView From { get; set; }
        public TextView To { get; set; }
        public WebView Body { get; set; }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here

            SetContentView(Resource.Layout.EmailBody);
            Subject = FindViewById<TextView>(Resource.Id.EB_Subject);
            From = FindViewById<TextView>(Resource.Id.EB_From);
            To = FindViewById<TextView>(Resource.Id.EB_To);
            Body = FindViewById<WebView>(Resource.Id.EB_Body);
            if (Intent.Extras != null)
            {
                var body = Intent.Extras.GetStringArray("Body");
                var from = Intent.Extras.GetString("From");
                var to = Intent.Extras.GetString("To");
                var subject = Intent.Extras.GetString("Subject");

                Subject.Text = subject;
                From.Text = from;
                To.Text = to;

                //Switch between two types of email. HTML and plain.
                //plain and hmtl have different formating, and is not cross-supported in webmail.
                //plain text in html mode looks bad and vice-versa
                var type = $"text/{body[1]}";
                Body.LoadDataWithBaseURL(null, body[0], type, "UTF-8", null);
            }
        }
    }
}