using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace IISExpress.TestRunner.Processes
{
    public class PhantomJSTestRunner : TestRunner
    {
        protected override void StartProcess(Process process, TaskCompletionSource<bool> source)
        {
            process.OutputDataReceived += (sender, args) =>
            {
                if (String.Equals(args.Data, "Unable to load the address!", StringComparison.InvariantCulture))
                {
                    source.SetException(new ArgumentException(args.Data));
                    return;
                }
                LogInfo(args.Data);
            };
            process.Exited += (sender, args) =>
            {
                LogInfo("PhantomJS finished");
                source.TrySetResult(process.ExitCode == 0);
            };
            LogInfo("PhantomJS starting");
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