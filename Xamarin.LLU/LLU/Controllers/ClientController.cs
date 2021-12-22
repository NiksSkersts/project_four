using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using LLU.Models;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using MimeKit;

namespace LLU.Controllers;

/// <summary>
///     WARNING: One should be careful with Disconnect(). It's advised to disconnect from the server to free up the socket,
///     but one should not do it too often.
///     LLU has some kind of security in place that blocks too many attempted Connect() requests - from this app and other
///     third party apps like thunderbird.
///     As far as I am aware, the suggestion is to disconnect on app exit or pause.
///     <para>
///         Imap clients were created with long-lived connections in mind. As far as I am aware, while not in active use,
///         they enter IDLE mode and just sync from time to time to get push notifications.
///     </para>
/// </summary>
internal class ClientController : IController {
    public readonly CancellationTokenSource Cancel;
    internal ImapClient? ImapClient;

    /// <summary>
    ///     On new message event -> switch to true;
    /// </summary>
    internal bool MessagesArrived = false;

    protected ClientController() => Cancel = new CancellationTokenSource();

    /// <summary>
    ///     Create an instance of ClientController by providing it with JSON file MailServer and MailPort parameters.
    /// </summary>
    /// <param name="secrets">Company secrets that are given to trusted people in an json file.</param>
    public ClientController(Secrets secrets) : this() {
        Host = secrets.MailServer;
        Port = secrets.MailPort;
    }

    private string Host { get; } = string.Empty;
    private int Port { get; }

    internal IMailFolder? Inbox => Client.Item1 is 0 ? Client.Item2!.Inbox : null;

    /// <summary>
    ///     This is a one stop IMAP instance. It's meant to provide an automatic way of handling IMAPClient and connections
    ///     with server.
    ///     It's to avoid unnecesary Client related code execution within other parts of the code that could lead to unknown
    ///     behaviour.
    /// </summary>
    public Tuple<byte, ImapClient?> Client {
        get {
            byte resultCode = 0;
            if (ImapClient == null || ImapClient.IsConnected is false)
                try {
                    ImapClient = Connect(Host, Port);
                }
                catch (Exception e) {
                    Console.WriteLine(e);
                    resultCode = 1;
                }

            return Tuple.Create(resultCode, ImapClient);
        }
        private set => ImapClient = value.Item2;
    }

    public bool ClientAuth(UserData userData) {
        if (Client.Item1 == 1 || Client.Item2?.IsConnected is false or null)
            return false;
        try {
            Client.Item2.Authenticate(userData.Username, userData.Password, Cancel.Token);
            return true;
        }
        catch (Exception) {
            return false;
        }
    }

    public byte Connect(object data) {
        var temp = (Secrets) data;
        ImapClient? client = null;
        byte resultCode;
        try {
            client = Connect(temp.MailServer, temp.MailPort);
            resultCode = 0;
        }
        catch (Exception) {
            resultCode = 1;
        }

        Client = Tuple.Create(resultCode, client);
        return resultCode;
    }

    public void Dispose() {
        Client.Item2?.Dispose();
    }

    public bool ClientAuth(string username, string password) =>
        ClientAuth(new UserData {Username = username, Password = password});

    /// <summary>
    ///     Function creates a new connection with the server and return error code.
    /// </summary>
    /// <param name="host">Server host address like: imap.google.com</param>
    /// <param name="port">Server port. Default IMAP:993</param>
    /// <returns></returns>
    private ImapClient Connect(string host, int port) {
        var client = new ImapClient {
            ServerCertificateValidationCallback = (s, c, h, e) => true
        };
        client.Connect(host, port, SecureSocketOptions.Auto, Cancel.Token);
        return client;
    }

    public bool DeleteMessages(ImapClient client, List<string> Ids) {
        try {
            foreach (var id in Ids)
                client.Inbox.AddFlagsAsync(UniqueId.Parse(id), MessageFlags.Deleted, true, Cancel.Token);
        }
        catch (Exception) {
            return false;
        }

        return true;
    }

    public ObservableCollection<MimeMessage> GetMessages() =>
        Client.Item1 is not 0 ? new ObservableCollection<MimeMessage>()
        : Client.Item2 is null ? new ObservableCollection<MimeMessage>()
        : AccessMessages(Client.Item2);

    private ObservableCollection<MimeMessage> AccessMessages(ImapClient client) {
        ObservableCollection<MimeMessage> messages = new();
        var state = client.Inbox.Open(FolderAccess.ReadOnly, Cancel.Token);
        if (state is not FolderAccess.None) {
            var uids = client.Inbox.Search(SearchQuery.All.And(SearchQuery.NotFlags(MessageFlags.Deleted)));
            foreach (var uid in uids)
                messages.Add(client.Inbox.GetMessage(uid));
        }

        return messages;
    }
}