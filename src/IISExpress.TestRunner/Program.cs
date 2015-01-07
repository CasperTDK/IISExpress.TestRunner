using System;
using System.Configuration;
using System.Diagnostics;
using IISExpress.TestRunner.Attribute;

namespace IISExpress.TestRunner
{
    internal class Program
    {
        private enum Env
        {
            Debug,
            Release
        }

        private static readonly Action<string> Log = str => Console.WriteLine(str);
        private static Env _environment;



        private static int Main(string[] args)
        {
            _environment = ConfigurationManager.AppSettings["environment"] == "debug" ? Env.Debug : Env.Release;

            Log("Starting IISExpress.TestRunner...");

            var parsedArguments = new CommandLineArguments();
            CommandLineParser.ParseArguments(args, parsedArguments);

            using (var iisExpress = new Processes.IISExpress())
            {
                var iisExpressStarted = iisExpress.Start(parsedArguments.WebsitePath, parsedArguments.WebsitePort, parsedArguments.WebsiteName);
                if (iisExpressStarted == -1) return HandleError("IISExpress");

                Processes.IISExpress testIISExpress = null;
                if (parsedArguments.HasSeperateTestProject)
                {
                    testIISExpress = new Processes.IISExpress();
                    var testIISExpressStarted = testIISExpress.Start(parsedArguments.TestWebsitePath, parsedArguments.TestWebsitePort, parsedArguments.TestWebsiteName);
                    if (testIISExpressStarted == -1) return HandleError("TestIISExpress");
                }
                using (var testRunner = new Processes.TestRunner())
                {
                    var testRunnerStarted = testRunner.Start(parsedArguments.TestRunnerPath, parsedArguments.TestRunnerArguments);
                    Pause();
                    if (testIISExpress != null) testIISExpress.Dispose();
                    if (testRunnerStarted == -1) return HandleError("TestRunner");
                }
            }
            return Shutdown();
        }

        private static int Shutdown()
        {
            Log("Shutting down");
            return 0;
        }



        private static int HandleError(string stepName)
        {
            Log("An error occurred executing " + stepName + ". Exiting...");
            Pause();
            Log("Shutting down");
            return -1;
        }

        private static void Pause()
        {
            if (_environment == Env.Debug)
            {
                Console.WriteLine("Waiting for keypress..");
                Console.ReadKey();
            }
        }

    }


    internal class CommandLineArguments
    {
        [ConsoleArgument(0)]
        public string WebsitePath { get; set; }
        [ConsoleArgument(1)]
        public string WebsitePort { get; set; }
        [ConsoleArgument(2)]
        public string TestRunnerPath { get; set; }
        [ConsoleArgument(3)]
        public string TestRunnerArguments { get; set; }
        [ConsoleArgument(4)]
        public string WebsiteName { get; set; }

        public bool HasSeperateTestProject
        {
            get { return TestWebsitePath != null; }
        }

        [ConsoleArgument(5, true)]
        public string TestWebsitePath { get; set; }
        [ConsoleArgument(6, true)]
        public string TestWebsitePort { get; set; }
        [ConsoleArgument(7, true)]
        public string TestWebsiteName { get; set; }
    }
}