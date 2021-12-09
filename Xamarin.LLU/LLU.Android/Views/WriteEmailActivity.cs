using System;
using Android.App;
using Android.OS;
using Android.Widget;
using LLU.Android.Controllers;
using LLU.Models;

namespace LLU.Android.Views;

[Activity(Label = "Write Email")]
internal class WriteEmailActivity : Activity {
    private EditText body;
    private Button SendButton;
    private EditText subject;
    private EditText to;

    protected override void OnCreate(Bundle savedInstanceState) {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.WriteEmailActivity);
        to = FindViewById<EditText>(Resource.Id.WE_To)!;
        subject = FindViewById<EditText>(Resource.Id.WE_Subject)!;
        body = FindViewById<EditText>(Resource.Id.WE_Body)!;
        SendButton = FindViewById<Button>(Resource.Id.WE_Send)!;
        SendButton.Click += SendButton_Click;
    }

    private void SendButton_Click(object sender, EventArgs e) {
        var attemptToSend = body.Text != null && subject.Text != null && to.Text != null &&
                            User.EmailUserData.CreateAndSendMessage(to.Text, subject.Text, body.Text);
        if (attemptToSend)
            Finish();
        else
            MessagingController.ShowSmtpSendError();
    }
}