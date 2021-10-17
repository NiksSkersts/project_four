using Java.Lang;
using LLU.Android.LLU.Models;
using MailKit.Net.Imap;
using MailKit.Net.Pop3;

namespace LLU.Android.Controllers
{
    public abstract class Email
    {
        public MailKit.MailStore GetStore(string host,int port, string username, string password, EmailProtocol protocol)
        {

            if (protocol == EmailProtocol.Pop3)
            {
            }
            Pop3Client ConnectToPop()
            {
                var client = new Pop3Client();
                client.Connect(host, port, false);
                client.Authenticate(username, password);
                var inbox = client.IsConnected;
                return inbox ? client : null;
            }

            return new ImapClient();
        }

        public static bool CheckConnection(string host,int port, string username, string password, EmailProtocol protocol)
        {
            bool connected = false;
            switch (protocol)
            {
                case EmailProtocol.Pop3:
                {
                    using var client = new Pop3Client();
                    client.Connect(host, port, false);
                    client.Authenticate(username, password);
                    connected = client.IsConnected;
                    client.Disconnect(true);
                    break;
                }
                case EmailProtocol.Imap:
                {
                    using var client = new ImapClient();
                    client.Connect(host, port, false);
                    client.Authenticate(username, password);
                    connected = client.IsConnected;
                    client.Disconnect(true);
                    break;
                }
                default:
                    throw new Exception("No protocol provided!");
            }
            return connected;
        }
    }
}