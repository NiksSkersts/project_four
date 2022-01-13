using System;
using System.Threading;
using System.Threading.Tasks;
using Java.IO;
using LLU.Android.Controllers;
using LLU.Models;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Console = System.Console;

namespace LLU.Controllers;

internal interface IController {
    /// <summary>
    ///     Common function to authenticate user in the server. Takes username and password.
    ///     Disconnecting just because user inputted wrong credentials is a waste of resources, and can get you banned from the
    ///     server for a period of time.
    ///     Keep connection with the server, but try auth again instead.
    ///     <para>WARNING: Client should be connected before calling this function!</para>
    /// </summary>
    /// <param name="userData">Consists of Username and Password fields.</param>
    /// <param name="userData.Username">
    ///     Username would normally be written like "example@gmail.com", but LLU uses only the part before
    ///     @.
    /// </param>
    /// <param name="userData.Password">Password....</param>
    /// <param name="client"></param>
    /// <returns></returns>
    object ClientAuth(UserData userData, object client);

    /// <summary>
    ///     Common function to create a connection with the server.
    ///     <param name="data"> Client object. Either IMAP or SMTP.</param>
    ///     <returns>Returns a created client object that has been connected with the server.</returns>
    /// </summary>
    object Connect(object data);
}

/// <summary>
///     Provides the base for SMTP and IMAP controllers
/// </summary>
internal abstract class ControllerBase {
    internal readonly CancellationTokenSource Cancel;
    protected readonly Secrets Secrets;
    internal readonly UserData UserData;
    protected ImapClient? ImapClient;

    /// <summary>
    ///     On new message event -> switch to true;
    /// </summary>
    internal bool MessagesArrived;

    /// <summary>
    /// </summary>
    protected SmtpClient? SmtpClient;

    /// <summary>
    /// </summary>
    /// <param name="cancel"></param>
    /// <param name="secrets"></param>
    /// <param name="userData"></param>
    protected ControllerBase(CancellationTokenSource cancel, Secrets secrets, UserData userData) {
        Cancel = cancel;
        Secrets = secrets;
        UserData = userData;
    }
}

/// <summary>
///     <para>
///         LLU has some kind of security in place that blocks too many attempted Connect() requests - from this app and
///         other third party apps like thunderbird.
///         As far as I am aware, the suggestion is to disconnect or go IDLE on app exit or pause.
///     </para>
///     <para>
///         Imap clients were created with long-lived connections in mind. As far as I am aware, while not in active use,
///         they enter IDLE mode and just sync from time to time to get push notifications.
///     </para>
/// </summary>
internal class ClientController : ControllerBase, IController {
    /// <summary>
    ///     Create an instance of ClientController by providing it with JSON file MailServer and MailPort parameters.
    /// </summary>
    /// <param name="secrets">Company secrets that are given to trusted people in an json file.</param>
    /// <param name="userData">Userdata object.</param>
    public ClientController(Secrets secrets, UserData userData) :
        base(new CancellationTokenSource(), secrets, userData) =>
        ImapClient = (ImapClient) Connect(secrets);

    /// <summary>
    ///     This is a one stop IMAP instance. It's meant to provide an automatic way of handling IMAPClient and connections
    ///     with server.
    ///     It's to avoid unnecessary Client related code execution within other parts of the code that could lead to unknown
    ///     behaviour.
    /// </summary>
    internal ImapClient Client {
        get {
            switch (ImapClient) {
                case null:
                    try {
                        ImapClient = (ImapClient) Connect(Secrets);
                    }
                    catch (Exception e) {
                        Console.WriteLine(e);
                    }

                    break;
                case {IsConnected: false, IsAuthenticated: false}:
                    try {
                        ImapClient = (ImapClient) Connect(Secrets);
                    }
                    catch (Exception e) {
                        Console.WriteLine(e);
                    }

                    break;
                case {IsConnected: true, IsAuthenticated: false}:
                    try {
                        ImapClient = (ImapClient) ClientAuth(UserData, ImapClient);
                    }
                    catch (Exception e) {
                        Console.WriteLine(e);
                    }

                    break;
                case {IsConnected: true, IsAuthenticated: true}:
                    return ImapClient;
            }

            if (ImapClient is null) // Shouldn't happen, but check nonetheless.
                throw new Exception("IMAPClient is null!");
            return ImapClient;
        }
        set => ImapClient = value;
    }

    public object Connect(object data) {
        var temp = (Secrets) data;
        var client = new ImapClient {
            ServerCertificateValidationCallback = (_, _, _, _) => true
        };
        if (!ConnectionController.IsConnectedToInternet) return client;
        try {
            client.Connect(temp.MailServer, temp.MailPort, SecureSocketOptions.Auto, Cancel.Token);
        }
        catch (Exception) {
            //todo implement workarounds for this, e.g offline mode.
            throw new Exception("Failed to Connect to server!");
        }

        return client;
    }

    public object ClientAuth(UserData userData, object client) {
        var imapClient = (ImapClient) client;
        if (imapClient.IsConnected)
            try {
                imapClient.Authenticate(userData.Username, userData.Password, Cancel.Token);
            }
            catch (Exception) {
                return false;
            }

        return imapClient;
    }

