using System;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using JoanZapata.XamarinIconify;
using JoanZapata.XamarinIconify.Fonts;
using LLU.Android.Controllers;
using LLU.Android.LLU.Models;
using LLU.Models;

namespace LLU.Android.Views;

[Activity(Label = "Write Email")]
internal class WriteEmailActivity : Activity {
    private EditText _body = null!;
    private Button _sendButton = null!;
    private EditText _subject = null!;
    private EditText _to = null!;

    protected override void OnCreate(Bundle? savedInstanceState) {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.WriteEmailActivity);
        _to = FindViewById<EditText>(Resource.Id.WE_To)!;
        _subject = FindViewById<EditText>(Resource.Id.WE_Subject)!;
        _body = FindViewById<EditText>(Resource.Id.WE_Body)!;
        _sendButton = FindViewById<Button>(Resource.Id.WE_Send)!;
        _sendButton.Click += SendButton_Click;
        _sendButton.Background =
            new IconDrawable(this, FontAwesomeIcons.fa_paper_plane.ToString()).WithColor(Color.Red);
    }

    private void SendButton_Click(object sender, EventArgs e) {
        var attemptToSend = false;
        if (string.IsNullOrEmpty(_to.Text)) {
            MessagingController.WarningNoRecipients();
            return;
        }
        var email = EmailUser.CreateEmail(_to.Text,_subject.Text,_body.Text);
        if (User.UserData != null) {
            if (User.EmailUserData != null) attemptToSend = User.EmailUserData.SendEmail(email);
        }
        if (attemptToSend)
            Finish();
        else
            MessagingController.ShowSmtpSendError();
    }
}