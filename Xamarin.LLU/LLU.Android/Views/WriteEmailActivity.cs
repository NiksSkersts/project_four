using System;
using Android.App;
using Android.OS;
using Android.Widget;
using LLU.Android.Controllers;
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
    }

    private void SendButton_Click(object sender, EventArgs e) {
        var attemptToSend = User.EmailUserData != null
                            && _body.Text != null
                            && _subject.Text != null
                            && _to.Text != null
                            && User.EmailUserData.CreateAndSendMessage(_to.Text, _subject.Text, _body.Text);
        if (attemptToSend)
            Finish();
        else
            MessagingController.ShowSmtpSendError();
    }
}