// Released under the MIT License.
// 
// Copyright (c) 2018 Ntreev Soft co., Ltd.
// Copyright (c) 2020 Jeesu Choi
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit
// persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the
// Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// Forked from https://github.com/NtreevSoft/Crema
// Namespaces and files starting with "Ntreev" have been renamed to "JSSoft".

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using JSSoft.Library.IO;
using JSSoft.Library;
using System.Linq;
using JSSoft.Library.Random;
using JSSoft.Crema.Services;
using JSSoft.Crema.Services.Random;
using JSSoft.Crema.ServiceModel;
using System.Threading.Tasks;
using System.Text;
using JSSoft.Crema.Services.Test.Extensions;
using JSSoft.Crema.Services.Extensions;
using System.Threading;

namespace JSSoft.Crema.Services.Test
{
    [TestClass]
    public class CremaHostTest
    {
        private static CremaBootstrapper app;
        private static ICremaHost cremaHost;
        private static Guid token;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            app = new CremaBootstrapper();
            app.Initialize(context, nameof(ITableCategory_ComplexTest));
            cremaHost = app.GetService(typeof(ICremaHost)) as ICremaHost;
        }

        [TestInitialize]
        public void Initialize()
        {
        }

        [TestCleanup]
        public async Task CleanupAsync()
        {
            if (token != Guid.Empty)
                await cremaHost.CloseAsync(token);
            token = Guid.Empty;
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            app.Release();
        }

