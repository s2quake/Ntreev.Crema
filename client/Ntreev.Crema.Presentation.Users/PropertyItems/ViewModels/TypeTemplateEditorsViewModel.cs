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

using Caliburn.Micro;
using Ntreev.Crema.Presentation.Framework;
using Ntreev.Crema.Presentation.Users.Properties;
using Ntreev.Crema.Presentation.Users.PropertyItems.Views;
using Ntreev.Crema.ServiceModel;
using Ntreev.Library;
using Ntreev.ModernUI.Framework;
using System;
using System.ComponentModel.Composition;

namespace Ntreev.Crema.Presentation.Users.PropertyItems.ViewModels
{
    [View(typeof(EditorsView))]
    [Export(typeof(IPropertyItem))]
    [RequiredAuthority(Authority.Guest)]
    [Dependency("Ntreev.Crema.Presentation.Types.PropertyItems.ViewModels.TypeInfoViewModel, Ntreev.Crema.Presentation.Types, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null")]
    [ParentType("Ntreev.Crema.Presentation.Types.IPropertyService, Ntreev.Crema.Presentation.Types, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null")]
    class TypeTemplateEditorsViewModel : EditorsViewModel
    {
        [ImportingConstructor]
        public TypeTemplateEditorsViewModel(Authenticator authenticator, ICremaAppHost cremaAppHost)
            : base(authenticator, cremaAppHost: cremaAppHost)
        {
            this.DisplayName = Resources.Title_UsersEditingTemplate;
        }

        public override bool CanSupport(object obj)
        {
            return obj is ITypeDescriptor;
        }

        public override string GetItemPath(object obj)
        {
            if (obj is ITypeDescriptor descriptor)
            {
                return descriptor.Path;
            }
            throw new NotImplementedException();
        }

        public override string ItemType => "TypeTemplate";

        //protected override bool IsDomain(DomainInfo domainInfo, object obj)
        //{
        //    var descriptor = obj as ITypeDescriptor;
        //    var path = descriptor.TypeInfo.CategoryPath + descriptor.TypeInfo.Name;
        //    if (domainInfo.ItemPath != path || domainInfo.ItemType != "TypeTemplate")
        //        return false;
        //    return true;
        //}
    }
}
