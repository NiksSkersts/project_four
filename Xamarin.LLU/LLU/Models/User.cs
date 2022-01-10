using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using Android.App;
using LLU.Android.Controllers;
using LLU.Controllers;
using LLU.Models;
using MailKit;
using MailKit.Net.Imap;
using MimeKit;
using Newtonsoft.Json;
using Unity;

namespace LLU.Android.LLU.Models;

/// <summary>
///     Defines the User class from which (at the moment only) EmailUser inherits from.
///     <para>
///         Provides common functions or parameters that should be shared amongst all classes that inherit from this
///         class.
///     </para>
/// </summary>
internal abstract class User : IDisposable {
    /// <summary>
    ///     Gets secrets from a json file.
    /// </summary>
    protected static Secrets Secrets {
        get {
            var assets = Application.Context.Assets;
            using var streamReader = new StreamReader(assets.Open("secrets"));
            var stream = streamReader.ReadToEnd();
            var secrets = JsonConvert.DeserializeObject<Secrets>(stream);
            return secrets;
        }
    }

    /// <summary>
    ///     Normally a connection to server would fail if you supply a bad username or password,
    ///     but the goal is to make as little as possible connections to the server to preserve cycles both on server and on
    ///     smartphone.
    ///     So let's add a safety check that makes sure we don't make an authentication query with useless credentials.
    /// </summary>
    protected internal static UserData? UserData {
        get {
            UserData user = new UserData();
            var path = DataController.GetFilePath("user");
            if (File.Exists(path)) {
                string[] lines = File.ReadAllLines(path);
                if (lines.Length is 2) {
                    user.Username = lines[0];
                    user.Password = lines[1];
                }
            }

            if ((!string.IsNullOrEmpty(user.Username)
                 || !string.IsNullOrEmpty(user.Password)))
                return user;
            RuntimeController.Instance.WipeDatabase();
            return null;
        }
        set {
            var path = DataController.GetFilePath("user");
            if (value is null) {
                if (File.Exists(path)) {
                    File.Delete(path);
                }

                return;
            }

            string[] lines = {value.Username, value.Password};
        redo:
            if (File.Exists(path)) {
                File.WriteAllLines(path, lines);
            }
            else {
                File.Create(path).Close();
                goto redo;
            }
        }
    }

    /// <summary>
    ///     Stores temporary data that gets deleted when the application terminates.
    ///     WARNING: all permanent data gets stored within the database!
    /// </summary>
    public static EmailUser? EmailUserData { get; set; }

    public void Dispose() { }
}

/// <summary>
///     Defines the main class that stores and deals with email data.
///     <para>It's responsible for an easy access to inbox, messages and creation of SMTP and IMAP clients.</para>
/// </summary>
internal class EmailUser : User {
    private readonly ClientController? _clientController;
    private IMailFolder? _inbox;
    private ObservableCollection<DatabaseData> _messages;

    /// <summary>
    ///     <para>
    ///         Basic constructor that creates the class with basic functionality.
    ///         This is required for IdleClientController to inherit this class.
    ///     </para>
    /// </summary>
    private EmailUser() {
        _messages = new ObservableCollection<DatabaseData>();
    }

