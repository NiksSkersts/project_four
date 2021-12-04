using LLU.Controllers;
using LLU.Models;
using MimeKit;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace LLU.Android.LLU.Models
{
    internal class EmailUser : User, IDisposable
    {
        public ClientController clientController;
        private SMTPController smtpController;
        protected ObservableCollection<MimeMessage> _messages;
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
        public ObservableCollection<MimeMessage> Messages
        {
            get
            {
                if (IsClientAuthenticated is false)
                    return null;

                using var client = clientController.Client.Item2;
                var messages = clientController.GetMessages();
                if (messages != null)
                {
                    messages.OrderBy(date => date.Date);
                    if (Database.CheckForChanges(UserData.UserID, messages[0].MessageId, messages.Count))
                        Database.ApplyChanges(messages, UserData.UserID);
                    _messages = messages;
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
                    foreach (var message in value)
                    {
                        _messages.Add(message);
                    }
            }
        }

        private EmailUser()
        {
            clientController = new ClientController(Secrets);
            smtpController = new SMTPController(Secrets);
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
                // Notable examples are students who were employees of LLU first, before they began to study.
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
        public bool CreateAndSendMessage(string receiversString,string subject, string body)
        {
            var email = new MimeMessage();
            var receivers = receiversString.Split(';');
            var full_list = receivers.Where(mailaddress => mailaddress.Contains('@')).ToList();
            for(int i = 0;i<full_list.Count-1; i++)
                full_list[i].Trim();
            try
            {
                email.From.Add(MailboxAddress.Parse($"{UserData.Username}@llu.lv"));
                foreach (var receiver in full_list)
                    email.To.Add(MailboxAddress.Parse(receiver));
                email.Subject = subject;
                email.Body = new TextPart(MimeKit.Text.TextFormat.Text) { Text = body };
                var auth = smtpController.ClientAuth(UserData.Username, UserData.Password);
                bool status = false;
                if(auth)
                    status = smtpController.SendMessage(email);
                smtpController.Dispose();
                return status && auth;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            clientController.Dispose();
            Database.Dispose();
        }
    }
}