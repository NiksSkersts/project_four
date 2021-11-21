using LLU.Controllers;
using LLU.Models;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LLU.Android.LLU.Models
{
    public class EmailUser : User, IDisposable
    {
        private ClientController clientController;
        protected List<MimeMessage> _messages;
        public bool IsClientConnected => clientController.Client.Item2.IsConnected;
        public bool IsClientAuthenticated
        {
            get
            {
                if (IsClientConnected is false)
                    return false;

                var status = clientController.Client.Item2.IsAuthenticated;
                if (status is false)
                    status = clientController.ClientAuth(UserData.Username, UserData.Password);
                return status;
            }
        }
        public List<MimeMessage> Messages
        {
            get
            {
                if (IsClientAuthenticated is false)
                    return null;

                using var client = clientController.Client.Item2;
                List<MimeMessage> messages = clientController.GetMessages();
                if (messages != null)
                {
                    messages.OrderByDescending(date => date.Date);
                    if (Database.CheckForChanges(UserData.UserID, messages[0].MessageId, messages.Count))
                    {
                        Database.ApplyChanges(messages, UserData.UserID);
                        _messages = messages;
                    }
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
        {
            clientController = new ClientController(Secrets);
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
                Guid temp = Guid.NewGuid();
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

        // Overrides any value that is currently assigned to the variables.
        // Variables are created as "protected" and their value should not be allowed to be viewed in outside classes.
        // Setting a new value is fine as long as it doesn't leak the original value.
        public void SetUserName(string username) => UserData.Username = username;
        public void SetPassword(string password) => UserData.Password = password;

        public void Dispose()
        {
            clientController.Dispose();
            Database.Dispose();
        }
    }
}