        [TestMethod]
        public async Task OpenAsyncTestAsync()
        {
            cremaHost.Opening += CremaHost_Opening;
            cremaHost.Opened += CremaHost_Opened;
            token = await cremaHost.OpenAsync();
            cremaHost.Opening -= CremaHost_Opening;
            cremaHost.Opened -= CremaHost_Opened;

            static void CremaHost_Opening(object sender, EventArgs e)
            {
                Assert.AreEqual(ServiceState.Opening, cremaHost.ServiceState);
            }

            static void CremaHost_Opened(object sender, EventArgs e)
            {
                Assert.AreEqual(ServiceState.Open, cremaHost.ServiceState);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task OpenAsyncTestAsync_Fail()
        {
            token = await cremaHost.OpenAsync();
            await cremaHost.OpenAsync();
        }

        [TestMethod]
        public async Task LoginAsyncTestAsync1()
        {
            token = await cremaHost.OpenAsync();
            await cremaHost.LoginAsync("admin", StringUtility.ToSecureString("admin"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task LoginAsyncTestAsync_Fail1()
        {
            token = await cremaHost.OpenAsync();
            await cremaHost.LoginAsync("admin", StringUtility.ToSecureString("admin1"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task LoginAsyncTestAsync_Fail2()
        {
            token = await cremaHost.OpenAsync();
            await cremaHost.LoginAsync(null, StringUtility.ToSecureString("admin"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task LoginAsyncTestAsync_Fail3()
        {
            token = await cremaHost.OpenAsync();
            await cremaHost.LoginAsync("admin", null);
        }

        [TestMethod]
        [ExpectedException(typeof(CremaException))]
        public async Task LoginAsyncTestAsync_Fail4()
        {
            token = await cremaHost.OpenAsync();
            await cremaHost.LoginAsync("admin", StringUtility.ToSecureString("admin"));
            await cremaHost.LoginAsync("admin", StringUtility.ToSecureString("admin"));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task LoginAsyncTestAsync_Fail5()
        {
            await cremaHost.LoginAsync("admin", StringUtility.ToSecureString("admin"));
        }

        [TestMethod]
        public async Task LoginAsyncTestAsync2()
        {
            token = await cremaHost.OpenAsync();
            var authenticationToken1 = await cremaHost.LoginAsync("admin", StringUtility.ToSecureString("admin"));
            await cremaHost.LogoutAsync("admin", StringUtility.ToSecureString("admin"));
            var authenticationToken2 = await cremaHost.LoginAsync("admin", StringUtility.ToSecureString("admin"));
            Assert.AreNotEqual(authenticationToken1, authenticationToken2);
        }

        [TestMethod]
        public async Task LogoutAsyncTestAsync()
        {
            token = await cremaHost.OpenAsync();
            var authenticationToken1 = await cremaHost.LoginAsync("admin", StringUtility.ToSecureString("admin"));
            var authentication1 = await cremaHost.AuthenticateAsync(authenticationToken1);
            await cremaHost.LogoutAsync(authentication1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task LogoutAsyncTestAsync_Fail1()
        {
            token = await cremaHost.OpenAsync();
            var authenticationToken1 = await cremaHost.LoginAsync("admin", StringUtility.ToSecureString("admin"));
            var authentication1 = await cremaHost.AuthenticateAsync(authenticationToken1);
            await cremaHost.LogoutAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task LogoutAsyncTestAsync_Fail2()
        {
            token = await cremaHost.OpenAsync();
            var authenticationToken1 = await cremaHost.LoginAsync("admin", StringUtility.ToSecureString("admin"));
            var authentication1 = await cremaHost.AuthenticateAsync(authenticationToken1);
            await cremaHost.LogoutAsync("admin", StringUtility.ToSecureString("admin"));
            var authenticationToken2 = await cremaHost.LoginAsync("admin", StringUtility.ToSecureString("admin"));
            var authentication2 = await cremaHost.AuthenticateAsync(authenticationToken2);
            await cremaHost.LogoutAsync(authentication1);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task LogoutAsyncTestAsync_Fail3()
        {
            token = await cremaHost.OpenAsync();
            var authenticationToken1 = await cremaHost.LoginAsync("admin", StringUtility.ToSecureString("admin"));
            var authentication1 = await cremaHost.AuthenticateAsync(authenticationToken1);
            await cremaHost.CloseAsync(token);
            token = Guid.Empty;
            await cremaHost.LogoutAsync(authentication1);
        }

        [TestMethod]
        public async Task AuthenticateAsyncTestAsync1()
        {
            token = await cremaHost.OpenAsync();
            var authenticationToken1 = await cremaHost.LoginAsync("admin", StringUtility.ToSecureString("admin"));
            var authentication1 = await cremaHost.AuthenticateAsync(authenticationToken1);
        }

        [TestMethod]
        public async Task AuthenticateAsyncTestAsync2()
        {
            token = await cremaHost.OpenAsync();
            var authenticationToken1 = await cremaHost.LoginAsync("admin", StringUtility.ToSecureString("admin"));
            var authentication = await cremaHost.AuthenticateAsync(Guid.Empty);
            Assert.AreEqual(null, authentication);
        }

        [TestMethod]
        public async Task CloseAsync1()
        {
            token = await cremaHost.OpenAsync();
            cremaHost.Closing += CremaHost_Closing;
            cremaHost.Closed += CremaHost_Closed;
            await cremaHost.CloseAsync(token);
            cremaHost.Closing -= CremaHost_Closing;
            cremaHost.Closed -= CremaHost_Closed;
            token = Guid.Empty;

            static void CremaHost_Closing(object sender, EventArgs e)
            {
                Assert.AreEqual(ServiceState.Closing, cremaHost.ServiceState);
            }

            static void CremaHost_Closed(object sender, ClosedEventArgs e)
            {
                Assert.AreEqual(ServiceState.Closed, cremaHost.ServiceState);
            }
        }

        [TestMethod]
        public async Task CloseAsync2()
        {
            token = await cremaHost.OpenAsync();
            cremaHost.CloseRequested += CremaHost_CloseRequested;

            var dateTime = DateTime.Now;
            await cremaHost.CloseAsync(token);
            cremaHost.CloseRequested -= CremaHost_CloseRequested;
            token = Guid.Empty;
            Assert.IsTrue(DateTime.Now - dateTime > TimeSpan.FromSeconds(3));

            static void CremaHost_CloseRequested(object sender, CloseRequestedEventArgs e)
            {
                Assert.AreEqual(ServiceState.Closing, cremaHost.ServiceState);
                e.AddTask(Task.Delay(3000));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CloseAsync_Fail1()
        {
            token = await cremaHost.OpenAsync();
            await cremaHost.CloseAsync(Guid.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task CloseAsync_Fail2()
        {
            await cremaHost.CloseAsync(Guid.Empty);
        }

        [TestMethod]
        public async Task ShutdownAsyncTestAsync1()
        {
            token = await cremaHost.OpenAsync();
            var manualEvent = new ManualResetEvent(false);
            var authentication = await cremaHost.LoginRandomAsync(Authority.Admin);
            cremaHost.Closed += CremaHost_Closed;
            try
            {
                await cremaHost.ShutdownAsync(authentication, ShutdownContext.None);
                if (manualEvent.WaitOne(10000) == false)
                    Assert.Fail();
            }
            finally
            {
                cremaHost.Closed -= CremaHost_Closed;
            }
            Assert.AreEqual(ServiceState.Closed, cremaHost.ServiceState);
            token = Guid.Empty;

            void CremaHost_Closed(object sender, ClosedEventArgs e)
            {
                manualEvent.Set();
            }
        }

        [TestMethod]
        public async Task ShutdownAsyncTestAsync2()
        {
            token = await cremaHost.OpenAsync();
            var isClosed = false;
            var manualEvent = new ManualResetEvent(false);
            var authentication = await cremaHost.LoginRandomAsync(Authority.Admin);
            var shutdownContext = new ShutdownContext() { IsRestart = true };
            cremaHost.Opened += CremaHost_Opened;
            cremaHost.Closed += CremaHost_Closed;
            try
            {
                await cremaHost.ShutdownAsync(authentication, shutdownContext);
                if (manualEvent.WaitOne(20000) == false)
                    Assert.Fail();
            }
            finally
            {
                cremaHost.Opened -= CremaHost_Opened;
                cremaHost.Closed -= CremaHost_Closed;
            }
            Assert.AreEqual(ServiceState.Open, cremaHost.ServiceState);
            Assert.IsTrue(isClosed);

            void CremaHost_Closed(object sender, ClosedEventArgs e)
            {
                isClosed = true;
            }

            void CremaHost_Opened(object sender, EventArgs e)
            {
                manualEvent.Set();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ShutdownAsyncTestAsync_Fail1()
        {
            token = await cremaHost.OpenAsync();
            var authentication = await cremaHost.LoginRandomAsync(Authority.Admin);
            await cremaHost.ShutdownAsync(authentication, null);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task ShutdownAsyncTestAsync_Fail2()
        {
            token = await cremaHost.OpenAsync();
            var authentication1 = await cremaHost.LoginRandomAsync(Authority.Member);
            await cremaHost.ShutdownAsync(authentication1, ShutdownContext.None);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task ShutdownAsyncTestAsync_Fail3()
        {
            token = await cremaHost.OpenAsync();
            var authentication1 = await cremaHost.LoginRandomAsync(Authority.Admin);
            var shutdownContext = new ShutdownContext()
            {
                Milliseconds = -1
            };
            await cremaHost.ShutdownAsync(authentication1, shutdownContext);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ShutdownAsyncTestAsync_Fail4()
        {
            token = await cremaHost.OpenAsync();
            var authentication1 = await cremaHost.LoginRandomAsync(Authority.Admin);
            var shutdownContext = new ShutdownContext()
            {
                Message = null
            };
            await cremaHost.ShutdownAsync(authentication1, shutdownContext);
        }

        [TestMethod]
        public async Task CancelShutdownAsyncTestAsync1()
        {
            token = await cremaHost.OpenAsync();
            var authentication1 = await cremaHost.LoginRandomAsync(Authority.Admin);
            var shutdownContext = new ShutdownContext()
            {
                Milliseconds = 5000
            };
            await cremaHost.ShutdownAsync(authentication1, shutdownContext);
            await cremaHost.CancelShutdownAsync(authentication1);
            await Task.Delay(1000);
            Assert.AreEqual(ServiceState.Open, cremaHost.ServiceState);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task CancelShutdownAsyncTestAsync2()
        {
            token = await cremaHost.OpenAsync();
            var authentication1 = await cremaHost.LoginRandomAsync(Authority.Admin);
            await cremaHost.CancelShutdownAsync(authentication1);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task CancelShutdownAsyncTestAsync_Fail1()
        {
            token = await cremaHost.OpenAsync();
            var authentication1 = await cremaHost.LoginRandomAsync(Authority.Member);
            await cremaHost.CancelShutdownAsync(authentication1);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task CancelShutdownAsyncTestAsync_Fail2()
        {
            token = await cremaHost.OpenAsync();
            var authentication1 = await cremaHost.LoginRandomAsync(Authority.Member);
            await cremaHost.CloseAsync(token);
            token = Guid.Empty;
            await cremaHost.CancelShutdownAsync(authentication1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CancelShutdownAsyncTestAsync_Fail3()
        {
            token = await cremaHost.OpenAsync();
            await cremaHost.CancelShutdownAsync(null);
        }

        public TestContext TestContext { get; set; }
    }
}