    public void Dispose() {
        Client.Dispose();
    }
}

/// <summary>
///     <para>
///         Controller whose main job is to create a connection with a server, send a message using SMTP and dispose of
///         itself.
///     </para>
/// </summary>
internal class SmtpController : ControllerBase, IController {
    /// <summary>
    ///     SMTPClient should be disconnected after sending the message. Please make sure all paths lead to disconnect and
    ///     disposal.
    /// </summary>
    /// <param name="secrets">Company secrets that are given to trusted people in an json file.</param>
    /// <param name="userData"></param>
    public SmtpController(Secrets secrets, UserData userData) : base(new CancellationTokenSource(), secrets, userData) {
        var smtp = new SmtpClient();
        try {
            smtp = (SmtpClient) Connect(secrets);
            smtp = (SmtpClient) ClientAuth(userData, smtp);
        }
        catch (Exception e) {
            // ignored
        }

        SmtpClient = smtp;
    }

    /// <summary>
    ///     Queries if the connection with the server is alright and won't cause any errors related to connection and
    ///     authentication.
    /// </summary>
    internal bool IsOkay => SmtpClient is {IsConnected: true, IsAuthenticated: true};

    public object Connect(object data) {
        SmtpClient client = new();
        client.ServerCertificateValidationCallback = (_, _, _, _) => true;
        try {
            client.Connect(Secrets.MailServer, Secrets.SMTPPort, SecureSocketOptions.StartTls, Cancel.Token);
        }
        catch (Exception) {
            Cancel.Cancel();
        }

        return client;
    }

    public object ClientAuth(UserData userData, object client) {
        var smtp = (SmtpClient) client;
        try {
            smtp.Authenticate(userData.Username, userData.Password, Cancel.Token);
        }
        catch (Exception) {
            // ignored
        }

        return smtp;
    }

    public void Dispose() {
        Cancel.Cancel();
        SmtpClient?.Dispose();
    }

    /// <summary>
    ///     Sends message via SMTP client.
    ///     <para>WARNING: Make sure you dispose of client after using it!</para>
    ///     <para>WARNING: Make sure you Authentificate before calling this function!</para>
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public bool SendMessage(MimeMessage email) {
        var status = false;
        if (SmtpClient is not null)
            try {
                SmtpClient.Send(email);
                SmtpClient.Disconnect(true);
                status = true;
            }
            catch (Exception) {
                status = false;
            }

        return status;
    }
}

/// <summary>
///     Class is built upon already existing ImapClient. By going into IDLE state, client takes up the task of receiving
///     new messages or notifications.
///     <para>
///         When creating an idle client, it should be noted that it's irreversible and ending IDLE state will require
///         client to be disposed of.
///     </para>
///     <para>It should be noted that an INBOX folder should be oppened before going into IDLE.</para>
///     Not yet implemented!
/// </summary>
internal class IdleClientController : ClientController {
    private readonly ClientController _clientControlleŗ;
    private CancellationTokenSource _done;

    /// <summary>
    /// </summary>
    /// <param name="client"></param>
    /// <param name="secrets"></param>
    public IdleClientController(ClientController client, Secrets secrets) : base(secrets, client.UserData) {
        _done = new CancellationTokenSource();
        _clientControlleŗ = client;
        Client = _clientControlleŗ.Client;
        if (Client is {IsConnected: true, IsAuthenticated: true}) {
            Client.Inbox.OpenAsync(FolderAccess.ReadOnly)
                .ContinueWith(second => Client.IdleAsync(_done.Token, _clientControlleŗ.Cancel.Token));
            IdleAsync();
        }
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    private async Task IdleAsync() {
        do {
            try {
                await WaitForNewMessagesAsync();

                if (_clientControlleŗ.MessagesArrived) _clientControlleŗ.MessagesArrived = false;
            }
            catch (OperationCanceledException) {
                break;
            }
        } while (_clientControlleŗ.Cancel.IsCancellationRequested is false);
    }

    /// <summary>
    ///     Note: IMAP servers are only supposed to drop the connection after 30 minutes, so normally
    ///     we'd IDLE for a max of, say, ~29 minutes... but GMail seems to drop idle connections after
    ///     about 10 minutes, so we'll only idle for 9 minutes.
    ///     <para>
    ///         Note: we don't want to spam the IMAP server with NOOP commands, so wait a minute between each NOOP command.
    ///     </para>
    ///     <para>
    ///         Protocol and I/O exceptions often result in the client getting disconnected
    ///     </para>
    /// </summary>
    private async Task WaitForNewMessagesAsync() {
        do {
            _done = new CancellationTokenSource(new TimeSpan(0, 9, 0));

            try {
                if (Client != null && Client.Capabilities.HasFlag(ImapCapabilities.Idle)) {
                    try {
                        await Client.IdleAsync(_done.Token, _clientControlleŗ.Cancel.Token);
                    }
                    finally {
                        _done.Dispose();
                    }
                }
                else {
                    await Task.Delay(new TimeSpan(0, 1, 0), _clientControlleŗ.Cancel.Token);
                    await Client!.NoOpAsync(_clientControlleŗ.Cancel.Token);
                }

                break;
            }
            catch (ImapProtocolException) { }
            catch (IOException) { }
        } while (true);
    }
}