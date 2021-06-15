using JSSoft.Crema.Data;
using JSSoft.Library;
using JSSoft.Library.IO;
using JSSoft.Library.Random;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Test.Extensions
{
    static class CremaBootstrapperExtensions
    {
#if CLIENT
        private static readonly object obj = new();
        private static readonly int startPort = 4006;
        private static readonly HashSet<int> reservedPort = new();
        private static readonly Dictionary<CremaBootstrapper, ServerHost> serverHostByApp = new();
#endif
#if SERVER
        private static readonly Dictionary<CremaBootstrapper, string> repositoryPathByApp = new();
#endif

        private static readonly Dictionary<Authentication, Guid> authenticationToToken = new();

        public static ServerHost Initialize(this CremaBootstrapper app, TestContext context)
        {
#if SERVER
            var repositoryPath = DirectoryUtility.Prepare(context.TestRunDirectory, "repo", context.FullyQualifiedTestClassName);
            var userInfos = UserContextExtensions.GenerateUserInfos(RandomUtility.Next(500, 1000), RandomUtility.Next(100, 1000));
            var dataSet = new CremaDataSet();
            CremaBootstrapper.CreateRepositoryInternal(app, repositoryPath, "git", "xml", string.Empty, userInfos, dataSet);
            app.BasePath = repositoryPath;
            repositoryPathByApp.Add(app, repositoryPath);
            return new ServerHost(app, userInfos);
#endif
#if CLIENT
            var cremaHost = app.GetService(typeof(ICremaHost)) as ICremaHost;
            var repositoryPath = DirectoryUtility.Prepare(context.TestRunDirectory, "repo", context.FullyQualifiedTestClassName);
            var solutionPath = Path.GetDirectoryName(Path.GetDirectoryName(context.TestDir));
            var executablePath = Path.Combine(solutionPath, "server", "JSSoft.Crema.ConsoleHost", "bin", "Debug", "netcoreapp3.1", "cremaserver.exe");
            var port = ReservePort();
            var repositoryInitCommand = new CommandHost(executablePath, solutionPath)
            {
                "init",
                $"\"{repositoryPath}\""
            };
            var serverHost = new ServerHost()
            {
                ExecutablePath = executablePath,
                RepositoryPath = repositoryPath,
                WorkingPath = solutionPath,
                Port = port
            };
            repositoryInitCommand.Run();
            serverHost.Start();
            serverHostByApp.Add(app, serverHost);
            app.Address = $"localhost:{port}";
#endif
        }

        public static void Release(this CremaBootstrapper app)
        {
#if SERVER
            var repositoryPath = repositoryPathByApp[app];
            DirectoryUtility.Delete(repositoryPath);
            repositoryPathByApp.Remove(app);
#endif
#if CLIENT
            var serverHost = serverHostByApp[app];
            serverHost.Stop();
            ReleasePort(serverHost.Port);
#endif
            app.Dispose();
        }

#if CLIENT
        private static int ReservePort()
        {
            lock (obj)
            {
                for (var i = startPort; i < int.MaxValue; i++)
                {
                    if (reservedPort.Contains(i) == false)
                    {
                        reservedPort.Add(i);
                        return i;
                    }
                }
                throw new NotImplementedException();
            }
        }

        private static void ReleasePort(int port)
        {
            lock (obj)
            {
                reservedPort.Remove(port);
            }
        }
#endif

    }
}
