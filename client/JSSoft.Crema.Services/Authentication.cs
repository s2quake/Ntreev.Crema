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
using JSSoft.Crema.Services.Properties;
using JSSoft.Crema.Services.Users;
using JSSoft.Library;
using System;

namespace JSSoft.Crema.Services
{
    public sealed class Authentication : IAuthentication
    {
        private readonly static object lockobj = new();
        internal static string SystemID = "system";
        internal static string SystemName = "System";
        internal static string AdminID = "admin";
        internal static string AdminName = "Administrator";

        private readonly IAuthenticationProvider provider;
        private readonly Guid token;
        private SignatureDate signatureDate;
        private EventHandler expired;
        private bool isExpired;
        private Authentication child;

        private Authentication()
        {

        }

        internal Authentication(IAuthenticationProvider provider)
            : this(provider, Guid.Empty)
        {

        }

        internal Authentication(IAuthenticationProvider provider, Guid token)
        {
            this.provider = provider;
            this.token = token;
        }

        public Authentication BeginCommission()
        {
            if (this.Parent != null || this.isExpired == true)
                throw new InvalidOperationException(Resources.Exception_Expired);
            if (this.child != null)
                throw new InvalidOperationException(Resources.Exception_Commissioned);
            var authentication = new Authentication(new UserAuthenticationProvider(this.provider.ID, this.provider.Name, this.provider.Authority, this.provider.AuthenticationTypes), this.token)
            {
                signatureDate = this.signatureDate,
                Parent = this,
            };
            this.child = authentication;
            return authentication;
        }

        public void EndCommission(Authentication authentication)
        {
            if (this.child == null)
                throw new InvalidOperationException();
            if (this.child != authentication)
                throw new InvalidOperationException();
            this.child.isExpired = true;
            this.child = null;
        }

        public override string ToString()
        {
            return $"{this.provider.ID}({this.provider.Name})";
        }

        public string ID
        {
            get
            {
                if (this.isExpired == true)
                    throw new InvalidOperationException();
                return this.provider.ID;
            }
        }

        public string Name
        {
            get
            {
                if (this.isExpired == true)
                    throw new InvalidOperationException();
                return this.provider.Name;
            }
        }

        public Authority Authority => this.provider.Authority;

        public SignatureDate SignatureDate
        {
            get => this.signatureDate;
            internal set
            {
                if (this.ID != value.ID)
                    throw new ArgumentException(Resources.Exception_InvalidID, nameof(value));
                this.signatureDate = value;
            }
        }

        public AuthenticationInfo AuthenticationInfo => new()
        {
            ID = this.ID,
            Name = this.Name,
            Authority = this.Authority,
        };

        public event EventHandler Expired
        {
            add
            {
                lock (lockobj)
                {
                    if (this.Parent != null)
                        this.Parent.expired += value;
                    else
                        this.expired += value;
                }

            }
            remove
            {
                lock (lockobj)
                {
                    if (this.Parent != null)
                        this.Parent.expired -= value;
                    else
                        this.expired -= value;
                }
            }
        }

        internal void InvokeExpiredEvent(string userID)
        {
            this.InvokeExpiredEvent(userID, string.Empty);
        }

        internal void InvokeExpiredEvent(string _1, string _2)
        {
            lock (lockobj)
            {
                this.isExpired = true;
                this.expired?.Invoke(this, EventArgs.Empty);
            }
        }

        internal SignatureDate Sign()
        {
            return this.Sign(DateTime.UtcNow);
        }

        internal SignatureDate Sign(DateTime _)
        {
            this.signatureDate = new SignatureDate(this.ID, DateTime.UtcNow);
            return this.signatureDate;
        }

        internal AuthenticationType Types => this.provider.AuthenticationTypes;

        internal Authentication Parent { get; private set; }

        internal bool IsAdmin => this.Types.HasFlag(AuthenticationType.Administrator);

        internal bool IsSystem => this.Types.HasFlag(AuthenticationType.System);

        internal readonly static Authentication System = new(new SystemAuthenticationProvider(), Guid.Parse("62E5A6E9-D4BE-438F-A188-D5842C0ED65E"));

        #region IAuthentication

        AuthenticationType IAuthentication.Types => this.Types;

        bool IAuthentication.IsAdmin => this.Types.HasFlag(AuthenticationType.Administrator);

        bool IAuthentication.IsSystem => this.Types.HasFlag(AuthenticationType.System);

        #endregion

        #region class

        class SystemAuthenticationProvider : IAuthenticationProvider
        {
            public AuthenticationType AuthenticationTypes => AuthenticationType.Administrator;

            public Authority Authority => Authority.Admin;

            public string ID => Authentication.SystemID;

            public string Name => Authentication.SystemName;
        }

        #endregion
    }
}
