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

using System;
using System.ComponentModel;
using System.Data;

namespace Ntreev.Crema.ServiceModel
{
    internal abstract class DomainUserBase
    {
        private DomainUserInfo domainUserInfo = DomainUserInfo.Empty;
        private DomainLocationInfo domainLocationInfo = DomainLocationInfo.Empty;
        private DomainUserState domainUserState;
        private PropertyCollection extendedProperties;

        protected DomainUserBase()
        {

        }

        public DomainUserInfo DomainUserInfo
        {
            get => this.domainUserInfo;
            set
            {
                this.domainUserInfo = value;
                this.OnDomainUserInfoChanged(EventArgs.Empty);
            }
        }

        public DomainLocationInfo DomainLocationInfo
        {
            get => this.domainLocationInfo;
            set
            {
                this.domainLocationInfo = value;
                this.OnDomainLocationInfoChanged(EventArgs.Empty);
            }
        }

        public DomainUserState DomainUserState
        {
            get => this.domainUserState;
            set
            {
                this.domainUserState = value;
                this.OnDomainUserStateChanged(EventArgs.Empty);
            }
        }

        public bool IsBeingEdited
        {
            get => this.DomainUserState.HasFlag(DomainUserState.IsBeingEdited);
            set
            {
                if (value == true)
                    this.DomainUserState |= DomainUserState.IsBeingEdited;
                else
                    this.DomainUserState &= ~DomainUserState.IsBeingEdited;
            }
        }

        public bool IsModified
        {
            get => this.DomainUserState.HasFlag(DomainUserState.IsModified);
            set
            {
                if (value == true)
                    this.DomainUserState |= DomainUserState.IsModified;
                else
                    this.DomainUserState &= ~DomainUserState.IsModified;
            }
        }

        public bool IsOnline
        {
            get => this.DomainUserState.HasFlag(DomainUserState.Online);
            set
            {
                if (value == true)
                    this.DomainUserState |= DomainUserState.Online;
                else
                    this.DomainUserState &= ~DomainUserState.Online;
            }
        }

        public bool IsOwner
        {
            get => this.DomainUserState.HasFlag(DomainUserState.IsOwner);
            set
            {
                if (value == true)
                    this.DomainUserState |= DomainUserState.IsOwner;
                else
                    this.DomainUserState &= ~DomainUserState.IsOwner;
            }
        }

        public bool CanRead => this.domainUserInfo.AccessType.HasFlag(DomainAccessType.Read);

        public bool CanWrite => this.domainUserInfo.AccessType.HasFlag(DomainAccessType.ReadWrite);

        [Browsable(false)]
        public PropertyCollection ExtendedProperties
        {
            get
            {
                if (this.extendedProperties == null)
                {
                    this.extendedProperties = new PropertyCollection();
                }
                return this.extendedProperties;
            }
        }

        public event EventHandler DomainUserInfoChanged;

        public event EventHandler DomainLocationInfoChanged;

        public event EventHandler DomainUserStateChanged;

        protected virtual void OnDomainUserInfoChanged(EventArgs e)
        {
            this.DomainUserInfoChanged?.Invoke(this, e);
        }

        protected virtual void OnDomainLocationInfoChanged(EventArgs e)
        {
            this.DomainLocationInfoChanged?.Invoke(this, e);
        }

        protected virtual void OnDomainUserStateChanged(EventArgs e)
        {
            this.DomainUserStateChanged?.Invoke(this, e);
        }
    }
}
