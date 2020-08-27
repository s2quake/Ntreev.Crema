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

using JSSoft.Crema.Presentation.Framework;
using JSSoft.Crema.Presentation.Home.Services.ViewModels;
using JSSoft.Library;
using JSSoft.ModernUI.Framework;
using System;
using System.ComponentModel.Composition;

namespace JSSoft.Crema.Presentation.Home
{
    [Export(typeof(DataBaseServiceViewModel))]
    class DataBaseServiceViewModel : ScreenBase
    {
        private readonly ICremaAppHost cremaAppHost;
        private readonly IAppConfiguration configs;

        private bool isBrowserExpanded = true;
        private bool isPropertyExpanded = true;

        private double browserDistance = 250.0;
        private double propertyDistance = 250.0;

        [ImportingConstructor]
        public DataBaseServiceViewModel(ICremaAppHost cremaAppHost, BrowserService browserService, DataBaseListViewModel contentService, PropertyService propertyService, IAppConfiguration configs)
            : base(cremaAppHost)
        {
            this.cremaAppHost = cremaAppHost;
            this.cremaAppHost.Opened += CremaAppHost_Opened;
            this.cremaAppHost.Closed += CremaAppHost_Closed;
            this.cremaAppHost.Loaded += CremaAppHost_Loaded;
            this.cremaAppHost.Unloaded += CremaAppHost_Unloaded;
            this.BrowserService = browserService;
            this.ContentService = contentService;
            this.PropertyService = propertyService;
            this.configs = configs;
        }

        public override string ToString()
        {
            return "데이터 베이스";
        }

        public BrowserService BrowserService { get; }

        public DataBaseListViewModel ContentService { get; }

        public PropertyService PropertyService { get; }

        [ConfigurationProperty("isBrowserExpanded")]
        public bool IsBrowserExpanded
        {
            get => this.isBrowserExpanded;
            set
            {
                this.isBrowserExpanded = value;
                this.NotifyOfPropertyChange(nameof(this.IsBrowserExpanded));
            }
        }

        [ConfigurationProperty("isPropertyExpanded")]
        public bool IsPropertyExpanded
        {
            get => this.isPropertyExpanded;
            set
            {
                this.isPropertyExpanded = value;
                this.NotifyOfPropertyChange(nameof(this.IsPropertyExpanded));
            }
        }

        [ConfigurationProperty("browserDistance")]
        public double BrowserDistance
        {
            get => this.browserDistance;
            set
            {
                this.browserDistance = value;
                this.NotifyOfPropertyChange(nameof(this.BrowserDistance));
            }
        }

        [ConfigurationProperty("propertyDistance")]
        public double PropertyDistance
        {
            get => this.propertyDistance;
            set
            {
                this.propertyDistance = value;
                this.NotifyOfPropertyChange(nameof(this.PropertyDistance));
            }
        }

        public bool IsVisible => this.cremaAppHost.IsLoaded;

        private void CremaAppHost_Opened(object sender, EventArgs e)
        {
            this.configs.Update(this);
            this.NotifyOfPropertyChange(nameof(this.IsVisible));
        }

        private void CremaAppHost_Closed(object sender, EventArgs e)
        {
            this.configs.Commit(this);
            this.NotifyOfPropertyChange(nameof(this.IsVisible));
        }

        private void CremaAppHost_Loaded(object sender, EventArgs e)
        {
            this.NotifyOfPropertyChange(nameof(this.IsVisible));
        }

        private void CremaAppHost_Unloaded(object sender, EventArgs e)
        {
            this.NotifyOfPropertyChange(nameof(this.IsVisible));
        }
    }
}
