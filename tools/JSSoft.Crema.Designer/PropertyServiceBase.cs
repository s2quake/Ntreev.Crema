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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSSoft.Crema.Designer
{
    public abstract class PropertyServiceBase : PropertyChangedBase, IPropertyService
    {
        private readonly IPropertyItem[] propertyItems;

        private IPropertyItem[] itemsSource;
        private object selectedObject;

        protected PropertyServiceBase(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            //this.cremaHost = cremaHost;
            //this.cremaHost.Opened += CremaHost_Opened;
            //this.cremaAppHost = cremaAppHost;
            //this.cremaAppHost.Loaded += CremaAppHost_Loaded;
            //this.cremaAppHost.Unloaded += CremaAppHost_Unloaded;

            this.itemsSource = this.propertyItems;
        }

        public object SelectedObject
        {
            get { return this.selectedObject; }
            set
            {
                if (this.selectedObject == value)
                    return;
                this.selectedObject = value;
                this.RefreshItems();
                this.OnSelectionChanged(EventArgs.Empty);
            }
        }

        public void RefreshItems()
        {
            foreach (var item in this.propertyItems)
            {
                if (item.CanSupport(this.selectedObject) == true)
                {
                    item.SelectObject(this.selectedObject);
                }
                else
                {
                    item.SelectObject(null);
                }
            }
        }

        public IEnumerable<IPropertyItem> Properties
        {
            get { return this.itemsSource; }
        }

        public event EventHandler SelectionChanged;

        protected virtual void OnSelectionChanged(EventArgs e)
        {
            this.SelectionChanged?.Invoke(this, e);
        }
    }
}
