using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using LLU.Android.Controllers;
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
    private ImapClient _imapClient;
    private readonly Secrets _secrets;
    /// <summary>
    ///     On new message event -> switch to true;
    /// </summary>
    internal bool MessagesArrived = false;
    /// <summary>
    ///     This is a one stop IMAP instance. It's meant to provide an automatic way of handling IMAPClient and connections
    ///     with server.
    ///     It's to avoid unnecessary Client related code execution within other parts of the code that could lead to unknown
    ///     behaviour.
    /// </summary>
    internal ImapClient Client {
        get {
            if (_imapClient.IsConnected is false)
                try {
                    _imapClient = (ImapClient) Connect(_secrets);
                }
                catch (Exception e) {
                    Console.WriteLine(e);
                }
            return _imapClient;
        }
        set => _imapClient = value;
    }

    /// <summary>
    ///     Create an instance of ClientController by providing it with JSON file MailServer and MailPort parameters.
    /// </summary>
    /// <param name="secrets">Company secrets that are given to trusted people in an json file.</param>
    public ClientController(Secrets secrets){
        Cancel = new CancellationTokenSource();
        _imapClient = (ImapClient) Connect(secrets);
        _secrets = secrets;
    }
    public object Connect(object data) {
        var temp = (Secrets) data;
        var client = new ImapClient() {
            ServerCertificateValidationCallback = (s, c, h, e) => true
        };
        if (!ConnectionController.IsConnectedToInternet) return client;
        try {
            client.Connect(temp.MailServer, temp.MailPort,SecureSocketOptions.Auto, Cancel.Token);
        }
        catch (Exception) {
            //ignored
        }

        return client;
    }
    public object ClientAuth(UserData userData, object client) {
        var imapClient = (ImapClient) client;
        if (imapClient.IsConnected) {
            try {
                imapClient.Authenticate(userData.Username, userData.Password, Cancel.Token);
            }
            catch (Exception) {
                return false;
            }
        }

        return imapClient;
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
    public MimeMessage? GetMessageFromServer(string id) {
        {
            UniqueId queryResult;
            try {
                Client.Inbox.Open(FolderAccess.ReadOnly);
                var message = Client.Inbox.GetMessage(UniqueId.Parse(id));
                Client.Inbox.Close();
                return message;
            }
            catch (Exception e) {
                // ignored
            }
        }
        return null;
    }
    public ObservableCollection<MimeMessage> GetMessages() =>  AccessMessages(Client);

    private ObservableCollection<MimeMessage> AccessMessages(ImapClient client) {
        ObservableCollection<MimeMessage> messages = new();
        if (!ConnectionController.IsConnectedToInternet || !Client.IsAuthenticated) return messages;
        var state = client.Inbox.Open(FolderAccess.ReadOnly, Cancel.Token);
        if (state is FolderAccess.None) return messages;
        var uids = client.Inbox.Search(SearchQuery.All.And(SearchQuery.NotFlags(MessageFlags.Deleted)));
        foreach (var uid in uids)
            messages.Add(client.Inbox.GetMessage(uid));
        return messages;
    }
    public void Dispose() {
        Client.Dispose();
    }
}