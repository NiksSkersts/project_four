using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Text;
using Android.Webkit;
using Android.Widget;
using LLU.Android.Models;
using Xamarin.Essentials;

namespace LLU.Android.Views;

[Activity(Label = "EmailBody")]
public class EmailBody : Activity {
    private Dictionary<string, string>? _data;
    private AttachmentDataAdapter _intentAttachmentsAdapter = null!;
    private List<string> _listviewData = null!;

    private Dictionary<string, string> AttachmentData {
        get {
            if (_data is not null)
                return _data;
            var filepaths = IntentAttachments;
            var names = new Dictionary<string, string>();
            if (filepaths != null)
                foreach (var path in filepaths) {
                    var name = path.Split('/');
                    names.Add(name[^1], path);
                }

            _data = names;
            return _data;
        }
    }

    private IEnumerable<string>? IntentAttachments {
        get {
            var attachmentsAvailable = Intent is {Extras: { }} && Intent.Extras.GetBoolean("Attachments");
            if (Intent?.Extras != null && attachmentsAvailable)
                return Intent.Extras.GetStringArray("AttachmentLocationOnDevice")!;

            return null;
        }
    }

    private TextView Subject { get; set; } = null!;
    private LinearLayout MainLayout { get; set; } = null!;
    private TextView From { get; set; } = null!;
    private TextView To { get; set; } = null!;
    private WebView Body { get; set; } = null!;
    private ListView Attachments { get; set; } = null!;

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
            var type = $"text/{body?[1]}";
            //Switch between two types of email. HTML and plain.
            //plain and hmtl have different formatting, and is not cross-supported in webmail.
            //plain text in html mode looks bad and vice-versa
            Body.LoadDataWithBaseURL(null, body?[0] ?? string.Empty, type, "UTF-8", null);
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