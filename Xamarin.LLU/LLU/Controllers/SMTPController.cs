using LLU.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System;
using System.Threading;

namespace LLU.Controllers;

internal class SMTPController : IController {
    protected string Host { get; set; }
    protected int Port { get; set; }
    public CancellationTokenSource cancel;
    public CancellationTokenSource done;
    protected SmtpClient _client;
    /// <summary>
    /// SMTPclient should be disconnected after sending the message. Please make sure all paths lead to disconnection and disposal.
    /// </summary>
    /// <param name="secrets">Company secrets that are given to trusted people in an json file.</param>
    public SMTPController(Secrets secrets) {
        Host = secrets.MailServer;
        Port = secrets.SMTPPort;
    }
    public Tuple<byte, SmtpClient> Client {
        get {
            byte code = 0;
            SmtpClient client = null;
            if (_client != null) {
                if (_client.IsConnected)
                    client = _client;
            }
            else
                client = Connect(ref code);
            return Tuple.Create(code, client);
            #region InnerFunctions
            SmtpClient Connect(ref byte code) {
                SmtpClient client = new();
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                try {
                    client.Connect(Host, Port, SecureSocketOptions.StartTls, cancel.Token);
                }
                catch (Exception e) {
                    Console.WriteLine(e);
                    code = 1;
                }
                return client;
            }
            #endregion
        }
    }
    public byte Connect() => Client.Item1;
    public bool ClientAuth(string username, string password) {
        if (Connect() is 1)
            return false;
        cancel = new CancellationTokenSource();
        try {
            Client.Item2.Authenticate(username, password, cancel.Token);
        }
        catch (Exception e) {
            Console.WriteLine(e);
        }
        return Client.Item2.IsAuthenticated;
    }
    /// <summary>
    /// Sends message via SMTP client.
    /// <para>WARNING: Make sure you dispose of client after using it!</para>
    /// <para>WARNING: Make sure you Authentificate before calling this function!</para>
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public bool SendMessage(MimeMessage email) {
        var auth = false;
        if (Connect() is 1 || Client.Item2.IsAuthenticated is false)
            return auth;
        Client.Item2.SendAsync(email).ContinueWith(x => Client.Item2.DisconnectAsync(true));
        return auth;
    }
    public void Dispose() {
        Client.Item2.Dispose();
        cancel.Cancel();
    }
}
