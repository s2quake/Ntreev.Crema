using JSSoft.Library.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace JSSoft.Crema.Services.Test.Extensions
{
    static class CremaBootstrapperExtensions
    {
#if CLIENT
        private static int port = 4006;
        private static readonly Dictionary<ICremaHost, int> cremaHostToPort = new Dictionary<ICremaHost, int>();
#endif
#if SERVER
        private static readonly Dictionary<CremaBootstrapper, string> repositoryPathByApp = new();
#endif

        private static readonly Dictionary<Authentication, Guid> authenticationToToken = new();

        public static void Initialize(this CremaBootstrapper app, TestContext context, string name)
        {
#if SERVER
            var repositoryPath = DirectoryUtility.Prepare(context.TestRunDirectory + "_repo", name);
            CremaBootstrapper.CreateRepository(app, repositoryPath, "git", "xml");
            app.BasePath = repositoryPath;
            repositoryPathByApp.Add(app, repositoryPath);
#endif
#if CLIENT
            var cremaHost = boot.GetService(typeof(ICremaHost)) as ICremaHost;
            var repositoryPath = DirectoryUtility.Prepare(context.TestRunDirectory + "_repo", name);
            CremaServeHost.Run("init", repositoryPath.WrapQuot());
            var process = CremaServeHost.RunAsync("run", repositoryPath.WrapQuot(), "--port", port);
            var eventSet = new ManualResetEvent(false);
            cremaHostToPort[cremaHost] = port;
            port += 2;
            process.OutputDataReceived += (s, e) =>
            {
                if (e.Data == "종료하시려면 <Q> 키를 누르세요.")
                    eventSet.Set();
            };
            eventSet.WaitOne();
            boot.Disposed += (s, e) =>
            {
                process.StandardInput.WriteLine("exit");
                process.WaitForExit(100);
                cremaHostToPort.Remove(cremaHost);
            };
#endif
        }

        public static void Release(this CremaBootstrapper app)
        {
            var repositoryPath = repositoryPathByApp[app];
            DirectoryUtility.Delete(repositoryPath);
            repositoryPathByApp.Remove(app);
            app.Dispose();
        }
    }
}
