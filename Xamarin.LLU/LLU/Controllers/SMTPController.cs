using System;
using System.Threading;
using LLU.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace LLU.Controllers;

internal class SmtpController : IController {
    private readonly CancellationTokenSource _cancel;
    private readonly SmtpClient _smtpClient;
    private readonly Secrets _secrets;
    internal bool IsOkay => _smtpClient is {IsConnected: true, IsAuthenticated: true};
    /// <summary>
    ///     SMTPClient should be disconnected after sending the message. Please make sure all paths lead to disconnection and
    ///     disposal.
    /// </summary>
    /// <param name="secrets">Company secrets that are given to trusted people in an json file.</param>
    /// <param name="userData"></param>
    public SmtpController(Secrets secrets, UserData userData) {
        _secrets = secrets;
        _cancel = new CancellationTokenSource();
        var smtp = new SmtpClient();
        try {
            smtp = (SmtpClient) Connect(secrets);
            smtp = (SmtpClient) ClientAuth(userData, smtp);
        }
        catch (Exception e) {
            // ignored
        }
        _smtpClient = smtp;
    }
    public object Connect(object data) {
        SmtpClient client = new();
        client.ServerCertificateValidationCallback = (_, _, _, _) => true;
        try {
            client.Connect(_secrets.MailServer, _secrets.SMTPPort, SecureSocketOptions.StartTls, _cancel.Token);
        }
        catch (Exception e) {
            _cancel.Cancel();
        }
        return client;
    }

    public object ClientAuth(UserData userData, object client) {
        var smtp = (SmtpClient) client;
        try {
            smtp.Authenticate(userData.Username, userData.Password, _cancel.Token);
        }
        catch (Exception e) {
            // ignored
        }

        return smtp;
    }

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
            _smtpClient!.Send(email);
            _smtpClient.Disconnect(true);
            status = true;
        }
        catch (Exception e) {
            status = false;
        }

        return status;
    }
    public void Dispose() {
        _cancel.Cancel();
        _smtpClient?.Dispose();
    }
}