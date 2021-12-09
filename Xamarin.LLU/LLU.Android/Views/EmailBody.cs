using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.OS;
using Android.Webkit;
using Android.Widget;
using LLU.Android.Models;
using Xamarin.Essentials;

namespace LLU.Android.Views;

[Activity(Label = "EmailBody")]
public class EmailBody : Activity {
    private Dictionary<string, string> _data;
    private AttachmentDataAdapter _intentAttachmentsAdapter;
    private List<string> _listviewData;

    private Dictionary<string, string> AttachmentData {
        get {
            if (_data.Count() is not 0)
                return _data;

            var filepaths = IntentAttachments;
            var names = new Dictionary<string, string>();
            if (filepaths != null)
                foreach (var path in filepaths) {
                    var name = path.Split('/');
                    names.Add(name[^1], path);
                }

            _data = names;
            return names;
        }
    }

    private IEnumerable<string>? IntentAttachments {
        get {
            var attachmentsavailable = Intent.Extras.GetBoolean("Attachments");
            if (Intent.Extras != null && attachmentsavailable)
                return Intent.Extras.GetStringArray("AttachmentLocationOnDevice")!;

            return null;
        }
    }

    public TextView Subject { get; set; }
    public LinearLayout MainLayout { get; set; }
    public TextView From { get; set; }
    public TextView To { get; set; }
    public WebView Body { get; set; }
    public ListView Attachments { get; set; }

    protected override void OnCreate(Bundle? savedInstanceState) {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.EmailBodyActivity);
        MainLayout = FindViewById<LinearLayout>(Resource.Id.EmailMainLayout)!;
        Subject = FindViewById<TextView>(Resource.Id.EB_Subject)!;
        From = FindViewById<TextView>(Resource.Id.EB_From)!;
        To = FindViewById<TextView>(Resource.Id.EB_To)!;
        Body = FindViewById<WebView>(Resource.Id.EB_Body)!;

        if (Intent?.Extras != null) {
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

        _listviewData = new List<string>();
        foreach (var item in AttachmentData) _listviewData.Add(item.Key);

        _intentAttachmentsAdapter = new AttachmentDataAdapter(this, _listviewData);

        Attachments = FindViewById<ListView>(Resource.Id.EB_Attachments)!;
        Attachments.Adapter = _intentAttachmentsAdapter;
        Attachments.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => {
            var selectedFromList = Attachments.GetItemAtPosition(e.Position)!.ToString();
            var path = AttachmentData[selectedFromList];
            Launcher.OpenAsync(new OpenFileRequest {
                File = new ReadOnlyFile(path)
            });
        };
    }
}