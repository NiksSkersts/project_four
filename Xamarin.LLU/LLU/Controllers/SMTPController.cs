using LLU.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System;
using System.Threading;

namespace LLU.Controllers
{
    internal class SMTPController : Controller, IDisposable
    {
        protected Tuple<byte, SmtpClient> _client;

        //Unlike IMAPClient, SMTPclient should be disconnected after sending the message. Please make sure all paths leads to disconnection and Dispose()
        public SMTPController(Secrets secrets)
        {
            Host = secrets.MailServer;
            Port = secrets.SMTPPort;
        }

        public Tuple<byte, SmtpClient> Client
        {
            get
            {
                byte code = 0;
                if (_client != null)
                    return _client;
                SmtpClient client = new();
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                try
                {
                    client.Connect(Host, Port, SecureSocketOptions.StartTls);
                    _client = Tuple.Create(code, client);
                    return _client;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    code = 1;
                    return Tuple.Create<byte, SmtpClient>(code, null);
                }
            }
        }
        public bool ClientAuth(string username, string password)
        {
            if (Client.Item1 == 1) return false;
            try
            {
                cancel = new CancellationTokenSource();
                Client.Item2.Authenticate(username, password, cancel.Token);
                return Client.Item2.IsAuthenticated;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
        public bool SendMessage(MimeMessage email, string username, string password)
        {
            var client = Client;
            var auth = false;
            if (client.Item1!=1 && client.Item2.IsConnected)
            {
                try
                {
                    auth = ClientAuth(username, password);
                    if (auth)
                        client.Item2.SendAsync(email);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                Client.Item2.DisconnectAsync(true);
            }
            Dispose();
            return auth;
        }
        public new void Dispose()
        {
            Client.Item2.Dispose();
            cancel.Cancel();
        }
    }
}
