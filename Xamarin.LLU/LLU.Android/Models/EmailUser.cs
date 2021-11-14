using LLU.Android.Controllers;
using LLU.Models;
using MailKit.Net.Imap;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace LLU.Android.LLU.Models
{
    public class EmailUser : User
    {
        protected ImapClient? _client;
        //WARNING: Client should be disconnected after being used!
        private ImapClient? Client
        {
            get
            {
                if (_client == null)
                {
                    var client = Email.Connect(Host, Port, Username, Password);
                    if (client != null)
                        _client = client;
                    else
                        return null;
                }
                return _client;

            }
        }

        protected List<MimeMessage> _messages;
        private List<MimeMessage> Messages
        {
            get
            {
                if (_messages == null)
                {
                    using var client = Client;
                    if (client == null)
                        return new();
                    _messages = Email.GetMessages(client);
                    if (_messages == null)
                    {
                        _messages = new();
                    }else
                    {
                        _messages?.Reverse();
                        client.DisconnectAsync(true);
                       
                    }
                }
                else
                {
                    return _messages;
                }
                return new();
            }
        }

        //Constructor creates a new EmailUser from username and password
        //It's used at times when app can't find any users in database
        //Assumption remains that this constructor is used at first-launch of the application or when app fails to connect with using database data (credential change)
        public EmailUser(string username, string password)
        {
            UserData userData = new();
            try
            {
                //Remove the @llu.lv part, if the client added it, just in case.
                var userid = username.Split('@')[0];
                userData.UserID = userid;
                userData.Username = userid;
                userData.Password = password;
                _ = Database.SaveUserAsync(userData).Result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Userid = userData.UserID;
            Username = userData.Username;
            Password = userData.Password;
            if (Messages != null)
                SaveMessagesToDB(Messages, userData);
        }

        //Constructor gets user data from database at app start and builds cache from that.
        public EmailUser()
        {
            var data = AccountManager.UserData;
            if (data != null)
            {
                Username = data.Username;
                Password = data.Password;
                if (Messages != null)
                    SaveMessagesToDB(Messages, data);
            }
        }
        private void SaveMessagesToDB(List<MimeMessage> messages, UserData userdata) => Database.SaveAllEmailAsync(messages, userdata.UserID);
        public string GetUserid() => Userid;
        public List<MimeMessage> GetMessages() => Messages;
        public void DeleteMessages(List<string> Uids)
        {

        }
    }
}