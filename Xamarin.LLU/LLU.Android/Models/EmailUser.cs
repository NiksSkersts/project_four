using System;
using System.Collections.Generic;
using Android.App;
using Android.Widget;
using LLU.Android.Controllers;
using LLU.LLU.Models;
using MailKit.Net.Pop3;
using MimeKit;

namespace LLU.Android.LLU.Models
{
    public class EmailUser : User
    {
        private readonly int port = 993;
        private readonly int SMTP = 587;
        private readonly string host = "mail.llu.lv";
        protected List<MimeMessage> _messages = new();
        public EmailUser(string username, string password)
        {
            if (Email.CheckConnection(host,port,username,password))
            {
                this.username = username;
                this.password = password;
                GetMessages();
            }
            Toast.MakeText(Application.Context,"Failed to connect!",ToastLength.Long);
        }

        void GetMessages()
        {
            _messages = Email.GetMessages(host,port,username,password);
        }
        public List<MimeMessage> ReturnMessages()
        {
            return _messages;
        }
    }
}