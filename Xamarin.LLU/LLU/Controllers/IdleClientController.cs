using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LLU.Models;
using MailKit;
using MailKit.Net.Imap;

namespace LLU.Controllers;

/// <summary>
///     Class is built upon already existing ImapClient. By going into IDLE state, client takes up the task of receiving
///     new messages or notifications.
///     <para>
///         When creating an idle client, it should be noted that it's irreversible and ending IDLE state will require
///         client to be disposed of.
///     </para>
///     <para>It should be noted that an INBOX folder should be oppened before going into IDLE.</para>
/// </summary>
internal class IdleClientController : ClientController {
    private readonly ClientController clientControlleŗ;

    /// <summary>
    /// </summary>
    /// <param name="client"></param>
    public IdleClientController(ClientController client) {
        clientControlleŗ = client;
        _client = client.Client.Item2;
        if (_client is {IsConnected: true, IsAuthenticated: true}) {
            _client.Inbox.OpenAsync(FolderAccess.ReadOnly).ContinueWith(secondTask =>
                _client.IdleAsync(clientControlleŗ.done.Token, clientControlleŗ.cancel.Token));
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

                if (clientControlleŗ.messagesArrived) {
                    User.EmailUserData.FetchMessageSummaries();
                    clientControlleŗ.messagesArrived = false;
                }
            }
            catch (OperationCanceledException) {
                break;
            }
        } while (clientControlleŗ.cancel.IsCancellationRequested is false);
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
            try {
                if (_client.Capabilities.HasFlag(ImapCapabilities.Idle)) {
                    clientControlleŗ.done = new CancellationTokenSource(new TimeSpan(0, 9, 0));
                    try {
                        await _client.IdleAsync(clientControlleŗ.done.Token, clientControlleŗ.cancel.Token);
                    }
                    finally {
                        clientControlleŗ.done.Dispose();
                        clientControlleŗ.done = null;
                    }
                }
                else {
                    await Task.Delay(new TimeSpan(0, 1, 0), clientControlleŗ.cancel.Token);
                    await _client.NoOpAsync(clientControlleŗ.cancel.Token);
                }

                break;
            }
            catch (ImapProtocolException) { }
            catch (IOException) { }
        } while (true);
    }
}