﻿using System;
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
/// Not yet implemented!
/// </summary>
internal class IdleClientController : ClientController {
    private CancellationTokenSource _done;
    private readonly ClientController _clientControlleŗ;

    /// <summary>
    /// </summary>
    /// <param name="client"></param>
    /// <param name="secrets"></param>
    public IdleClientController(ClientController client, Secrets secrets) : base(secrets) {
        _done = new CancellationTokenSource();
        _clientControlleŗ = client;
        Client = _clientControlleŗ.Client;
        if (Client is {IsConnected: true, IsAuthenticated: true}) {
            Client.Inbox.OpenAsync(FolderAccess.ReadOnly).ContinueWith(second=>Client.IdleAsync(_done.Token, _clientControlleŗ.Cancel.Token));
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

                if (_clientControlleŗ.MessagesArrived) {
                    _clientControlleŗ.MessagesArrived = false;
                }
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