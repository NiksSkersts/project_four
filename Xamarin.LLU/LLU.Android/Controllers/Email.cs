using MailKit.Net.Imap;
using MimeKit;
using System.Collections.Generic;
using System.Linq;
using MailKit;
using MailKit.Search;
using MailKit.Security;

#nullable enable
namespace LLU.Android.Controllers
{
    public abstract class Email
    {
        //Get Messages by using an existing client.
        //Disconnect on finish
        public static List<MimeMessage>? GetMessages(string host, int port, string username, string password)
        {
            using ImapClient? client = Connect(host, port, username, password) ?? null;
            if (client == null)
                return null;
            var inbox = AccessInbox(client);
            client.DisconnectAsync(true);
            return inbox;
        }
        public static List<MimeMessage>? GetMessages(ImapClient? client)
        {

            var inbox = AccessInbox(client);
            client.DisconnectAsync(true);
            return inbox;
        }
        public static List<MimeMessage>? AccessInbox(ImapClient client)
        {
            List<MimeMessage> messages = new();
            var state = client.Inbox.Open(FolderAccess.ReadOnly);
            if (state != FolderAccess.None)
            {
                var uids = client.Inbox.Search(SearchQuery.All);
                messages = uids.Select(uid => client.Inbox.GetMessage(uid)).ToList();
            }
            return messages;
        }

        //Create a new connection with the server.
        //Authentificate and return the client.
        //Instead of crashing the app on failure, give out null, to implement safeguards and fallbacks in-app code. 
        public static ImapClient? Connect(string host, int port, string username, string password)
        {
            var client = new ImapClient ();
            try
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                client.Connect(host, port, SecureSocketOptions.Auto);
                client.Authenticate(username, password);
                return client;
            }
            catch (System.Exception e)
            {
                return null;
            }
        }
    }
}