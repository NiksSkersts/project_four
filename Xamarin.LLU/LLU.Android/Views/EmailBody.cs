using Android.App;
using Android.Content;
using Android.OS;
using Android.Webkit;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using JoanZapata.XamarinIconify;
using JoanZapata.XamarinIconify.Fonts;
using LLU.Android.Controllers;
using System.Collections.Generic;

namespace LLU.Android
{
    [Activity(Label = "EmailBody")]
    public class EmailBody : Activity
    {
        private Dictionary<string, string> data = null;
        private Dictionary<string, string> AttachmentData
        {
            get
            {
                if (data != null)
                    return data;

                var filepaths = IntentAttachments;
                if (filepaths == null)
                    return new();
                var names = new Dictionary<string, string>();
                foreach (var path in filepaths)
                {
                    var name = path.Split('/');
                    names.Add(name[^1], path);
                }
                data = names;
                return names;
            }
        }
        private string[] IntentAttachments
        {
            get
            {
                var attachmentsavailable = Intent.Extras.GetBoolean("Attachments");
                if (Intent.Extras != null && attachmentsavailable)
                {
                    return Intent.Extras.GetStringArray("AttachmentLocationOnDevice");
                }
                return null;
            }
        }
        public TextView Subject { get; set; }
        public LinearLayout MainLayout { get; set; }
        public RecyclerView AttachmentFrame { get; set; }
        public TextView From { get; set; }
        public TextView To { get; set; }
        public WebView Body { get; set; }
        public ImageButton BackButton { get; set; }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.EmailBody);

            //BackButton = FindViewById<ImageButton>(Resource.Id.backbutton);
            //BackButton.SetImageDrawable(new IconDrawable(this, MaterialIcons.md_arrow_back.ToString()));
            //BackButton.Click += BackButtonClick;
            //Bind various views to their XAML counterparts. Create events and deal with intent extras that I specified in EmailActivity.cs
            MainLayout = FindViewById<LinearLayout>(Resource.Id.EmailMainLayout);
            Subject = FindViewById<TextView>(Resource.Id.EB_Subject);
            From = FindViewById<TextView>(Resource.Id.EB_From);
            To = FindViewById<TextView>(Resource.Id.EB_To);
            Body = new WebView(this);
            if (Intent.Extras != null)
            {
                var body = Intent.Extras.GetStringArray("Body");
                var from = $"Nosūtītājs <{Intent.Extras.GetString("From")}>";
                var to = $"Saņēmējs <{Intent.Extras.GetString("To")}>";
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
                AttachmentFrame = new(this);
                var adapter = new AttachmentViewAdapter(AttachmentData);
                var layoutmanager = new LinearLayoutManager(Application.Context);
                AttachmentFrame.SetAdapter(adapter);
                AttachmentFrame.SetLayoutManager(layoutmanager);
                MainLayout.AddView(AttachmentFrame);
            }
        }

        private void BackButtonClick(object sender, System.EventArgs e)
        {
            OnBackPressed();
        }
    }
}