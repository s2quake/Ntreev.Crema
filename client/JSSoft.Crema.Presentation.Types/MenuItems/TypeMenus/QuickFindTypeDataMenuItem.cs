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
using JSSoft.Crema.Presentation.Types.Properties;
using JSSoft.ModernUI.Framework;
using JSSoft.ModernUI.Framework.Controls;
using System;
using System.ComponentModel.Composition;
using System.Windows.Input;

namespace JSSoft.Crema.Presentation.Types.MenuItems.TypeMenus
{
    [Export(typeof(IMenuItem))]
    [Export(typeof(QuickFindTypeDataMenuItem))]
    [ParentType(typeof(TypeMenuItem))]
    class QuickFindTypeDataMenuItem : MenuItemBase
    {
        private readonly IShell shell;
        private readonly ITypeDocumentService documentService;
        private readonly Lazy<TypeServiceViewModel> typeService;

        [ImportingConstructor]
        public QuickFindTypeDataMenuItem(IShell shell, ITypeDocumentService documentService, Lazy<TypeServiceViewModel> typeService)
        {
            this.shell = shell;
            this.shell.ServiceChanged += this.InvokeCanExecuteChangedEvent;
            this.documentService = documentService;
            this.documentService.SelectionChanged += this.InvokeCanExecuteChangedEvent;
            this.typeService = typeService;
            this.InputGesture = new KeyGesture(Key.F, ModifierKeys.Control);
            this.DisplayName = Resources.MenuItem_QuickFind;
        }

        protected override bool OnCanExecute(object parameter)
        {
            if (this.shell.SelectedService != this.typeService.Value)
                return false;
            if (SearchBox.ShowCommand is ICommand command)
                return command.CanExecute(parameter);
            return false;
        }

        protected override void OnExecute(object parameter)
        {
            if (SearchBox.ShowCommand is ICommand command)
            {
                command.Execute(parameter);
            }
        }
    }
}
