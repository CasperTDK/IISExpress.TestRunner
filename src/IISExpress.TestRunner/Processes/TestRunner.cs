using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IISExpress.TestRunner.Processes
{
    public class TestRunner : DisposableProcess
    {
        private Process _testRunner;

        protected override Process Process
        {
            get { return _testRunner; }
        }

        private string TestrunnerName;


        public int Start(string filename, string arguements)
        {
            var completionSource = new TaskCompletionSource<bool>();

            _testRunner = new Process();
            _testRunner.StartInfo.FileName = filename;
            _testRunner.StartInfo.Arguments = arguements;
            _testRunner.StartInfo.UseShellExecute = false;
            _testRunner.StartInfo.RedirectStandardOutput = true;
            _testRunner.EnableRaisingEvents = true;

            TestrunnerName = Path.GetFileName(filename);

            LogInfo(string.Format("Starting {0} with arguments {1}", TestrunnerName, Process.StartInfo.Arguments));

            int resultCode;

            try
            {
                Task.Run(() => StartProcess(_testRunner, completionSource));
                var testRunnerTask = completionSource.Task;
                resultCode = testRunnerTask.Result ? 0 : -1;
            }
            catch (AggregateException e)
            {
                string message = e.InnerExceptions.Select(exception => exception.Message).FirstOrDefault();
                LogInfo(string.Format("An error occurred while executing {1}. {0}", message, TestrunnerName));
                resultCode = -1;
            }


            return resultCode;
        }

        protected virtual void StartProcess(Process process, TaskCompletionSource<bool> source)
        {
            process.OutputDataReceived += (sender, args) => LogInfo(args.Data);
            process.Exited += (sender, args) =>
            {
                LogInfo(string.Format("{0} finished", TestrunnerName));
                source.TrySetResult(process.ExitCode == 0);
            };
            LogInfo(string.Format("{0} starting", TestrunnerName));
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
    }
}