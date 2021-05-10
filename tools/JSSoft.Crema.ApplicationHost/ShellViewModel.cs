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

using JSSoft.Crema.ApplicationHost.ViewModels;
using JSSoft.Crema.Tools.Framework;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;

namespace JSSoft.Crema.ApplicationHost
{
    [Export(typeof(IShell))]
    [Export(typeof(IContentService))]
    [InheritedExport(typeof(ShellViewModel))]
    class ShellViewModel : Caliburn.Micro.PropertyChangedBase, IShell, IContentService
    {
        private object selectedContent;

        [ImportingConstructor]
        public ShellViewModel([ImportMany]IEnumerable<IContent> contents)
        {
            this.Contents = new ObservableCollection<object>
            {
                new ConsoleViewModel()
            };
            foreach (var item in contents)
            {
                this.Contents.Add(item);
            }

            this.selectedContent = this.Contents.First();
        }

        public object SelectedContent
        {
            get => this.selectedContent;
            set
            {
                if (this.selectedContent == value)
                    return;
                this.selectedContent = this.Contents.SingleOrDefault(item => item == value);
                this.NotifyOfPropertyChange(() => this.SelectedContent);
            }
        }

        public ObservableCollection<object> Contents { get; private set; }
    }
}
