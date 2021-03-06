﻿// Released under the MIT License.
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

using Caliburn.Micro;
using JSSoft.Crema.Presentation.Framework;
using JSSoft.Crema.Presentation.Users.Properties;
using JSSoft.Crema.Presentation.Users.PropertyItems.Views;
using JSSoft.Crema.ServiceModel;
using JSSoft.Library;
using JSSoft.ModernUI.Framework;
using System;
using System.ComponentModel.Composition;

namespace JSSoft.Crema.Presentation.Users.PropertyItems.ViewModels
{
    [View(typeof(EditorsView))]
    [Export(typeof(IPropertyItem))]
    [RequiredAuthority(Authority.Guest)]
    [Dependency("JSSoft.Crema.Presentation.Tables.PropertyItems.ViewModels.TableInfoViewModel, JSSoft.Crema.Presentation.Tables, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null")]
    [ParentType("JSSoft.Crema.Presentation.Tables.IPropertyService, JSSoft.Crema.Presentation.Tables, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null")]
    class TableTemplateEditorsViewModel : EditorsViewModel
    {
        [ImportingConstructor]
        public TableTemplateEditorsViewModel(Authenticator authenticator, ICremaAppHost cremaAppHost)
            : base(authenticator, cremaAppHost: cremaAppHost)
        {
            this.DisplayName = Resources.Title_UsersEditingTemplate;
        }

        public override bool CanSupport(object obj)
        {
            return obj is ITableDescriptor;
        }

        public override string GetItemPath(object obj)
        {
            if (obj is ITableDescriptor descriptor)
            {
                return descriptor.Path;
            }
            throw new NotImplementedException();
        }

        public override string ItemType => "TableTemplate";

        //protected override bool IsDomain(DomainInfo domainInfo, object obj)
        //{
        //    var descriptor = obj as ITableDescriptor;
        //    var path = descriptor.TableInfo.CategoryPath + descriptor.TableInfo.Name;
        //    if (domainInfo.ItemPath != path || domainInfo.ItemType != "TableTemplate")
        //        return false;
        //    return true;
        //}
    }
}