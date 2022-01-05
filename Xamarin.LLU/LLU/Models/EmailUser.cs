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
using Unity;

namespace LLU.Android.LLU.Models;

/// <summary>
/// Defines the main class that stores and deals with email data.
/// <para>It's responsible for an easy access to inbox, messages and creation of SMTP and IMAP clients.</para>
/// </summary>
internal class EmailUser : User {
    private readonly ClientController? _clientController;
    private ObservableCollection<DatabaseData> _messages;
    private IMailFolder? _inbox;
    private List<string> _idsInMessages = new();
    
    //todo implement better way to get new emails.
    private IMailFolder Inbox {
        get {
            var mailFolder = _inbox;
            if (_clientController is not null) {
            sec_check:
                if (_clientController.Client.IsConnected) {
                    if (_clientController.Client.IsAuthenticated) {
                        if (mailFolder is null) {
                            mailFolder = _clientController.Client.Inbox;
                            
                            mailFolder.CountChanged += (sender, args) => {
                                var folder = (ImapFolder) sender;
                                var inboxCount = _messages.Count;
                                if (folder.Count <= inboxCount) return;
                                var arrivedMessageCount = folder.Count - _messages.Count;
                                _clientController!.MessagesArrived = true;
                            };
                            
                            mailFolder.MessageExpunged += (object sender, MessageEventArgs e) => {
                                if (e.Index < _messages.Count)
                                    _messages.RemoveAt(e.Index);
                            };
                            mailFolder.MessageFlagsChanged += (sender, args) => {
                                foreach (var message in _messages) {
                                        if (!message.UniqueId.Equals(args.UniqueId.ToString())) continue;
                                        switch (args.Flags) {
                                            case MessageFlags.None:
                                                break;
                                            case MessageFlags.Seen:
                                                message.NewFlag = true;
                                                break;
                                        }
                                }
                            };
                            
                            mailFolder.MessagesVanished += (sender, args) => {
                                foreach (var uid in args.UniqueIds) {
                                    foreach (var message in _messages) {
                                        if (!message.UniqueId.Equals(uid.ToString())) continue;
                                        var res = _messages.Remove(message);
                                        var res1 = _idsInMessages.Remove(message.Id);
                                    }
                                }
                            };
                        }
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
                    else {
                        _clientController.ClientAuth(UserData!,_clientController.Client);
                        goto sec_check;
                    }
                }
                
            }
            else {
                throw new Exception("_clientController is null!");
            }

            _inbox = mailFolder;
            return mailFolder!;
        }
    }
    
    /// <summary>
    /// <para>
    /// Main goal is to maintain a single entity that deals with getting messages, instead of doing it all over the code.
    /// This ensures full control over how calls are being made throughout the code, and how those calls are being filtered out.
    /// Efficiency is the key, thus it is advised to make as less calls to the server as it it possible,
    /// and when it is required to query the server - get as only the information that is required.
    /// </para>
    /// <param name="get">Messages getter gets new messages from the server. This property is meant to be like a gateway.</param>
    /// <param name="set">Not yet implemented.
    /// <para>
    /// Will deal with SMTP controller.
    /// Imagine - on added message, create a new SMTPController, send the message and dispose of the controller.
    /// </para>
    /// <para>
    /// Currently this is done separately.
    /// </para>
    /// </param>
    /// </summary>
    public ObservableCollection<DatabaseData> Messages {
        get {
            if (_clientController is not null) {
                if (_clientController.Client is {IsConnected: false, IsAuthenticated: false}) return _messages;
                var fetched = Inbox.Fetch(0, -1,
                    MessageSummaryItems.UniqueId | MessageSummaryItems.Envelope | MessageSummaryItems.Size | MessageSummaryItems.Flags);
                
                if (Inbox.Count != _messages.Count) {
                    var isThereAnyChanges = false;
                    List<string> ids = new List<string>();
                    foreach (var item in fetched) {
                        if (_idsInMessages.Exists(q =>
                                q.Equals(item.Envelope.MessageId) || q.Equals(item.UniqueId.ToString()))) {
                            ids.Add(item.Envelope.MessageId);
                            continue;
                            
                        };
                        var message = Inbox.GetMessage (item.UniqueId);
                        var newFlag = true;
                        var hasBeenDeleted = false;
                        if (item.Flags is not (null or MessageFlags.None)) {
                            switch (item.Flags.Value) {
                                case MessageFlags.Seen:
                                    newFlag = false;
                                    break;
                                case MessageFlags.Deleted:
                                    hasBeenDeleted = true;
                                    break;
                            }
                        }
                        if (string.IsNullOrEmpty(item.Envelope.MessageId)) {
                            ids.Add(item.UniqueId.ToString());
                        }
                        ids.Add(item.Envelope.MessageId);
                    
                        _messages.Add(DataController.ConvertFromMime(message,item.UniqueId,item.Folder.Name,newFlag,hasBeenDeleted));
                        isThereAnyChanges = true;
                        
                        if (App.Container is not null && newFlag) {
                            App.Container.Resolve<INotificationController>().SendNotification("New E-mail",message.Subject);
                        }

                    }

                    _idsInMessages = ids;
                    for (int i = 0; i < _messages.Count; i++) {
                        bool exists = false;
                        foreach (var id in ids) {
                            if (_messages[i].Id.Equals(id) || _messages[i].UniqueId.Equals(id)) {
                                exists = true;
                            }
                        }
                        if (exists is false) {
                            isThereAnyChanges =_messages.Remove(_messages[i]);
                        }
                    }
                    if (isThereAnyChanges) {
                        Database.UpdateDatabase(_messages);
                        _clientController.MessagesArrived = false;
                    }
                    
                }
                Inbox?.Close();
            }
            return _messages;
        }
        set {
            
        }
    }
    
    /// <summary>
    /// <para>
    /// Basic constructor that creates the class with basic functionality. This is needed for IdleClientController to inherit this class.
    /// </para>
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
    private void MessagesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
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