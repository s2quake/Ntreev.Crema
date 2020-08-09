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

using Ntreev.Crema.Presentation.Differences.Documents.ViewModels;
using Ntreev.Crema.Presentation.Differences.Properties;
using Ntreev.Crema.Presentation.Framework;
using Ntreev.Library;
using Ntreev.ModernUI.Framework;
using System;
using System.ComponentModel.Composition;

namespace Ntreev.Crema.Presentation.Differences
{
    [Export(typeof(IContentService))]
    [InheritedExport(typeof(DifferencesServiceViewModel))]
    [Order(50)]
    class DifferencesServiceViewModel : ScreenBase, IContentService
    {
        private readonly ICremaAppHost cremaAppHost;
        private readonly IAppConfiguration configs;

        private bool isBrowserExpanded = true;
        private bool isPropertyExpanded = true;

        private double browserDistance = 250.0;
        private double propertyDistance = 250.0;

        private bool isVisible;

        [ImportingConstructor]
        public DifferencesServiceViewModel(ICremaAppHost cremaAppHost, IBrowserService browserService, DocumentServiceViewModel documentService, IPropertyService propertyService, IAppConfiguration configs)
        {
            this.cremaAppHost = cremaAppHost;
            this.cremaAppHost.Opened += CremaAppHost_Opened;
            this.cremaAppHost.Closed += CremaAppHost_Closed;
            this.cremaAppHost.Loaded += CremaAppHost_Loaded;
            this.cremaAppHost.Unloaded += CremaAppHost_Unloaded;
            this.BrowserService = browserService;
            this.DocumentService = documentService;
            this.PropertyService = propertyService;
            this.configs = configs;
            this.DisplayName = Resources.Title_Differences;
        }

        public override string ToString()
        {
            return Resources.Title_Differences;
        }

        public async void Dispose()
        {
            await this.DocumentService.TryCloseAsync();
        }

        public IBrowserService BrowserService { get; }

        public DocumentServiceViewModel DocumentService { get; }

        public IPropertyService PropertyService { get; }

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

        public bool IsVisible
        {
            get => this.isVisible;
            set
            {
                this.isVisible = value;
                this.NotifyOfPropertyChange(nameof(this.IsVisible));
            }
        }

        private void CremaAppHost_Opened(object sender, EventArgs e)
        {
            this.configs.Update(this);
            this.Refresh();
        }

        private void CremaAppHost_Closed(object sender, EventArgs e)
        {
            this.configs.Commit(this);
        }

        private void CremaAppHost_Loaded(object sender, EventArgs e)
        {
            this.IsVisible = true;
        }

        private void CremaAppHost_Unloaded(object sender, EventArgs e)
        {
            this.IsVisible = false;
        }
    }
}
