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

using JSSoft.Crema.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Threading.Tasks;

namespace JSSoft.Crema.Bot
{
    class Autobot : AutobotBase
    {
        private readonly AutobotService service;
        private readonly string address;
        private readonly SecureString password;
        private AutobotCremaBootstrapper app = new AutobotCremaBootstrapper() { Verbose = LogVerbose.None };
        private ICremaHost cremaHost;
        private Guid token;

        public Autobot(AutobotService service, string address, string autobotID, SecureString password)
            : base(autobotID)
        {
            this.service = service;
            this.address = address;
            this.password = password;
        }

        public override object GetService(Type serviceType)
        {
            if (this.app.GetService(typeof(ICremaHost)) is ICremaHost cremaHost)
            {
                return cremaHost.GetService(serviceType);
            }
            return this.app.GetService(serviceType);
        }

        public override AutobotServiceBase Service => this.service;

        protected override async Task<Authentication> OnLoginAsync()
        {
            this.cremaHost = this.app.GetService(typeof(ICremaHost)) as ICremaHost;
            this.token = await this.cremaHost.OpenAsync(this.address, this.AutobotID, this.password);
            var autheticator = this.app.GetService(typeof(Authenticator)) as Authenticator;
            await this.cremaHost.Dispatcher.InvokeAsync(() => this.cremaHost.Closed += CremaHost_Closed);
            return autheticator;
        }

        private void CremaHost_Closed(object sender, ClosedEventArgs e)
        {

        }

        protected override async Task OnLogoutAsync(Authentication authentication)
        {
            await this.cremaHost.Dispatcher.InvokeAsync(() => this.cremaHost.Closed -= CremaHost_Closed);
            await this.cremaHost.CloseAsync(this.token);
        }

        protected override void OnDisposed(EventArgs e)
        {
            base.OnDisposed(e);

            //if (this.cremaHost != null && this.cremaHost.IsOpened == true)
            //{
            //    this.cremaHost.Dispatcher.Invoke(() => this.cremaHost.Close(this.token));
            //}
            this.token = Guid.Empty;
            this.app?.Dispose();
            this.app = null;
        }

        #region classes

        class AutobotCremaBootstrapper : CremaBootstrapper
        {
            public override IEnumerable<Tuple<Type, object>> GetParts()
            {
                foreach (var item in base.GetParts())
                {
                    yield return item;
                }
                var autheticator = new Authenticator();
                yield return new Tuple<Type, object>(typeof(IPlugin), autheticator);
                yield return new Tuple<Type, object>(typeof(Authenticator), autheticator);
            }

            public override IEnumerable<Assembly> GetAssemblies()
            {
                var itemList = base.GetAssemblies().ToList();

                return itemList;
            }
        }

        class Authenticator : AuthenticatorBase
        {

        }

        #endregion
    }
}
