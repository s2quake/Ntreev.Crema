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

using Ntreev.Crema.Presentation.Framework;
using Ntreev.Crema.Presentation.Types.Dialogs.ViewModels;
using Ntreev.Crema.Presentation.Types.Documents.ViewModels;
using Ntreev.Crema.Presentation.Types.Properties;
using Ntreev.Crema.Services;
using Ntreev.Crema.Services.Extensions;
using Ntreev.Library;
using Ntreev.ModernUI.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace Ntreev.Crema.Presentation.Types
{
    [Export(typeof(IContentService))]
    [InheritedExport(typeof(TypeServiceViewModel))]
    class TypeServiceViewModel : ScreenBase, IContentService
    {
        private readonly Authenticator authenticator;
        private readonly ICremaAppHost cremaAppHost;
        private readonly IAppConfiguration configs;
        private readonly Lazy<IShell> shell = null;

        private bool isBrowserExpanded = true;
        private bool isPropertyExpanded = true;

        private double browserDistance = 250.0;
        private double propertyDistance = 250.0;

        private bool isFirst;
        private bool isVisible;


        [ImportingConstructor]
        public TypeServiceViewModel(Authenticator authenticator, ICremaAppHost cremaAppHost, IBrowserService browserService,
            TypeDocumentServiceViewModel documentService, IPropertyService propertyService, IAppConfiguration configs, Lazy<IShell> shell)
        {
            this.authenticator = authenticator;
            this.cremaAppHost = cremaAppHost;
            this.cremaAppHost.Opened += CremaAppHost_Opened;
            this.cremaAppHost.Closed += CremaAppHost_Closed;
            this.cremaAppHost.Loaded += CremaAppHost_Loaded;
            this.cremaAppHost.Unloaded += CremaAppHost_Unloaded;
            this.BrowserService = browserService;
            this.DocumentService = documentService;
            this.PropertyService = propertyService;
            this.configs = configs;
            this.shell = shell;
            this.DisplayName = Resources.Title_Type;
        }

        public override string ToString()
        {
            return Resources.Title_Type;
        }

        public async void Dispose()
        {
            await this.DocumentService.TryCloseAsync();
        }

        public IBrowserService BrowserService { get; } = null;

        public TypeDocumentServiceViewModel DocumentService { get; } = null;

        public IPropertyService PropertyService { get; } = null;

        [ConfigurationProperty("isBrowserExpanded")]
        [DefaultValue(true)]
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
        [DefaultValue(true)]
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
        [DefaultValue(250.0)]
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
        [DefaultValue(250.0)]
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

        private async void CremaHost_Closed(object sender, EventArgs e)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.shell.Value.ServiceChanged -= Shell_ServiceChanged;
            });
        }

        private void CremaAppHost_Opened(object sender, EventArgs e)
        {
            this.shell.Value.ServiceChanged += Shell_ServiceChanged;
            this.configs.Update(this);
            this.Refresh();
        }

        private void CremaAppHost_Closed(object sender, EventArgs e)
        {
            this.configs.Commit(this);
            this.Shell.ServiceChanged -= Shell_ServiceChanged;
        }


        private void CremaAppHost_Loaded(object sender, EventArgs e)
        {
            this.isFirst = false;
            this.IsVisible = true;
        }

        private void CremaAppHost_Unloaded(object sender, EventArgs e)
        {
            this.IsVisible = false;
        }

        private async void Shell_ServiceChanged(object sender, EventArgs e)
        {
            if (this.Shell.SelectedService == this)
            {
                if (this.cremaAppHost.GetService(typeof(IDataBase)) is IDataBase dataBase)
                {
                    await this.RestoreAsync(dataBase);
                }
            }
        }

        private async Task RestoreAsync(IDataBase dataBase)
        {
            if (this.isFirst == true)
                return;

            this.isFirst = true;

            var domainContext = dataBase.GetService(typeof(IDomainContext)) as IDomainContext;
            var items = await await domainContext.Dispatcher.InvokeAsync(async () =>
            {
                var domains = domainContext.Domains.Where(item => item.DataBaseID == dataBase.ID).ToArray();
                var restoreList = new List<System.Action>();

                foreach (var item in domains)
                {
                    if (await item.Users.ContainsAsync(this.authenticator.ID) == false)
                        continue;

                    var itemPath = item.DomainInfo.ItemPath;
                    var itemType = item.DomainInfo.ItemType;

                    if (item.Host is ITypeTemplate template)
                    {
                        if (itemType == "NewTypeTemplate")
                        {
                            var category = dataBase.TypeContext[itemPath] as ITypeCategory;
                            var dialog = await category.Dispatcher.InvokeAsync(() => new NewTypeViewModel(this.authenticator, category, template));
                            restoreList.Add(new System.Action(() => dialog.ShowDialogAsync()));
                        }
                        else if (itemType == "TypeTemplate")
                        {
                            var type = dataBase.TypeContext[itemPath] as IType;
                            var dialog = await type.Dispatcher.InvokeAsync(() => new EditTemplateViewModel(this.authenticator, type, template));
                            restoreList.Add(new System.Action(() => dialog.ShowDialogAsync()));
                        }
                    }
                }
                return restoreList.ToArray();
            });

            foreach (var item in items)
            {
                if (this.cremaAppHost.IsLoaded == false)
                    return;

                item();
            }
        }

        private IShell Shell => this.shell.Value;
    }
}
