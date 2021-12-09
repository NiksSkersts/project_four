using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using LLU.Controllers;
using LLU.Models;
using MailKit;
using MailKit.Net.Imap;
using MimeKit;
using MimeKit.Text;

namespace LLU.Android.LLU.Models;

internal class EmailUser : User {
    public bool CreateAndSendMessage(string receiversString, string subject, string body) {
        var email = new MimeMessage();
        var receivers = receiversString.Split(';');
        var fullList = receivers.Where(mailaddress => mailaddress.Contains('@')).ToList();
        for (var i = 0; i < fullList.Count - 1; i++)
            fullList[i] = fullList[i].Trim();
        try {
            email.From.Add(MailboxAddress.Parse($"{Username}@llu.lv"));
            foreach (var receiver in fullList)
                email.To.Add(MailboxAddress.Parse(receiver));
            email.Subject = subject;
            email.Body = new TextPart(TextFormat.Text) {Text = body};
            var auth = _smtpController.ClientAuth(Username, Password);
            var status = false;
            if (auth) {
                status = _smtpController.SendMessage(email).Result;
                if (status)
                    _clientController.Client.Item2?.Inbox.GetSubfolderAsync("Sent", _clientController.cancel.Token)
                        .ContinueWith(second => second.Result.Append(email));
            }

            _smtpController.Dispose();
            return status && auth;
        }
        catch {
            return false;
        }
    }

    private void OnMessageFlagsChanged(object sender, MessageFlagsChangedEventArgs e) {
        var folder = (ImapFolder) sender;
    }

    /// <summary>
    ///     Note: If you are keeping a local cache of message information (e.g. MessageSummary data) for the folder, then
    ///     you'll need to remove the message at e.Index.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnMessageExpunged(object sender, MessageEventArgs e) {
        var folder = (ImapFolder) sender;

        if (e.Index < _messages.Count)
            _messages.RemoveAt(e.Index);
        else
            Console.WriteLine("{0}: message #{1} has been expunged.", folder, e.Index);
    }

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
        var folder = (ImapFolder) sender;
        if (folder.Count <= _messages.Count) return;
        _clientController.done = new CancellationTokenSource(new TimeSpan(0, 9, 0));
        var arrived = folder.Count - _messages.Count;
        _clientController.messagesArrived = true;
        _clientController.done.Cancel();
    }

    /// <summary>
    ///     Fetch summary information for messages that we don't already have.
    ///     <para>
    ///         Protocol and I/O exceptions often result in the client getting disconnected
    ///     </para>
    /// </summary>
    public void FetchMessageSummaries() {
        IList<IMessageSummary> fetched;

        do {
            try {
                var startIndex = _messages.Count;
                fetched = _clientController.Client.Item2.Inbox.Fetch(startIndex, -1,
                    MessageSummaryItems.Full | MessageSummaryItems.UniqueId, _clientController.cancel.Token);
                foreach (var messageSummary in fetched) Summaries.Add(messageSummary);
                break;
            }
            catch (ImapProtocolException) { }
            catch (IOException) { }
        } while (true);

        ;
    }

    #region Backing fields and class wide variables

    private readonly ClientController _clientController;
    private readonly SmtpController _smtpController;
    private ObservableCollection<MimeMessage> _messages;
    private IMailFolder _mailfolder;

    #endregion

    #region Constructors and Destructors

    private EmailUser() {
        _clientController = new ClientController(Secrets);
        _smtpController = new SmtpController(Secrets);
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
        Username = username;
        Password = password;
        Database.ClientAuth(username, password);
    }

    public new void Dispose() {
        _clientController.Dispose();
        Database.Dispose();
    }

    #endregion

    #region Parameters

    public bool IsClientConnected => _clientController.Client.Item2.IsConnected;

    public bool IsClientAuthenticated {
        get {
            if (IsClientConnected is false)
                return false;

            var status = _clientController.Client.Item2.IsAuthenticated;
            if (status is false)
                status = _clientController.ClientAuth(Username, Password);
            return status;
        }
    }

    private IMailFolder Inbox {
        get {
            _mailfolder = _clientController.Client.Item2.Inbox;
            if (_mailfolder.IsOpen) return _mailfolder;
            _mailfolder.Open(FolderAccess.ReadWrite);
            _mailfolder.CountChanged += OnCountChanged;
            _mailfolder.MessageExpunged += OnMessageExpunged;
            _mailfolder.MessageFlagsChanged += OnMessageFlagsChanged;
            return _mailfolder;
        }
    }

    private ObservableCollection<IMessageSummary> Summaries { get; }

    public ObservableCollection<MimeMessage> Messages {
        get {
            if (!IsClientConnected || !IsClientAuthenticated) return _messages;

            using var client = _clientController.Client.Item2;

            var unordered = _clientController.GetMessages().OrderBy(x => x.Date);

            ObservableCollection<MimeMessage> messages = new();

            foreach (var message in unordered) messages.Add(message);

            if (Database.CheckForChanges(messages[0].MessageId, messages.Count))
                Database.ApplyChanges(messages, Username);

            _messages = messages;

            return _messages;
        }
        set {
            foreach (var message in value)
                _messages.Add(message);
        }
    }

    #endregion
}