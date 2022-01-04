using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;
using LLU.Android.Controllers;
using LLU.Controllers;
using LLU.Models;
using MailKit;
using MailKit.Net.Imap;
using MimeKit;

namespace LLU.Android.LLU.Models;

/// <summary>
/// Defines the main class that stores and deals with email data.
/// <para>It's responsible for an easy access to inbox, messages and creation of SMTP and IMAP clients.</para>
/// </summary>
internal class EmailUser : User {
    private readonly ClientController? _clientController;
    private ObservableCollection<DatabaseData> _messages;
    private readonly IMailFolder? _inbox = null;
    private List<string> _idsInMessages = new();
    
    /// <summary>
    /// 
    /// </summary>
    private IMailFolder Inbox {
        get {
            var mailFolder = _inbox;
            if (_clientController is not null) {
                
                if (mailFolder is null) {
                    if (_clientController.Client is{ IsConnected:true, IsAuthenticated:true}) {
                        mailFolder = _clientController.Client.Inbox;
                        mailFolder.CountChanged += OnCountChanged;
                        mailFolder.MessageExpunged += OnMessageExpunged;
                        mailFolder.MessageFlagsChanged += OnMessageFlagsChanged;
                    }
                }
                else {
                    if (mailFolder.IsOpen) {
                        if (mailFolder.Access is FolderAccess.None or FolderAccess.ReadOnly) {
                            mailFolder.Close();
                            mailFolder.Open(FolderAccess.ReadWrite);
                        }
                    }
                    else {
                        mailFolder.Open(FolderAccess.ReadWrite);
                    }
                }
            }
            else {
                throw new Exception("_clientController is null!");
            }
            return mailFolder;
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    public ObservableCollection<DatabaseData> Messages {
        get {
            ObservableCollection<DatabaseData> messages = new();
            
            if (_clientController is not null) {
                if (_clientController.Client is {IsConnected: false, IsAuthenticated: false}) return _messages;
                
                var fetched = Inbox.Fetch(0, -1,
                    MessageSummaryItems.UniqueId | MessageSummaryItems.Size | MessageSummaryItems.Flags);

                var isThereAnyChanges = false;
                
                foreach (var item in fetched) {
                    if (_idsInMessages.Exists(q => q.Equals(item.UniqueId.ToString()))) continue;
                    
                    var message = Inbox.GetMessage (item.UniqueId);
                    var hasRead = false;
                    var hasBeenDeleted = false;
                    if (item.Flags is not (null or MessageFlags.None)) {
                        if (item.Flags.Value == MessageFlags.Seen) {
                            hasRead = true;
                        }

                        if (item.Flags.Value == MessageFlags.Deleted) {
                            hasBeenDeleted = true;
                        }
                    }
                    
                    messages.Add(DataController.ConvertFromMime(message,item.UniqueId,item.Folder.Name,hasRead,hasBeenDeleted));
                    isThereAnyChanges = true;
                }

                if (isThereAnyChanges)
                    Database.UpdateDatabase(messages);
            }
            _messages = messages;
            return _messages;
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    private EmailUser() {
        _messages = new ObservableCollection<DatabaseData>();
        Messages.CollectionChanged += MessagesOnCollectionChanged;
        _clientController = new ClientController(Secrets);
    }

    /// <summary>
    ///     Constructor creates a new EmailUser on app launch. Assumption remains that this constructor is used at first-launch
    ///     of the application or when app fails to connect with using database data (credential change).
    ///     <para>
    ///         WARNING: This does not validate connection. Validity should be done within LoginActivity and AccountManager.
    ///         This class, for all intents and purposes, only is the middle layer between Database, Server and functionality.
    ///     </para>
    ///     <para name="userid">
    ///         WARNING: One should avoid validating by char count(). Not all LLU employees or students have matr.code.
    ///         Notable examples are students who were employees of LLU first, before they began to study.
    ///     </para>
    /// </summary>
    /// <param name="username">
    ///     WARNING: LLU only accepts username that contains the part before the @ (matr.code), while other
    ///     email services accept either both or full username with domain.
    /// </param>
    /// <param name="password">Just a password. Make sure it is not null.</param>
    public EmailUser(string username, string password) : this() {
        username = username.ToLower().Contains("@llu.lv") ? username.Split('@')[0] : username;
        UserData = new UserData {
            Username = username,
            Password = password
        };
        if (_clientController != null) {
            _clientController.Client = (ImapClient) _clientController.ClientAuth(UserData, _clientController.Client);
            foreach (var id in Messages) {
                _idsInMessages.Add(id.Id);
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="toText"></param>
    /// <param name="subjectText"></param>
    /// <param name="bodyText"></param>
    /// <returns></returns>
    public MimeMessage CreateEmail(string toText, string? subjectText, string? bodyText) 
        => DataController.CreateEmail(toText, UserData.Username, subjectText, bodyText);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public bool SendEmail(MimeMessage email) {
        var status = false;
        var auth = false;
        try {
            var smtpController = new SmtpController(Secrets,new UserData{
                Username = UserData.Username,
                Password = UserData.Password
            });
            auth = smtpController.IsOkay;
            if (auth) {
                status = smtpController.SendMessage(email);
                smtpController.Dispose();
                if (status)
                    Inbox.GetSubfolderAsync("Sent", _clientController.Cancel.Token)
                        .ContinueWith(second => second.Result.Append(email), _clientController.Cancel.Token);
            }
        }
        catch (Exception e) {
            return false;
        }
        return status && auth;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="uniqueId"></param>
    /// <returns></returns>
    public MimeMessage? GetMessageFromServer(string uniqueId) {
        try {
            return Inbox.GetMessage(UniqueId.Parse(uniqueId));
        }
        catch (Exception e) {
            // ignored
        }

        return null;
    }
    private static void OnMessageFlagsChanged(object sender, MessageFlagsChangedEventArgs e) {
        var folder = (ImapFolder) sender;
    }
    private void MessagesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
    }
    // todo implement expunge function
    /// <summary>
    ///     Note: If you are keeping a local cache of message information (e.g. MessageSummary data) for the folder, then
    ///     you'll need to remove the message at e.Index.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnMessageExpunged(object sender, MessageEventArgs e) {
        return;
        var folder = (ImapFolder) sender;

        if (e.Index < _messages.Count)
            _messages.RemoveAt(e.Index);
        else
            Console.WriteLine("{0}: message #{1} has been expunged.", folder, e.Index);
    }
    //todo implement better way to get new emails.
    /// <summary>
    ///     <para>
    ///         Note: because we are keeping track of the MessageExpunged event and updating our
    ///         'messages' list, we know that if we get a CountChanged event and folder.Count is
    ///         larger than messages.Count, then it means that new messages have arrived.
    ///     </para>
    ///     Note: your first instinct may be to fetch these new messages now, but you cannot do
    ///     that in this event handler (the ImapFolder is not re-entrant).
    ///     Instead, cancel the `done` token and update our state so that we know new messages
    ///     have arrived. We'll fetch the summaries for these new messages later...
    ///     <para>
    ///     </para>
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnCountChanged(object sender, EventArgs e) {
        return;
        var folder = (ImapFolder) sender;
        if (folder.Count <= _messages.Count) return;
        var arrived = folder.Count - _messages.Count;
        _clientController.MessagesArrived = true;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="uniqueId"></param>
    /// <param name="flags"></param>
    internal void SetMessageFlags(string uniqueId, MessageFlags flags) {
        var uid = UniqueId.Parse(uniqueId);
        Inbox.SetFlags(uid, flags,true,CancellationToken.None);
    }
    /// <summary>
    /// 
    /// </summary>
    public new void Dispose() {
        _clientController?.Dispose();
        Database.Dispose();
    }


}