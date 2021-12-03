using Android.App;
using LLU.Models;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LLU.Controllers
{
    internal class ClientController : Controller, IDisposable
    {
        // WARNING: One should be careful with Disconnect(). It's advised to disconnect from the server to free up the socket, but one should not do it too often.
        // LLU has some kind of security in place that blocks too many attempted Connect() requests - from this app and other third party apps like thunderbird.
        // As far as I am aware, the suggestion is to disconnect on app exit or pause.
        // Imap clients were created with long-lived connections in mind. As far as I am aware, while not in active use, they enter IDLE mode and just sync from time to time to get push notifications.
        protected Tuple<byte, ImapClient> _client;
        public Tuple<byte, ImapClient> Client
        {
            get
            {
                if (_client == null || _client.Item2.IsConnected is false)
                    _client = Connect(Host, Port);
                return _client;
            }
            set
            {
                // Make sure setter is not attempting to disconnect on an null client
                if (value is null && _client is null)
                    return;
                if (value is null && Client.Item2.IsConnected)
                // Value == null means that the program is requesting client to be disposed of.
                // Destroying client without disconnecting is strongly discouraged!
                {
                    try
                    {
                        Client.Item2.DisconnectAsync(true);
                        Client.Item2.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
                else
                    _client = value;
            }
        }
        

        public ClientController(Secrets secrets)
        {
            Host = secrets.MailServer;
            Port = secrets.MailPort;
        }

        //Create a new connection with the server.
        //Authentificate and return the client.
        //Instead of crashing the app on failure, give out null, to implement safeguards and fallbacks in-app code. 
        private Tuple<byte, ImapClient> Connect(string host, int port)
        {
            cancel = new System.Threading.CancellationTokenSource();
            // Quick intro into resultCode:
            // 0 - All good
            // 1 - Client connection failed
            // 2 - Auth failed

            var client = new ImapClient();
            byte resultCode = 0;
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;
            try
            {
                client.Connect(host, port, SecureSocketOptions.Auto, cancel.Token);
                
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
        public bool ClientAuth(string username, string password)
        {
            if (Client.Item1 == 1) return false;
            if (Client.Item2.IsConnected)
            {
                try
                {
                    Client.Item2.Authenticate(username, password, cancel.Token);
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return false;
                }
            }
            return false;
        }

        public bool DeleteMessages(ImapClient client, List<string> Ids)
        {
            try
            {
                foreach (var id in Ids)
                {
                    client.Inbox.AddFlagsAsync(UniqueId.Parse(id), MessageFlags.Deleted, true, cancel.Token);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            return true;

        }

        public ObservableCollection<MimeMessage> GetMessages()
        {
            if (Client.Item1 != 0)
                return null;

            var inbox = AccessMessages(Client.Item2);
            if (inbox == null) return new();
            return inbox;
        }

        private ObservableCollection<MimeMessage> AccessMessages(ImapClient client)
        {
            ObservableCollection<MimeMessage> messages = new();
            var state = client.Inbox.Open(FolderAccess.ReadOnly, cancel.Token);
            if (state != FolderAccess.None)
            {
                var uids = client.Inbox.Search(SearchQuery.All);
                foreach (var uid in uids)
                    messages.Add(client.Inbox.GetMessage(uid));
            }
            return messages;
        }
        public new void Dispose()
        {
            Client.Item2.Dispose();
        }
    }
}
