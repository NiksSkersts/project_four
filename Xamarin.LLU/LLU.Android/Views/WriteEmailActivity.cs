using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using LLU.Android.LLU.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LLU.Android.Views
{
    [Activity(Label = "Write Email")]
    internal class WriteEmailActivity : Activity
    {
        private EditText to;
        private EditText subject;
        private EditText body;
        private Button SendButton;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.WriteEmailActivity);
            to = FindViewById<EditText>(Resource.Id.WE_To);
            subject = FindViewById<EditText>(Resource.Id.WE_Subject);
            body = FindViewById<EditText>(Resource.Id.WE_Body);
            SendButton = FindViewById<Button>(Resource.Id.WE_Send);

            SendButton.Click += SendButton_Click;


        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            EmailUser.EmailUserData.CreateAndSendMessage(to.Text.ToString(),subject.Text.ToString(),body.Text.ToString());
            Finish();
        }
    }
}