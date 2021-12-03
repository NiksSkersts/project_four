using MailKit.Security;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace LLU.Controllers
{
    internal abstract class Controller : IDisposable
    {
        protected string Host { get; set; }
        protected int Port { get; set; }

        public CancellationTokenSource cancel;
        public CancellationTokenSource done;

        public void Dispose()
        {
            cancel?.Cancel();
        }
    }
}
