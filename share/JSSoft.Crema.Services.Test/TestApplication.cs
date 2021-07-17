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
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using JSSoft.Crema.Services.Extensions;
using JSSoft.Library.Random;
using JSSoft.Crema.Services.Test.Common;
using JSSoft.Crema.Services.Random;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JSSoft.Library.IO;
using JSSoft.Crema.Data;
using JSSoft.Library;
using JSSoft.Crema.Services.Test.Common.Extensions;

namespace JSSoft.Crema.Services.Test
{
    public partial class TestApplication : CremaBootstrapper
    {
        private ICremaHost cremaHost;
        private Guid token;
        private UserContextMetaData userInfos;
        private Authentication expiredAuthentication;

        public new object GetService(Type serviceType)
        {
            if (serviceType == typeof(ICremaHost))
                return base.GetService(serviceType);
            if (this.cremaHost.GetService(serviceType) is object service)
                return service;
            return base.GetService(serviceType);
        }

        public async Task OpenAsync()
        {
            var cremaHost = this.cremaHost;
            var token = await cremaHost.OpenAsync();
            var userContext = cremaHost.GetService(typeof(IUserContext)) as IUserContext;
            var userID = Authentication.AdminID;
            var password = Authentication.AdminID.ToSecureString();
            var authenticationToken = await cremaHost.LoginAsync(userID, password);
            var authentication = await cremaHost.AuthenticateAsync(authenticationToken);
            var userInfos = await userContext.Dispatcher.InvokeAsync(() => userContext.GetMetaData());
            await cremaHost.LogoutAsync(authentication);
            this.token = token;
            this.userInfos = userInfos;
            this.expiredAuthentication = authentication;
        }

        public async Task CloseAsync()
        {
            await this.cremaHost.CloseAsync(this.token);
            this.token = Guid.Empty;
        }

        public Task<Authentication> LoginRandomAsync()
        {
            var items = new Authority[] { Authority.Admin, Authority.Member, Authority.Guest };
            return LoginRandomAsync(items.Random());
        }

        public Task<Authentication> LoginRandomAsync(Authority authority)
        {
            return LoginRandomAsync(authority, DefaultPredicate);
        }

        public async Task<Authentication> LoginRandomAsync(Authority authority, Func<IUser, bool> predicate)
        {
            var cremaHost = this.cremaHost;
            if (cremaHost.GetService(typeof(IUserCollection)) is IUserCollection userCollection)
            {
                var user = await userCollection.GetRandomUserAsync(Test);
                var name = user.ID;
                var password = user.GetPassword();
                var token = await cremaHost.LoginAsync(name, password);
                return await cremaHost.AuthenticateAsync(token);
            }
            throw new NotImplementedException();

            bool Test(IUser user)
            {
                if (user.BanInfo.Path != string.Empty)
                    return false;
                if (user.UserState == UserState.Online)
                    return false;
                if (user.Authority != authority)
                    return false;
                return predicate(user);
            }
        }

        public Task LogoutAsync(Authentication authentication)
        {
            return this.cremaHost.LogoutAsync(authentication);
        }

        public Authentication ExpiredAuthentication => this.expiredAuthentication;

        private static bool DefaultPredicate(IUser _) => true;
    }
}
