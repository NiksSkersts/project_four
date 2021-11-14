using LLU.Android.Controllers;
using LLU.Models;
using MailKit.Net.Imap;
using MimeKit;
using System;
using System.Collections.Generic;

namespace LLU.Android.LLU.Models
{
    public class EmailUser : User
    {
        protected ImapClient _client;
        //WARNING: Client should be disconnected after being used!
        private ImapClient Client
        {
            get
            {
                if (_client == null)
                {
                    var client = EmailController.Connect(Host, Port, Username, Password);
                    if (client != null)
                        _client = client;
                    else
                        return null;
                }
                return _client;

            }
        }
        public bool IsAbleToConnect
        {
            get
            {
                try
                {
                    using var client = Client;
                    if (client == null)
                        return false;
                    if (Client.IsConnected)
                    {
                        Client.DisconnectAsync(true);
                        return true;
                    }
                    return false;
                }
                catch
                {
                    return false;
                }

            }
        }

        protected List<MimeMessage> _messages;
        private List<MimeMessage> Messages
        {
            get
            {
                DoItAgain:
                if(_messages != null)
                {
                    var isChanged = Database.CheckForChanges(Userid);
                    if (isChanged)
                    {
                        _messages = null;
                        goto DoItAgain;
                    }
                    return _messages;
                }
                using var client = Client;
                if (client != null)
                {
                    List<MimeMessage> messages = EmailController.GetMessages(client);
                    if (messages != null)
                    {
                        messages.Reverse();
                        Database.ApplyChanges(messages, Userid);
                        client.DisconnectAsync(true);
                        _messages = messages;
                    }
                }
                return null;
            }
        }
        //Gets and assigns secrets from deserialized json file, before the rest of the construction begins.
        private EmailUser()
        {
            Host = Secrets.MailServer;
            Port = Secrets.MailPort;
        }

        //Constructor creates a new EmailUser from username and password
        //It's used at times when app can't find any users in database
        //Assumption remains that this constructor is used at first-launch of the application or when app fails to connect with using database data (credential change)
        public EmailUser(string username, string password):this()
        {
            UserData userData = new();
            try
            {
                //Remove the @llu.lv part, if the client added it.
                var userid = username.Split('@')[0];
                userData.UserID = userid;
                userData.Username = userid;
                userData.Password = password;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Userid = userData.UserID;
            Username = userData.Username;
            Password = userData.Password;
        }

        //Constructor gets user data from database at app start and builds cache from that.
        public EmailUser(UserData data)
        {
            if (data != null)
            {
                Username = data.Username;
                Password = data.Password;
            }
        }
        public string GetUserid() => Userid;
        public List<MimeMessage> GetMessages() => Messages;
        public void DeleteMessages(List<string> Uids)
        {

        }
    }
}