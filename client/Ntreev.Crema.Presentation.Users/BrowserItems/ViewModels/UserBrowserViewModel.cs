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
using Ntreev.Crema.Presentation.Users.Dialogs.ViewModels;
using Ntreev.Crema.Presentation.Users.Properties;
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services;
using Ntreev.Library;
using Ntreev.ModernUI.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Ntreev.Crema.Presentation.Users.BrowserItems.ViewModels
{
    [Export(typeof(IBrowserItem))]
    [Export(typeof(UserBrowserViewModel))]
    [ParentType("Ntreev.Crema.Presentation.Tables.IBrowserService, Ntreev.Crema.Presentation.Tables, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null")]
    [ParentType("Ntreev.Crema.Presentation.Types.IBrowserService, Ntreev.Crema.Presentation.Types, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null")]
    [ParentType("Ntreev.Crema.Presentation.Home.IBrowserService, Ntreev.Crema.Presentation.Home, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null")]
    class UserBrowserViewModel : TreeViewBase, IBrowserItem
    {
        private readonly ICremaAppHost cremaAppHost;

        [Import]
        private readonly Authenticator authenticator = null;
        [ImportMany]
        private readonly IEnumerable<IPropertyService> propertyServices = null;
        [Import]
        private readonly Lazy<IShell> shell = null;

        private readonly DelegateCommand deleteCommand;

        [Import]
        private readonly IFlashService flashService = null;
        [Import]
        private readonly IBuildUp buildUp = null;

        [ImportingConstructor]
        public UserBrowserViewModel(ICremaAppHost cremaAppHost)
        {
            this.cremaAppHost = cremaAppHost;
            this.cremaAppHost.Opened += CremaAppHost_Opened;
            this.cremaAppHost.Closed += CremaAppHost_Closed;
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
            return this.Shell.SelectedService.GetType().Assembly == propertyService.GetType().Assembly;
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

                this.buildUp.BuildUp(viewModel);
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
                    this.flashService?.Flash();
                    await AppMessageBox.ShowAsync(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else if (messageType == MessageType.None)
            {
                if (e.UserID != this.authenticator.ID)
                {
#pragma warning disable CS1998 // 이 비동기 메서드에는 'await' 연산자가 없으며 메서드가 동시에 실행됩니다. 'await' 연산자를 사용하여 비블로킹 API 호출을 대기하거나, 'await Task.Run(...)'을 사용하여 백그라운드 스레드에서 CPU 바인딩된 작업을 수행하세요.
                    await this.Dispatcher.InvokeAsync(async () =>
#pragma warning restore CS1998 // 이 비동기 메서드에는 'await' 연산자가 없으며 메서드가 동시에 실행됩니다. 'await' 연산자를 사용하여 비블로킹 API 호출을 대기하거나, 'await Task.Run(...)'을 사용하여 백그라운드 스레드에서 CPU 바인딩된 작업을 수행하세요.
                    {
                    });
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

        private IShell Shell => this.shell.Value;
    }
}
