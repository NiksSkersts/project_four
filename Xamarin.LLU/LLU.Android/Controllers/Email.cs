using Java.Lang;
using LLU.Android.LLU.Models;
using LLU.LLU.Models;
using MailKit.Net.Imap;
using MailKit.Net.Pop3;
using MimeKit;
using System.Collections.Generic;

namespace LLU.Android.Controllers
{
    public abstract class Email
    {
        public static bool CheckConnection(string host,int port, string username, string password)
        {
            bool connected = false;
            using var client = new ImapClient();
            client.Connect(host, port, false);
            client.Authenticate(username, password);
            connected = client.IsConnected;
            client.Disconnect(true);
            return connected;
        }
        public static List<MimeMessage> GetMessages(string host, int port, string username, string password)
        {
            List<MimeMessage> _messages = new();
            using var client = new ImapClient();
            client.Connect(host, port, false);
            client.Authenticate(username, password);
            var inbox = client.Inbox;
            client.Disconnect(true);

            for (var i = 0; i < inbox.Count; i++)
            {
                _messages.Add(inbox.GetMessage(i));
            }

            return _messages;
        }
    }
}