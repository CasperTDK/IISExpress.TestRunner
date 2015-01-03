using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IISExpress.TestRunner.Processes
{
    public class IISExpress : DisposableProcess
    {
        private Process _iisExpressProcess;

        protected override Process Process
        {
            get { return _iisExpressProcess; }
        }

        public int Start(string websitePath, string websitePort, string sitename)
        {
            var completionSource = new TaskCompletionSource<bool>();

            var iisExpressPath = DetermineIisExpressPath();

            // ReSharper disable once UseObjectOrCollectionInitializer
            _iisExpressProcess = new Process();
            _iisExpressProcess.StartInfo.FileName = iisExpressPath;
            _iisExpressProcess.StartInfo.Arguments = string.Format("/path:{0} /port:{1}", websitePath, websitePort);
            _iisExpressProcess.StartInfo.RedirectStandardOutput = true;
            _iisExpressProcess.StartInfo.UseShellExecute = false;
            _iisExpressProcess.EnableRaisingEvents = true;

            LogInfo(string.Format("Starting {0} with arguments {1}", sitename ?? "Development Web Site", Process.StartInfo.Arguments));


            int resultCode;

            try
            {
                Task.Run(() => StartProcess(Process, completionSource));
                var phantomJsTask = completionSource.Task;
                resultCode = phantomJsTask.Result ? 0 : -1;
            }
            catch (AggregateException e)
            {
                var message = e.InnerExceptions.Select(exception => exception.Message).FirstOrDefault();
                LogInfo(string.Format("An error occurred while starting IISExpress. {0}", message));
                resultCode = -1;
            }

            return resultCode;
        }

        protected void StartProcess(Process process, TaskCompletionSource<bool> source)
        {
            process.OutputDataReceived += (sender, args) =>
            {
                if (args.Data == null) return;
                var str = args.Data;
                if (str.StartsWith("Successfully"))
                {
                    process.Exited -= ServerFailedOnStart(source);
                    source.SetResult(true);
                }
            };
            process.Exited += ServerFailedOnStart(source);
            LogInfo("IIS starting");
            try
            {
                process.Start();
                process.BeginOutputReadLine();
            }
            catch (Exception)
            {
                Dispose();
                source.SetException(new ArgumentException("Failed starting process"));
            }
        }

        private static EventHandler ServerFailedOnStart(TaskCompletionSource<bool> source)
        {
            return (sender, args) => source.TrySetCanceled();
        }

        private static String DetermineIisExpressPath()
        {
            var iisExpressPath = Environment.GetFolderPath(Environment.Is64BitOperatingSystem
                ? Environment.SpecialFolder.ProgramFilesX86
                : Environment.SpecialFolder.ProgramFiles);

            iisExpressPath = Path.Combine(iisExpressPath, @"IIS Express\iisexpress.exe");

            return iisExpressPath;
        }


    }
}