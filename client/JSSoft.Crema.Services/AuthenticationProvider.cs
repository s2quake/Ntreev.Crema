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

namespace JSSoft.Crema.Services
{
    class AuthenticationProvider : IAuthenticationProvider
    {
        private readonly CremaHost cremaHost;

        public AuthenticationProvider(CremaHost cremaHost)
        {
            this.cremaHost = cremaHost;
            // this.isUser = true;
            // this.ID = user.ID;
            // this.Name = user.UserName;
            // this.Authority = user.Authority;
        }

        // public AuthenticationProvider(string name)
        // {
        //     this.ID = name;
        //     this.Name = name;
        //     this.Authority = Authority.Guest;
        // }

        internal AuthenticationType AuthenticationTypes
        {
            get
            {
                var authority = this.Authority;
                if (authority == Authority.Admin)
                    return AuthenticationType.User | AuthenticationType.Administrator;
                else if (authority == Authority.Member)
                    return AuthenticationType.User;
                else
                    return AuthenticationType.User;
            }
        }

        public Authority Authority => this.cremaHost.Authority;

        public string ID => this.cremaHost.UserID;

        public string Name => this.cremaHost.UserName;

        #region IAuthenticationProvider

        AuthenticationType IAuthenticationProvider.AuthenticationTypes => this.AuthenticationTypes;

        #endregion
    }
}
