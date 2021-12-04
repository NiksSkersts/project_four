using LLU.Models;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

namespace LLU.Controllers;

/// <summary>
///WARNING: One should be careful with Disconnect(). It's advised to disconnect from the server to free up the socket, but one should not do it too often.
///LLU has some kind of security in place that blocks too many attempted Connect() requests - from this app and other third party apps like thunderbird.
///As far as I am aware, the suggestion is to disconnect on app exit or pause.
///<para>Imap clients were created with long-lived connections in mind. As far as I am aware, while not in active use, they enter IDLE mode and just sync from time to time to get push notifications.</para>
/// </summary>
internal class ClientController : IController {
    protected string Host { get; set; }
    protected int Port { get; set; }
    public CancellationTokenSource cancel;
    public CancellationTokenSource done;
    protected ImapClient _client;
    /// <summary>
    /// This is a one stop IMAP instance. It's meant to provide an automatic way of handling IMAPClient and connections with server.
    /// It's to avoid unnecesary Client related code execution within other parts of the codethat could lead to unknown behaviour. 
    /// </summary>
    public Tuple<byte, ImapClient> Client {
        get {
            byte resultCode = 0;
            if (_client == null || _client.IsConnected is false) {
                try {
                    _client = Connect(Host, Port);
                }
                catch (Exception e) {
                    Console.WriteLine(e);
                    resultCode = 1;
                }
            }
            return Tuple.Create(resultCode, _client);
        }
    }
    /// <summary>
    /// Create an instance of ClientController by providing it with JSON file MailServer and MailPort parameters.
    /// </summary>
    /// <param name="secrets">Company secrets that are given to trusted people in an json file.</param>
    public ClientController(Secrets secrets) {
        Host = secrets.MailServer;
        Port = secrets.MailPort;
    }

    public byte Connect() => Client.Item1;
    /// <summary>
    /// Function creates a new connection with the server and return error code.
    /// </summary>
    /// <param name="host">Server host address like: imap.google.com</param>
    /// <param name="port">Server port. Default IMAP:993</param>
    /// <returns></returns>
    private ImapClient Connect(string host, int port) {
        cancel = new CancellationTokenSource();
        var client = new ImapClient {
            ServerCertificateValidationCallback = (s, c, h, e) => true
        };
        client.Connect(host, port, SecureSocketOptions.Auto, cancel.Token);
        return client;
    }
    /// <summary>
    /// Disconnecting just because user inputted wrong credentials is a waste of resources, and can get you banned from the server for a period of time.
    /// Keep connection with the server, but try auth again instead.
    /// </summary>
    /// <param name="username">Username would normally be written like "example@gmail.com", but LLU uses only the part before @.</param>
    /// <param name="password">Password....</param>
    /// <returns></returns>
    public bool ClientAuth(string username, string password) {
        if (Client.Item1 == 1)
            return false;
        if (Client.Item2.IsConnected) {
            try {
                Client.Item2.Authenticate(username, password, cancel.Token);
                return true;
            }
            catch (Exception) {
                return false;
            }
        }
        return false;
    }
    public bool DeleteMessages(ImapClient client, List<string> Ids) {
        try {
            foreach (var id in Ids) {
                client.Inbox.AddFlagsAsync(UniqueId.Parse(id), MessageFlags.Deleted, true, cancel.Token);
            }
        }
        catch (Exception) {
            return false;
        }
        return true;

    }
    public ObservableCollection<MimeMessage> GetMessages() => Client.Item1 != 0 ? null : AccessMessages(Client.Item2);
    private ObservableCollection<MimeMessage> AccessMessages(ImapClient client) {
        ObservableCollection<MimeMessage> messages = new();
        var state = client.Inbox.Open(FolderAccess.ReadOnly, cancel.Token);
        if (state != FolderAccess.None) {
            var uids = client.Inbox.Search(SearchQuery.All.And(SearchQuery.NotFlags(MessageFlags.Deleted)));
            foreach (var uid in uids)
                messages.Add(client.Inbox.GetMessage(uid));
        }
        return messages;
    }
    public void Dispose() => Client.Item2.Dispose();
}
