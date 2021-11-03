using System;
using System.Collections.Generic;
using LLU.Android.Controllers;
using LLU.Models;
using MailKit.Net.Imap;
using MimeKit;

#nullable enable
namespace LLU.Android.LLU.Models
{
    public class EmailUser
    {
        readonly DatabaseManagement db = MainActivity.Database;
        //DISCLAIMER: this data is publicly available for everyone to see.
        private readonly string host = "mail.llu.lv";
        private readonly int port = 993;
        //END OF DISCLAIMER

        protected readonly string username = string.Empty;
        protected readonly string password = string.Empty;
        protected List<MimeMessage> _messages = new();

        //Constructor creates a new EmailUser from username and password
        //It's used at times when app can't find any users in database
        //Assumption remains that this constructor is used at first-launch of the application or when app fails to connect with using database data (credential change)
        public EmailUser(string username, string password)
        {
            this.username = username;
            this.password = password;
            var imapClient = Email.Connect(host, port, this.username, this.password);
            if(imapClient!= null)
            {
                GetMessages(imapClient);
                UserData userData = new();
                try
                {
                    var userid = this.username.Split('@')[0];
                    userData.UserID =userid;
                    userData.Username = this.username;
                    userData.Password = this.password;
                    _ = db.SaveUserAsync(userData).Result;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                SaveMessagesToDB(userData);
            }
            else
            {
                //todo add safeguard on client == null
            }
        }

        //Constructor gets user data from database at app start and builds cache from that.
        public EmailUser()
        {
            var data = db.GetUserData().Result;
            if (data!=null)
            {
                this.username = data[0].Username;
                this.password = data[0].Password;
            }
            var client = Email.Connect(host, port, username, password);
            if(client != null)
            {
                GetMessages(client);
                SaveMessagesToDB(data[0]);
            }
            return;
        }
        private void GetMessages(ImapClient? client)
        {
            if (client != null)
                _messages = Email.GetMessages(client);
            else _messages = Email.GetMessages(host, port, username, password);

            _messages?.Reverse();
        }
        private void SaveMessagesToDB(UserData userdata) => db.SaveAllEmailAsync(_messages, userdata.UserID);
        public List<MimeMessage>? ReturnMessages()
        {
            if (_messages.Count == 0 || _messages == null)
                return null;
            else
                return _messages;
        }
    }
}