    /// <summary>
    ///     Constructor creates a new EmailUser on app launch. Assumption remains that this constructor is used at first-launch
    ///     of the application or when app fails to connect with using database data (credential change).
    ///     <para>
    ///         WARNING: This does not validate connection. Validation should be done within LoginActivity and AccountManager.
    ///         This class, for all intents and purposes, only is the middle layer between Database, Server and functionality.
    ///     </para>
    ///     <para name="userid">
    ///         WARNING: One should avoid validating by char count(). Not all LLU employees or students have standard (length
    ///         == 7) usernames.
    ///         Notable examples are employees and students who came to work before they began to study.
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
        _clientController = new ClientController(Secrets, UserData);
        if (_clientController != null && _clientController.Client.IsAuthenticated) {
            _messages = RuntimeController.Instance.GetMessages();
        }
    }

    //todo implement better way to get new emails.
    /// <summary>
    ///     <para>Maintains connection with IMAP server and forces reconnect if CLIENT seems to be unavailable.</para>
    ///     <para>
    ///         Initializes IMailFolder and makes sure it's OPEN before it gets accessed.
    ///         Maintains IMailFolder events. As they are small, events are embedded as lambda statements. <br></br>
    ///         todo: refactor lambdas as functions if they grow larger.
    ///     </para>
    /// </summary>
    /// <exception cref="Exception">
    ///     null client controller shouldn't happen unless INBOX gets accessed before LOGIN is completed.
    ///     For the sake of debugging - throw an exception.
    /// </exception>
    private IMailFolder Inbox {
        get {
            var mailFolder = _inbox;
            if (_clientController is not null) {
            securityCheck:
                var client =  _clientController.Client;
                if (_clientController.Client is {IsConnected:true,IsAuthenticated:true}) {
                    if (mailFolder is null) {
                        mailFolder = client.Inbox;
#region toFix

                                        //todo fix events! As of 07.01.2022 they are not working.
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
                                                }
                                            }
                                        };

#endregion
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
                    goto securityCheck;
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
    ///     <para>
    ///         Main goal is to maintain a single entity that deals with getting messages, instead of doing it all over the
    ///         code.
    ///         This ensures full control over how calls are being made throughout the code, and how those calls are being
    ///         filtered out.
    ///         Efficiency is the key, thus it is advised to make as less calls to the server as it it possible,
    ///         and when it is required to query the server - get as only the information that is required.
    ///     </para>
    ///     <param name="get">Messages getter gets new messages from the server. This property is meant to be like a gateway.</param>
    ///     <param name="set">
    ///         Not yet implemented.
    ///         <para>
    ///             Will deal with SMTP controller.
    ///             Imagine - on added message, create a new SMTPController, send the message and dispose of the controller.
    ///         </para>
    ///         <para>
    ///             Currently this is done separately.
    ///         </para>
    ///     </param>
    /// </summary>
    public IEnumerable<DatabaseData> Messages {
        get {
            if (_clientController is null) return _messages;
            if (IsthereAnyChanges()) {
                
                var fetched = FetchMessagesFromServer(0, -1);
                var fetchedRw = fetched!.ToList();
                var oldMessages = _messages;
                var newCollection = new ObservableCollection<DatabaseData>();
                var deletedMessages = new List<DatabaseData>();

                if (fetched is not null) {
                    
                    foreach (var oldMessage in oldMessages) {
                        var exists = false;
                        foreach (var fetchedMessage in fetched) {
                            if (oldMessage.Id.Equals(fetchedMessage.Envelope.MessageId) || oldMessage.Id.Equals(fetchedMessage.UniqueId.ToString())) {
                                newCollection.Add(oldMessage);
                                fetchedRw.Remove(fetchedMessage);
                                exists = true;
                            }

                            if (exists is false) {
                                deletedMessages.Add(oldMessage);
                            }
                        }
                        
                    }

                    foreach (var item in fetchedRw) {

                        var message = Inbox.GetMessage(item.UniqueId);
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
                        

                        newCollection.Add(DataController.ConvertFromMime(message, item.UniqueId, item.Folder.Name, newFlag,
                            hasBeenDeleted));
                        
                        RuntimeController.Instance.UpdateDatabase(newCollection);
                        
                        if (App.Container is not null && newFlag) {
                            App.Container.Resolve<INotificationController>()
                                .SendNotification("New E-mail", message.Subject);
                        }
                        
                    }
                    
                    foreach (var message in deletedMessages) {
                        RuntimeController.Instance.RemoveMessage(message);
                    }
                    _clientController.MessagesArrived = false;
                    _messages = newCollection;
                }
            }

            Inbox?.Close();
            return _messages.OrderByDescending(q=>q.Time);



            bool IsthereAnyChanges() => Inbox.Count != _messages.Count;
            IList<IMessageSummary>? FetchMessagesFromServer(int startIndex, int endIndex) {
                var fetched = Inbox.Fetch(startIndex, endIndex,
                    MessageSummaryItems.UniqueId | MessageSummaryItems.Envelope | MessageSummaryItems.Flags);
                return fetched;
            }
        }
        set => throw new NotImplementedException("Messages being set!");
    }

    /// <summary>
    ///     Converts inputted text data to MimeMessage.
    /// </summary>
    /// <param name="toText">Email addresses seperated by char ';'</param>
    /// <param name="subjectText">Self-explanatory</param>
    /// <param name="bodyText">Self-explanatory</param>
    /// <returns></returns>
    public static MimeMessage CreateEmail(string toText, string? subjectText, string? bodyText)
        => DataController.CreateEmail(toText, UserData.Username, subjectText, bodyText);

    /// <summary>
    ///     Sends email by creating and SMTP controller and pushing MimeMessage to the server.
    ///     The same message is then added to folder "Sent".
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public bool SendEmail(MimeMessage email) {
        var status = false;
        bool auth;
        try {
            var smtpController = new SmtpController(Secrets, new UserData {
                Username = UserData!.Username,
                Password = UserData.Password
            });
            auth = smtpController.IsOkay;
            if (auth) {
                status = smtpController.SendMessage(email);
                smtpController.Dispose();
                if (status)
                    Inbox.GetSubfolderAsync("Sent", CancellationToken.None)
                        .ContinueWith(second => second.Result.Append(email), CancellationToken.None);
            }
        }
        catch (Exception) {
            return false;
        }

        return status && auth;
    }

    /// <summary>
    ///     Gets a single MimeMessage from the server by using its uniqueID.
    /// </summary>
    /// <param name="uniqueId">
    ///     UniqueID is an ID that's completely unique for that email in that specific folder.
    ///     Imagine database table ID. It's unique in the specified table, but it can repeat in the whole database.
    /// </param>
    /// <returns> MimeMessage unless the specified UniqueID doesn't exist, then it returns null.</returns>
    public MimeMessage? GetMessageFromServer(string uniqueId) {
        try {
            return Inbox.GetMessage(UniqueId.Parse(uniqueId));
        }
        catch (Exception) {
            // ignored
        }

        return null;
    }

    /// <summary>
    ///     Sets a message flag in the server. For example, if user reads an email,
    ///     this flag is then sent to the server alongside the specified unique ID.
    /// </summary>
    /// <param name="uniqueId"></param>
    /// <param name="flags"></param>
    internal void SetMessageFlags(string uniqueId, MessageFlags flags) {
        var uid = UniqueId.Parse(uniqueId);
        Inbox.SetFlags(uid, flags, true, CancellationToken.None);
    }

    /// <summary>
    ///     Makes sure the client controller and database gets closed and disposed of.
    ///     This is done to prevent zombie connection accumulation server-side.
    /// </summary>
    public new void Dispose() {
        _clientController?.Dispose();
        RuntimeController.Instance.Dispose();
    }
}