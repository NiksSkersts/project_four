using System;
using System.Threading;
using System.Threading.Tasks;
using LLU.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace LLU.Controllers;

internal class SmtpController : IController {
    private CancellationTokenSource _cancel;
    private SmtpClient? _smtpClient;

    /// <summary>
    ///     SMTPclient should be disconnected after sending the message. Please make sure all paths lead to disconnection and
    ///     disposal.
    /// </summary>
    /// <param name="secrets">Company secrets that are given to trusted people in an json file.</param>
    public SmtpController(Secrets secrets) {
        _cancel = new CancellationTokenSource();
        Connect(secrets);
    }

    private string Host { get; set; } = string.Empty;
    private int Port { get; set; }

    private Tuple<byte, SmtpClient> Client {
        get {
            byte code = 0;

            if (_smtpClient is {IsConnected: true})
                return Tuple.Create(code,_smtpClient);
            
            SmtpClient client = new();
            _cancel = new CancellationTokenSource();
            client.ServerCertificateValidationCallback = (_, _, _, _) => true;
            try {
                client.Connect(Host, Port, SecureSocketOptions.StartTls, _cancel.Token);
            }
            catch (Exception e) {
                _cancel.Cancel();
                code = 1;
            }

            _smtpClient = client;
            return Tuple.Create(code, client);
        }
    }


    public bool ClientAuth(UserData userData) => ClientAuth(userData.Username, userData.Password);

    public void Dispose() {
        Client.Item2.Dispose();
        _cancel.Cancel();
    }

    public byte Connect(object data) {
        var temp = (Secrets) data;
        Host = temp.MailServer;
        Port = temp.SMTPPort;
        return 0;
    }

    public bool ClientAuth(string username, string password) {
        if (Client.Item2.IsConnected is false)
            return false;
        try {
            Client.Item2.Authenticate(username, password, _cancel.Token);
        }
        catch (Exception e) {
            return false;
        }

        return Client.Item2.IsAuthenticated;
    }

    //todo fix SendMessage function. It sends the message successfully, yet returns false...
    /// <summary>
    ///     Sends message via SMTP client.
    ///     <para>WARNING: Make sure you dispose of client after using it!</para>
    ///     <para>WARNING: Make sure you Authentificate before calling this function!</para>
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public bool SendMessage(MimeMessage email) {
        bool status;
        try {
            Client.Item2.Send(email);
            Client.Item2.Disconnect(true);
            status = true;
        }
        catch (Exception e) {
            status = false;
        }
        return status;
    }
}