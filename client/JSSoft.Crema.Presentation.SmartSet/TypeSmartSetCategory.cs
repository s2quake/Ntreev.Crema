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

using JSSoft.Library.ObjectModel;
using System;

namespace JSSoft.Crema.Presentation.SmartSet
{
    class TypeSmartSetCategory : CategoryBase<TypeSmartSet, TypeSmartSetCategory, TypeSmartSetCollection, TypeSmartSetCategoryCollection, TypeSmartSetContext>,
        ISmartSetCategory, IServiceProvider
    {
        public TypeSmartSetCategory()
        {

        }

        #region ISmartSetCategory

        ISmartSetCategory ISmartSetCategory.Parent => this.Parent;

        IContainer<ISmartSetCategory> ISmartSetCategory.Categories => this.Categories;

        IContainer<ISmartSet> ISmartSetCategory.Items => this.Items;


        string ISmartSetCategory.ParentPath
        {
            get => this.Parent.Path;
            set => this.Parent = this.Container[value];
        }

        ISmartSetCategory ISmartSetCategory.CreateCategory(string name)
        {
            return new TypeSmartSetCategory()
            {
                Name = name,
                Parent = this,
            };
        }

        ISmartSet ISmartSetCategory.CreateItem(string name)
        {
            return new TypeSmartSet()
            {
                Name = name,
                Category = this,
            };
        }

        #endregion

        #region IServiceProvider

        public object GetService(Type serviceType)
        {
            return this.Context.GetService(serviceType);
        }

        #endregion
    }
}
