using LLU.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System;
using System.Threading;

namespace LLU.Controllers
{
    internal class SMTPController : Controller,IDisposable 
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
                SmtpClient client;
                if (_client != null)
                    return _client;
                else
                {
                    client = new();
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
                    }
                }
                return Tuple.Create((byte)1, client);
            }
            set
            {
                _client = value;
            }
        }
        public bool ClientAuth(string username, string password)
        {
            if (Client.Item1 == 1) return false;
            if (Client.Item2.IsConnected)
            {
                try
                {
                    cancel = new CancellationTokenSource();
                    Client.Item2.Authenticate(username, password, cancel.Token);
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Dispose();
                    return false;
                }
            }
            return false;
        }
        public bool SendMessage(MimeMessage email, string username, string password)
        {
            try
            {
                var client = Client;
                if (client.Item2.IsConnected)
                {
                    var auth = ClientAuth(username, password);
                    if (auth)
                        client.Item2.SendAsync(email);
                    client.Item2.DisconnectAsync(true);
                    return auth;
                }
                Dispose();
                return false;

            }
            catch
            {
                Dispose();
                return false;
            }
        }
        public new void Dispose()
        {
            cancel.Cancel();
            Client.Item2.Dispose();
        }
    }
}
