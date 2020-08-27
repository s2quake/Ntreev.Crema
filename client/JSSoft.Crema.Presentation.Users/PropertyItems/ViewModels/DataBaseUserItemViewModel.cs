﻿//Released under the MIT License.
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

using JSSoft.Crema.ServiceModel;
using JSSoft.ModernUI.Framework.ViewModels;
using System;

namespace JSSoft.Crema.Presentation.Users.PropertyItems.ViewModels
{
    class DataBaseUserItemViewModel : ListBoxItemViewModel
    {
        private AuthenticationInfo authenticationInfo;

        public DataBaseUserItemViewModel(IServiceProvider serviceProvier, AuthenticationInfo authenticationInfo)
            : base(serviceProvier)
        {
            this.authenticationInfo = authenticationInfo;
        }

        public string ID => this.authenticationInfo.ID;

        public string Name => this.authenticationInfo.Name;

        public override string DisplayName => this.authenticationInfo.ID + " [" + this.authenticationInfo.Name + "]";

        public Authority Authority => this.authenticationInfo.Authority;

        public AuthenticationInfo AuthenticationInfo
        {
            get => this.authenticationInfo;
            set
            {
                this.authenticationInfo = value;
                this.Refresh();
            }
        }
    }
}
