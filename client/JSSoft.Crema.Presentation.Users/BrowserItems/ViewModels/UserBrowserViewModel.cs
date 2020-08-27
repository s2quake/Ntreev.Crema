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
using JSSoft.Crema.Presentation.Users.Dialogs.ViewModels;
using JSSoft.Crema.Presentation.Users.Properties;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using JSSoft.Library;
using JSSoft.ModernUI.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace JSSoft.Crema.Presentation.Users.BrowserItems.ViewModels
{
    [Export(typeof(IBrowserItem))]
    [Export(typeof(UserBrowserViewModel))]
    [ParentType("JSSoft.Crema.Presentation.Tables.IBrowserService, JSSoft.Crema.Presentation.Tables, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null")]
    [ParentType("JSSoft.Crema.Presentation.Types.IBrowserService, JSSoft.Crema.Presentation.Types, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null")]
    [ParentType("JSSoft.Crema.Presentation.Home.IBrowserService, JSSoft.Crema.Presentation.Home, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null")]
    class UserBrowserViewModel : TreeViewBase, IBrowserItem
    {
        private readonly Authenticator authenticator;
        private readonly ICremaAppHost cremaAppHost;
        private readonly IShell shell;
        private readonly IEnumerable<IPropertyService> propertyServices;
        private readonly DelegateCommand deleteCommand;

        [ImportingConstructor]
        public UserBrowserViewModel(Authenticator authenticator, ICremaAppHost cremaAppHost, IShell shell, [ImportMany] IEnumerable<IPropertyService> propertyServices)
        {
            this.authenticator = authenticator;
            this.cremaAppHost = cremaAppHost;
            this.cremaAppHost.Opened += CremaAppHost_Opened;
            this.cremaAppHost.Closed += CremaAppHost_Closed;
            this.shell = shell;
            this.propertyServices = propertyServices;
            this.deleteCommand = new DelegateCommand(this.Delete_Execute, this.Delete_CanExecute);
            this.DisplayName = Resources.Title_Users;
            this.Dispatcher.InvokeAsync(() =>
            {
                foreach (var item in this.propertyServices)
                {
                    this.AttachPropertyService(item);
                }
            });
        }

        public async Task NotifyMessageAsync()
        {
            var userContext = this.cremaAppHost.GetService(typeof(IUserContext)) as IUserContext;
            var dialog = await NotifyMessageViewModel.CreateInstanceAsync(this.authenticator, userContext);
            if (dialog != null)
                await dialog.ShowDialogAsync();
        }

        public bool IsVisible => this.cremaAppHost.IsOpened;

        public bool IsAdmin => this.authenticator.Authority == Authority.Admin;

        public ICommand DeleteCommand => this.deleteCommand;

        protected override bool Predicate(IPropertyService propertyService)
        {
            return this.shell.SelectedService.GetType().Assembly == propertyService.GetType().Assembly;
        }

        private void CremaAppHost_Closed(object sender, EventArgs e)
        {
            this.cremaAppHost.UserConfigs.Commit(this);
            this.FilterExpression = string.Empty;
            this.Items.Clear();
        }

        private async void CremaAppHost_Opened(object sender, EventArgs e)
        {
            if (this.cremaAppHost.GetService(typeof(IUserContext)) is IUserContext userContext)
            {
                var viewModel = await userContext.Dispatcher.InvokeAsync(() =>
                {
                    userContext.Users.MessageReceived += Users_MessageReceived;
                    return new UserCategoryTreeViewItemViewModel(this.authenticator, userContext.Root, this);
                });
                this.Items.Add(viewModel);

                this.cremaAppHost.UserConfigs.Update(this);
            }
        }

        private async void Users_MessageReceived(object sender, MessageEventArgs e)
        {
            var message = e.Message;
            var messageType = e.MessageType;
            var sendUserID = e.UserID;
            var userIDs = e.Items.Select(item => item.ID).ToArray();

            if (messageType == MessageType.Notification)
            {
                if (e.UserID != this.authenticator.ID && (userIDs.Any() == false || userIDs.Any(item => item == this.authenticator.ID) == true))
                {
                    await this.Dispatcher.InvokeAsync(() =>
                    {
                    });
                    var title = string.Format(Resources.Title_AdministratorMessage_Format, sendUserID);
                    if (this.cremaAppHost.GetService(typeof(IFlashService)) is IFlashService flashService)
                        flashService.Flash();
                    await AppMessageBox.ShowAsync(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else if (messageType == MessageType.None)
            {
                if (e.UserID != this.authenticator.ID)
                {
                    var userContext = this.cremaAppHost.GetService(typeof(IUserContext)) as IUserContext;
                    var dialog = await ViewMessageViewModel.CreateInstanceAsync(this.authenticator, userContext, message, sendUserID);
                    if (dialog != null)
                        await dialog.ShowDialogAsync();
                }
            }
        }

        private void Delete_Execute(object parameter)
        {
            if (parameter is UserTreeViewItemViewModel userViewModel)
            {
                userViewModel.DeleteCommand.Execute(parameter);
            }
            else if (parameter is UserCategoryTreeViewItemViewModel categoryViewModel)
            {
                categoryViewModel.DeleteCommand.Execute(parameter);
            }
        }

        private bool Delete_CanExecute(object parameter)
        {
            if (parameter is UserTreeViewItemViewModel userViewModel)
            {
                return userViewModel.DeleteCommand.CanExecute(parameter);
            }
            else if (parameter is UserCategoryTreeViewItemViewModel categoryViewModel)
            {
                return categoryViewModel.DeleteCommand.CanExecute(parameter);
            }
            return false;
        }

        [ConfigurationProperty(ScopeType = typeof(IUserConfiguration))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:사용되지 않는 private 멤버 제거", Justification = "<보류 중>")]
        private string[] Settings
        {
            get => this.GetSettings();
            set => this.SetSettings(value);
        }
    }
}
