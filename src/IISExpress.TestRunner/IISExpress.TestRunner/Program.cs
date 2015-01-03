using System;
using System.Diagnostics;
using IISExpress.TestRunner.Attribute;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace IISExpress.TestRunner
{
    internal class Program
    {
        private enum Env
        {
            Debug,
            Release
        }
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));
        private static Env _environment;

        private static void ConfigureLogging()
        {
            var patternLayout = new PatternLayout { ConversionPattern = "%date [%thread] %-5level %logger - %message%newline" };
            var hierarchy = (Hierarchy)LogManager.GetRepository();
            hierarchy.Root.AddAppender(new TraceAppender { Layout = patternLayout });
            hierarchy.Root.AddAppender(new ConsoleAppender { Layout = patternLayout });

            hierarchy.Root.Level = _environment == Env.Debug ? Level.Debug : Level.Info;

            hierarchy.Configured = true;
        }

        private static int Main(string[] args)
        {
#if DEBUG
            _environment = Env.Debug;
#else
             environment = Env.Release;
#endif
            ConfigureLogging();
            Log.Debug("Starting IISExpress.TestRunner...");

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
                    if (testIISExpress != null) testIISExpress.Dispose();
                    if (testRunnerStarted == -1) return HandleError("TestRunner");
                }
            }
            return Shutdown();
        }

        private static int Shutdown()
        {
            if (_environment == Env.Debug)
            {
                Debugger.Break();
                Console.ReadKey();
            }

            Log.Debug("Shutting down");
            return 0;
        }


        private static int HandleError(string stepName)
        {
            Log.Info("An error occurred executing " + stepName + ". Exiting...");

            if (_environment == Env.Debug)
            {
                Debugger.Break();
                Console.ReadKey();
            }

            Log.Debug("Shutting down");
            return -1;
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