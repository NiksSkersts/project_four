using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using JoanZapata.XamarinIconify;
using JoanZapata.XamarinIconify.Fonts;
using LLU.Android.Controllers;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Essentials;

namespace LLU.Android
{
    [Activity(Label = "EmailBody")]
    public class EmailBody : Activity
    {
        private Dictionary<string,string> AttachmentData {
            get
            {
                var filepaths = IntentAttachments;
                if (filepaths == null)
                    return null;
                var names = new Dictionary<string,string>();
                foreach (var path in filepaths)
                {
                    var name = path.Split('/');
                    names.Add(name[name.Length-1], path);
                }
                return names;
            }
        }
        private string[] IntentAttachments 
        { 
            get
            {
                var attachmentsavailable = Intent.Extras.GetBoolean("Attachments");
                if (Intent.Extras!=null && attachmentsavailable)
                {
                    return Intent.Extras.GetStringArray("AttachmentLocationOnDevice");
                }
                return null;
            } 
        }
        public TextView Subject { get; set; }
        public LinearLayout MainLayout { get; set; }
        public TextView From { get; set; }
        public TextView To { get; set; }
        public WebView Body { get; set; }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.EmailBody);

            //Bind various views to their XAML counterparts. Create events and deal with intent extras that I specified in EmailActivity.cs
            MainLayout = FindViewById<LinearLayout>(Resource.Id.EmailMainLayout);
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
            if (AttachmentData != null)
            {
                TextView temp = new(this)
                {
                    Text = "Pielikumi",
                    Clickable = false
                };
                MainLayout.AddView(temp);
                for (int i = 0; i<AttachmentData.Count;i++)
                {
                    temp = new(this)
                    {
                        Text = AttachmentData.Keys.ToArray()[i],
                        Clickable = true
                    };
                    temp.Click += AttachmentClick;
                    MainLayout.AddView(temp);
                }
            }
        }

        private void AttachmentClick(object sender, System.EventArgs e)
        {
            var attachment = sender as TextView;
            var filename = attachment.Text;
            Launcher.OpenAsync(new OpenFileRequest()
            {
                File = new ReadOnlyFile(AttachmentData.Single(key=>key.Key.Equals(filename)).Value)
            });
        }
    }
}