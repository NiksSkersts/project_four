using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using LLU.Android.LLU.Models;
using MailKit;
using MailKit.Net.Imap;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LLU.Controllers
{
    internal static class ServiceController
    {
        public static ComponentName StartBackgroundConnectionService() =>
            Application.Context.StartService(new Intent(Application.Context, typeof(BackgroundConnectionService)));
    }
    [Service]
    internal class BackgroundConnectionService : Service
    {
        public ClientController clientController;
        ObservableCollection<MimeMessage> _messages;
        ImapClient _client;
        CancellationTokenSource cancel;
        CancellationTokenSource done;
        bool messagesArrived;
        IMailFolder inbox;

        public override void OnCreate()
        {
            base.OnCreate();
            clientController = EmailUser.EmailUserData.clientController;
            _messages = EmailUser.EmailUserData.Messages;
            _client = clientController.Client.Item2;
            inbox = _client.Inbox;
            inbox.CountChanged += OnCountChanged;
            inbox.MessageExpunged += OnMessageExpunged;
            inbox.MessageFlagsChanged += OnMessageFlagsChanged;
            Idle();
        }

        private void OnMessageFlagsChanged(object sender, MessageFlagsChangedEventArgs e)
        {
            var folder = (ImapFolder)sender;
        }

        private void OnMessageExpunged(object sender, MessageEventArgs e)
        {
            var folder = (ImapFolder)sender;

            if (e.Index < _messages.Count)
            {
                var message = _messages[e.Index];

                // Note: If you are keeping a local cache of message information
                // (e.g. MessageSummary data) for the folder, then you'll need
                // to remove the message at e.Index.
                _messages.RemoveAt(e.Index);
            }
            else
            {
                Console.WriteLine("{0}: message #{1} has been expunged.", folder, e.Index);
            }
        }

        private void OnCountChanged(object sender, EventArgs e)
        {
            var folder = (ImapFolder)sender;

            // Note: because we are keeping track of the MessageExpunged event and updating our
            // 'messages' list, we know that if we get a CountChanged event and folder.Count is
            // larger than messages.Count, then it means that new messages have arrived.
            if (folder.Count > _messages.Count)
            {
                int arrived = folder.Count - _messages.Count;

                // Note: your first instinct may be to fetch these new messages now, but you cannot do
                // that in this event handler (the ImapFolder is not re-entrant).
                // 
                // Instead, cancel the `done` token and update our state so that we know new messages
                // have arrived. We'll fetch the summaries for these new messages later...
                messagesArrived = true;
                done?.Cancel();
            }
        }

        public override IBinder OnBind(Intent intent) => null;
        public override void OnDestroy()
        {
            base.OnDestroy();
            cancel.Cancel();
            inbox.MessageFlagsChanged -= OnMessageFlagsChanged;
            inbox.MessageExpunged -= OnMessageExpunged;
            inbox.CountChanged -= OnCountChanged;
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            cancel.Dispose();
        }
        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            return base.OnStartCommand(intent, flags, startId);
        }
        void Idle()
        {
            do
            {
                try
                {
                    WaitForNewMessages();

                    if (messagesArrived)
                    {
                        FetchMessageSummaries();
                        messagesArrived = false;
                    }
                }
                catch (System.OperationCanceledException)
                {
                    break;
                }
            } while (!cancel.IsCancellationRequested);
        }
        private void WaitForNewMessages()
        {
            do
            {
                try
                {
                    if (_client.Capabilities.HasFlag(ImapCapabilities.Idle))
                    {
                        // Note: IMAP servers are only supposed to drop the connection after 30 minutes, so normally
                        // we'd IDLE for a max of, say, ~29 minutes... but GMail seems to drop idle connections after
                        // about 10 minutes, so we'll only idle for 9 minutes.
                        done = new CancellationTokenSource(new TimeSpan(0, 9, 0));
                        try
                        {
                             _client.Idle(done.Token, cancel.Token);
                        }
                        finally
                        {
                            done.Dispose();
                            done = null;
                        }
                    }
                    else
                    {
                        // Note: we don't want to spam the IMAP server with NOOP commands, so lets wait a minute
                        // between each NOOP command.
                        Task.Delay(new TimeSpan(0, 1, 0), cancel.Token);
                        _client.NoOpAsync(cancel.Token);
                    }
                    break;
                }
                catch (ImapProtocolException)
                {
                    // protocol exceptions often result in the client getting disconnected
                }
                catch (IOException)
                {
                    // I/O exceptions always result in the client getting disconnected
                }
            } while (true);
        }
        void FetchMessageSummaries()
        {
            IList<IMessageSummary> fetched = null;

            do
            {
                try
                {
                    // fetch summary information for messages that we don't already have
                    int startIndex = _messages.Count;

                    fetched = _client.Inbox.Fetch(startIndex, -1, MessageSummaryItems.Full | MessageSummaryItems.UniqueId, cancel.Token);
                    break;
                }
                catch (ImapProtocolException)
                {
                    // protocol exceptions often result in the client getting disconnected
                }
                catch (IOException)
                {
                    // I/O exceptions always result in the client getting disconnected
                }
            } while (true);

            foreach (var message in fetched)
                _messages.Add(new MimeMessage
                {
                    MessageId = message.Envelope.MessageId,

                    
                }) ;
        }
    }
}
