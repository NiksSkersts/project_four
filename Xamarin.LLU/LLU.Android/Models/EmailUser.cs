using LLU.Android.Controllers;
using LLU.Models;
using MailKit.Net.Imap;
using MimeKit;
using System;
using System.Collections.Generic;

namespace LLU.Android.LLU.Models
{
    public class EmailUser : User, IDisposable
    {
        #region Imap Client
        // WARNING: One should be careful with Disconnect(). It's advised to disconnect from the server to free up the socket, but one should not do it too often.
        // LLU has some kind of security in place that blocks too many attempted Connect() requests - from this app and other third party apps like thunderbird.
        // As far as I am aware, the suggestion is to disconnect on app exit or pause.
        // Imap clients were created with long-lived connections in mind. As far as I am aware, while not in active use, they enter IDLE mode and just sync from time to time to get push notifications.
        protected Tuple<byte, ImapClient> _client;
        public Tuple<byte, ImapClient> Client
        {
            get
            {
                if (_client == null)
                //Client should be null only when the app is launched for the first time, when disconnect was required or forced, e.g. password change
                {
                    var bundle = EmailController.Connect(Host, Port);
                    _client = bundle;
                }
                return _client;

            }
            set
            {
                if(value is null && Client.Item2.IsConnected)
                    // Value == null means that the program is requesting client to disconnect.
                    // Destroying client without disconnecting is strongly discouraged!
                {
                    Client.Item2.DisconnectAsync(true);
                    Client.Item2.Dispose();
                }
                else
                    _client = value;
            }
        }
        #endregion
        protected List<MimeMessage> _messages;
        public List<MimeMessage> Messages
        {
            get
            {
                if (_messages != null)
                    if (Database.CheckForChanges(UserData.UserID, _messages[0].MessageId, _messages.Count))
                        _messages = null;
                using var client = Client.Item2;
                if (client != null)
                {
                    List<MimeMessage> messages = EmailController.GetMessages(client);
                    if (messages != null)
                    {
                        messages.Reverse();
                        Database.ApplyChanges(messages, UserData.UserID);
                        _messages = messages;
                    }
                    else
                        return null;
                }
                else
                    return null;

                return _messages;
            }
            set
            {
                if (_messages == null)
                    _messages = value;
                else
                    _messages.AddRange(value);
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
        public EmailUser(string username, string password) : this()
        {
            UserData userData = new();
            try
            {
                // WARNING: One should avoid validating by char count(). Not all LLU employees or students have matr.code;
                // Notable examples are students who were employees of LLU first, before they begun to study.
                Guid temp = new();
                userData.UserID = temp.ToString();

                // WARNING: LLU only accepts username that contains the part before the @ (matr.code), while other email services accept either both or full username with domain.
                if (username.ToLower().Contains("@llu.lv"))
                    userData.Username = username.Split('@')[0];
                else
                    userData.Username = username;
                userData.Password = password;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            UserData = userData;
            Database.SaveUserAsync(UserData);
        }
        public string GetUserid() => UserData.UserID;
        public bool Auth()
        {
            if (Client.Item1 == 1) return false;
            if (Client.Item2.IsConnected)
            {
                try
                {
                    Client.Item2.Authenticate(UserData.Username, UserData.Password);
                    return true;
                }catch (Exception e)
                {
                    Console.WriteLine(e);
                    return false;
                }
            }
            return false;
        }

        // Overrides any value that is currently assigned to the variables.
        // Variables are created as "protected" and their value should not be allowed to be viewed in outside classes.
        // Setting a new value is fine as long as it doesn't leak the original value.
        public void SetUserName(string username) => UserData.Username = username;
        public void SetPassword(string password) => UserData.Password = password;

        public void Dispose()
        {
            Client = null;
            Database.Dispose();
        }
        public void ClientIdle()
        {

        }
    }
}