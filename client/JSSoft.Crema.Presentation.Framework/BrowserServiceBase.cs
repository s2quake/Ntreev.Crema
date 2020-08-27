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

using JSSoft.ModernUI.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace JSSoft.Crema.Presentation.Framework
{
    public abstract class BrowserServiceBase : PropertyChangedBase, IBrowserService
    {
        private readonly ICremaAppHost cremaAppHost;
        private readonly IBrowserItem[] browserItems;

        private readonly ObservableCollection<IBrowserItem> itemsSource = new ObservableCollection<IBrowserItem>();

        protected BrowserServiceBase(ICremaAppHost cremaAppHost, IEnumerable<IBrowserItem> browserItems)
        {
            this.cremaAppHost = cremaAppHost;
            this.cremaAppHost.Opened += CremaAppHost_Opened;
            this.browserItems = ParentItemUtility.GetItems(this, browserItems).ToArray();
        }

        public IEnumerable<IBrowserItem> Browsers => this.itemsSource;

        public ObservableCollection<IBrowserItem> ItemsSource => this.itemsSource;

        private async void CremaAppHost_Opened(object sender, EventArgs e)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                var query = from item in this.browserItems
                            let attr = Attribute.GetCustomAttribute(item.GetType(), typeof(RequiredAuthorityAttribute), true) as RequiredAuthorityAttribute
                            where attr == null || this.cremaAppHost.Authority >= attr.Authority
                            select item;

                this.itemsSource.Clear();
                foreach (var item in query)
                {
                    this.itemsSource.Add(item);
                }
                this.NotifyOfPropertyChange(nameof(this.ItemsSource));
            });
        }
    }
}
