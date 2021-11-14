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
        #region Imap Client
        // WARNING: One should be careful with Disconnect(). It's advised to disconnect from the server to free up the socket, but one should not do it too often.
        // LLU has some kind of security in place that blocks too many attempted Connect() requests - from this app and other third party apps like thunderbird.
        // As far as I am aware, the suggestion is to disconnect on app exit or pause.
        protected ImapClient _client;
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
        #endregion
        protected List<MimeMessage> _messages;
        private List<MimeMessage> Messages
        {
            get
            {
                DoItAgain:
                if(_messages != null)
                {
                    var isChanged = Database.CheckForChanges(Userid,_messages[0].MessageId,_messages.Count);
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
                        _messages = messages;
                    }
                }
                return null;
            }
        }
        
        private EmailUser()
            //Gets and assigns secrets from deserialized json file, before the rest of the construction begins.
        {
            Host = Secrets.MailServer;
            Port = Secrets.MailPort;
        }

        // Constructor creates a new EmailUser on app launch.
        // Assumption remains that this constructor is used at first-launch of the application or when app fails to connect with using database data (credential change)
        // WARNING: This does not validate connection. Validity should be done within LoginActivity and AccountManager.
        // This class, for all intents and purposes, only is the middle layer between Database, Server and functionality.
        public EmailUser(string username, string password):this()
        {
            UserData userData = new();
            try
            {
                // WARNING: One should avoid validating by char count(). Not all LLU employees or students have matr.code;
                // Notable examples are students who were employees of LLU first, before they begun to study.
                string userid;

                if (username.Contains('@'))
                    //Remove the @llu.lv part, if the client added it.
                    userid = username.Split('@')[0];
                else
                    userid = username;

                userData.UserID = userid;

                // WARNING: LLU only accepts username that contains the part before the @ (matr.code), while other email services accept either both or full username with domain.
                if(username.ToLower().Contains("@llu.lv"))
                    userData.Username = userid;
                else
                    userData.Username = username;
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
        public string GetUserid() => Userid;
        public List<MimeMessage> GetMessages() => Messages;
        public void DeleteMessages(List<string> Uids)
        {

        }
        public ImapClient GetClient() => Client;
        public void DisconnectClient()
        {
            if (Client.IsConnected)
                Client.Disconnect(true);
        }
    }
}