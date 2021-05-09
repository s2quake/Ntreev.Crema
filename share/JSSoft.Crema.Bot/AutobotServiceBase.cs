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

using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using JSSoft.Crema.Services.Extensions;
using JSSoft.Library;
using JSSoft.Library.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace JSSoft.Crema.Bot
{
    public abstract class AutobotServiceBase
    {
        private const string masterBotID = "smith";
        private const string masterBotName = "Smith";
        private readonly ICremaHost cremaHost;
        private readonly Dictionary<string, AutobotBase> botByID = new();

        protected AutobotServiceBase(ICremaHost cremaHost, IEnumerable<ITaskProvider> taskProviders)
        {
            this.cremaHost = cremaHost;
            this.cremaHost.Opened += CremaHost_Opened;
            this.cremaHost.CloseRequested += CremaHost_CloseRequested;
            this.TaskProviders = taskProviders.ToArray();
        }

        public async Task CreateAutobotAsync(Authentication authentication, string autobotID, SecureString password)
        {
            if (this.ServiceState != ServiceState.Open)
                throw new InvalidOperationException();

            await this.Dispatcher.InvokeAsync(() =>
            {
                var autobots = CreateAutobots(new string[] { autobotID }, new SecureString[] { password });
                this.StartAutobots(autobots);
            });
        }

        public Task StartAsync(Authentication authentication)
        {
            return this.StartAsync(authentication, 0);
        }

        public async Task StartAsync(Authentication authentication, int count)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                if (this.ServiceState != ServiceState.None)
                    throw new InvalidOperationException();
                this.ServiceState = ServiceState.Opening;
            });

            try
            {
                var masterBotPassword = StringUtility.ToSecureString("1111");
                var autobotIDList = new List<string>(count + 1) { masterBotID };
                var autobotPasswordList = new List<SecureString>(count + 1) { masterBotPassword };
                var userContext = this.cremaHost.GetService(typeof(IUserContext)) as IUserContext;
                if (await userContext.Users.ContainsAsync(masterBotID) == false)
                {
                    var category = userContext.Categories.Random();
                    await category.AddNewUserAsync(authentication, masterBotID, masterBotPassword, masterBotName, Authority.Admin);
                }
                for (var i = 0; i < count; i++)
                {
                    var info = this.GetRandomUserInfo();
                    if (await userContext.Users.ContainsAsync(info.ID) == false)
                    {
                        var category = userContext.Categories.Random();
                        await category.AddNewUserAsync(authentication, info.ID, info.Password, info.Name, info.Authority);
                    }
                    autobotIDList.Add(info.ID);
                    autobotPasswordList.Add(info.Password);
                }
                await this.Dispatcher.InvokeAsync(() =>
                {
                    var autobots = CreateAutobots(autobotIDList.ToArray(), autobotPasswordList.ToArray());
                    this.StartAutobots(autobots);
                    this.ServiceState = ServiceState.Open;
                });
            }
            catch
            {
                await this.Dispatcher.InvokeAsync(() => this.ServiceState = ServiceState.None);
                throw;
            }
        }

        public async Task StopAsync()
        {
            var tasks = await this.Dispatcher.InvokeAsync(() =>
            {
                if (this.ServiceState != ServiceState.Open)
                    throw new InvalidOperationException();
                this.ServiceState = ServiceState.Closing;
                return this.botByID.Values.Select(item => item.CancelAsync()).ToArray();
            });

            await Task.WhenAll(tasks);
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.ServiceState = ServiceState.Closed;
            });
        }

        public ServiceState ServiceState { get; set; }

        public ITaskProvider[] TaskProviders { get; }

        public bool AllowException { get; set; }

        public CremaDispatcher Dispatcher { get; private set; }

        protected abstract AutobotBase CreateInstance(string autobotID, SecureString password);

        private async void Autobot_Disposed(object sender, EventArgs e)
        {
            if (sender is AutobotBase autobot)
            {
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.botByID.Remove(autobot.AutobotID);
                });
            }
        }

        private void CremaHost_Opened(object sender, EventArgs e)
        {
            this.Dispatcher = new CremaDispatcher(this);
        }

        private void CremaHost_CloseRequested(object sender, CloseRequestedEventArgs e)
        {
            e.AddTask(this.DisposeAsync());
        }

        internal (string ID, string Name, SecureString Password, Authority Authority) GetRandomUserInfo()
        {
            var number = RandomUtility.Next(1000);
            var value = RandomUtility.Next(3);
            if (value == 0)
                return ($"admin{number}", $"Admin{number}", StringUtility.ToSecureString("admin"), Authority.Admin);
            else if (value == 1)
                return ($"member{number}", $"Member{number}", StringUtility.ToSecureString("member"), Authority.Member);
            return ($"guest{number}", $"Guest{number}", StringUtility.ToSecureString("guest"), Authority.Guest);
        }

        private AutobotBase[] CreateAutobots(string[] autobotIDs, SecureString[] passwords)
        {
            this.Dispatcher.VerifyAccess();
            var autobotList = new List<AutobotBase>(autobotIDs.Length);
            for (var i = 0; i < autobotIDs.Length; i++)
            {
                try
                {
                    if (this.botByID.ContainsKey(autobotIDs[i]) == true)
                        throw new InvalidOperationException();

                    var autobot = this.CreateInstance(autobotIDs[i], passwords[i]);
                    autobotList.Add(autobot);
                }
                catch { }
            }
            return autobotList.ToArray();
        }

        private void StartAutobots(AutobotBase[] autobots)
        {
            foreach (var item in autobots)
            {
                item.Disposed += Autobot_Disposed;
                this.botByID.Add(item.AutobotID, item);
                item.ExecuteAsync(this.TaskProviders);
            }
        }

        private async Task DisposeAsync()
        {
            if (this.ServiceState == ServiceState.Open)
            {
                await this.StopAsync();
            }
            await this.Dispatcher.DisposeAsync();
        }
    }
}
