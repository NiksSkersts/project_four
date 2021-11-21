using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace LLU.Android.Controllers
{
    public abstract class EmailController
    {
        //Create a new connection with the server.
        //Authentificate and return the client.
        //Instead of crashing the app on failure, give out null, to implement safeguards and fallbacks in-app code. 
        public static Tuple<byte,ImapClient> Connect(string host, int port)
        {
            // Quick intro into resultCode:
            // 0 - All good
            // 1 - Client connection failed
            // 2 - Auth failed

            var client = new ImapClient();
            byte resultCode= 0;
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;
            try
            {
                client.Connect(host, port, SecureSocketOptions.Auto);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                resultCode = 1;
            }
            return Tuple.Create(resultCode, client);
        }
        // Disconnecting just because user inputted wrong credentials is a waste of resources, and can get you banned from the server for a period of time.
        // Keep connection with the server, but try auth again instead.
        public static bool ClientAuth(ImapClient client, string username, string password)
        {
            try
            {
                client.Authenticate(username, password);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

        }
        public static bool DeleteMessages(ImapClient client, List<string> Ids)
        {
            try
            {
                foreach (var id in Ids)
                {
                    client.Inbox.AddFlagsAsync(UniqueId.Parse(id), MessageFlags.Deleted, true);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            return true;

        }
        public static List<MimeMessage> GetMessages(ImapClient client)
        {
            var inbox = AccessMessages(client);
            if(inbox == null) return new();
            return inbox;
        }
        public static List<MimeMessage>? AccessMessages(ImapClient client)
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
        public static bool DisconnectFromServer(ImapClient client)
        {
            try
            {
                client.Disconnect(true);
                client.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            return true;
        }
    }
}