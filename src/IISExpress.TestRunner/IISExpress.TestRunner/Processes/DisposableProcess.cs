using System;
using System.Diagnostics;

namespace IISExpress.TestRunner.Processes
{
    public abstract class DisposableProcess : IDisposable
    {
        protected Action<string> LogInfo { get; set; }

        private Boolean _isDisposed;

        protected abstract Process Process { get; }

        protected DisposableProcess()
        {
            LogInfo = Console.WriteLine;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(Boolean disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                if (Process.HasExited == false)
                {
                    Process.Kill();
                }

                Process.Dispose();
            }

            _isDisposed = true;
        }
    }
}