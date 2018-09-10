//Released under the MIT License.
//
//Copyright (c) 2018 Ntreev Soft co., Ltd.
//
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
//documentation files (the "Software"), to deal in the Software without restriction, including without limitation the 
//rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit 
//persons to whom the Software is furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the 
//Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
//WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR 
//COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
//OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services;
using Ntreev.Library;
using Ntreev.Library.Random;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ntreev.Crema.Bot
{
    public abstract class AutobotServiceBase
    {
        private const string masterBotID = "Smith";
        private readonly ICremaHost cremaHost;
        private readonly IEnumerable<ITaskProvider> taskProviders;
        private readonly Dictionary<string, AutobotBase> botByID = new Dictionary<string, AutobotBase>();

        protected AutobotServiceBase(ICremaHost cremaHost, IEnumerable<ITaskProvider> taskProviders)
        {
            this.cremaHost = cremaHost;
            this.cremaHost.Closing += CremaHost_Closing;
            this.taskProviders = taskProviders;
        }

        public async Task CreateAutobotAsync(Authentication authentication, string autobotID)
        {
            if (this.IsPlaying == false || this.IsClosing == true)
                throw new InvalidOperationException();

            await this.Dispatcher.InvokeAsync(() =>
            {
                if (this.botByID.ContainsKey(autobotID) == true)
                    throw new InvalidOperationException();

                var autobot = this.CreateInstance(autobotID);
                autobot.AllowException = this.AllowException;
                this.botByID.Add(autobotID, autobot);
                autobot.Disposed += Autobot_Disposed;
                autobot.ExecuteAsync(this.taskProviders);
            });
        }

        public async Task StartAsync(Authentication authentication)
        {
            if (this.IsPlaying == true || this.IsClosing == true)
                throw new InvalidOperationException();

            this.IsPlaying = true;
            var userContext = this.cremaHost.GetService(typeof(IUserContext)) as IUserContext;
            if (await userContext.Dispatcher.InvokeAsync(() => userContext.Categories.Contains("/autobots/")) == false)
            {
                await userContext.Root.AddNewCategoryAsync(authentication, "autobots");
            }
            if (await userContext.Dispatcher.InvokeAsync(() => userContext.Users.Contains(masterBotID)) == false)
            {
                var category = userContext.Categories["/autobots/"];
                await category.AddNewUserAsync(authentication, masterBotID, StringUtility.ToSecureString("1111"), masterBotID, Authority.Admin);
            }


            var autobotIDList = new List<string>();

            for (var i = 0; i < 3; i++)
            {
                var autobotID = $"Autobot{RandomUtility.Next(1000)}";
                var authority = RandomUtility.NextEnum<Authority>();
                //var authentication = context.Authentication;
                //var userContext = autobot.CremaHost.GetService(typeof(IUserContext)) as IUserContext;
                if (await userContext.Dispatcher.InvokeAsync(() => userContext.Users.Contains(autobotID)) == false)
                {
                    var category = userContext.Categories["/autobots/"];
                    await category.AddNewUserAsync(authentication, autobotID, StringUtility.ToSecureString("1111"), autobotID, authority);
                }
                autobotIDList.Add(autobotID);
            }

            this.Dispatcher = new CremaDispatcher(this);
            await this.CreateAutobotAsync(authentication, masterBotID);
            foreach (var item in autobotIDList)
            {
                await this.CreateAutobotAsync(authentication, item);
            }
        }

        public async Task StopAsync()
        {
            if (this.IsPlaying == false || this.IsClosing == true)
                throw new InvalidOperationException();

            this.IsPlaying = false;
            this.IsClosing = true;
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.Dispatcher.Dispose();
                this.Dispatcher = null;
            });
            foreach (var item in this.botByID.Values)
            {
                item.Cancel();
            }
            while (this.botByID.Any())
            {
                Thread.Sleep(1);
            }
            this.botByID.Clear();
            this.IsClosing = false;
        }

        public bool IsPlaying { get; private set; }

        public bool IsClosing { get; private set; }

        public ITaskProvider[] TaskProviders
        {
            get { return this.taskProviders.ToArray(); }
        }

        public bool AllowException
        {
            get; set;
        }

        public CremaDispatcher Dispatcher { get; private set; }

        protected abstract AutobotBase CreateInstance(string autobotID);

        private void Autobot_Disposed(object sender, EventArgs e)
        {
            if (sender is AutobotBase autobot)
            {
                lock (this.botByID)
                {
                    this.botByID.Remove(autobot.AutobotID);
                }
            }
        }

        private void CremaHost_Closing(object sender, EventArgs e)
        {
            if (this.IsPlaying == true)
                this.StopAsync().Wait();
        }
    }
}
