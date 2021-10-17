using System;
using System.Collections.Generic;
using Android.App;
using Android.Widget;
using LLU.Android.Controllers;
using MailKit.Net.Pop3;
using MimeKit;

namespace LLU.Android.LLU.Models
{
    public class EmailUser : User
    {
        private readonly EmailProtocol _protocol = EmailProtocol.Pop3;
        private readonly int port = 1100;
        private readonly string host = "pop3.mailtrap.io";
        protected List<MimeKit.MimeMessage> _messages = new List<MimeMessage>();
        public EmailUser(string username, string password)
        {
            if (Email.CheckConnection(host,port,username,password,_protocol))
            {
                this.username = username;
                this.password = password;
            }
            Toast.MakeText(Application.Context,"Failed to connect!",ToastLength.Long);
        }
    }
}