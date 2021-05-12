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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace JSSoft.Crema.Tools.Runtime.ViewModels
{
    public class GenerationItemCollection : ObservableCollection<GenerationItemViewModel>
    {
        private readonly IAppConfiguration configs;
        private IEnumerable<IMenuItem> contextMenus;

        public GenerationItemCollection(IAppConfiguration configs)
        {
            this.configs = configs;

            if (this.configs.TryGetValue<GenerationItemInfo[]>(this.GetType(), typeof(GenerationItemInfo[]), "Items", out GenerationItemInfo[] items) == true)
            {
                foreach (var item in items)
                {
                    var itemViewModel = new GenerationItemViewModel()
                    {
                        Name = item.Name,
                        Address = item.Address,
                        OutputPath = item.OutputPath,
                        DataBase = item.DataBase,
                        LanguageType = item.LanguageType,
                        FilterExpression = item.FilterExpression,
                        Tags = item.Tags,
                        IsDevmode = item.IsDevmode,
                        Options = item.Options,
                    };

                    base.Add(itemViewModel);
                }
            }
            this.CollectionChanged += CodeGenerationItemInfoCollection_CollectionChanged;
        }

        public new void Add(GenerationItemViewModel viewModel)
        {
            this.RemoveEquale(viewModel);
            this.Insert(0, viewModel);
            viewModel.Owner = this;
        }

        public void RemoveEquale(GenerationItemViewModel serverInfo)
        {
            for (var i = this.Count - 1; i >= 0; i--)
            {
                var recentItem = this[i];
                if (recentItem.Equals(serverInfo) == true)
                {
                    this.RemoveAt(i);
                }
            }
        }

        public void Write()
        {
            var serverInfoList = new List<GenerationItemInfo>(this.Count);

            foreach (var item in this)
            {
                var serverInfo = new GenerationItemInfo()
                {
                    Name = item.Name,
                    Address = item.Address,
                    OutputPath = item.OutputPath,
                    DataBase = item.DataBase,
                    LanguageType = item.LanguageType,
                    FilterExpression = item.FilterExpression,
                    Tags = item.Tags,
                    IsDevmode = item.IsDevmode,
                    Options = item.Options,
                };
                serverInfoList.Add(serverInfo);
            }

            //this.configs[this.GetType(), "Items"] = serverInfoList.ToArray();
        }

        public IEnumerable<IMenuItem> ContextMenus
        {

/* 'JSSoft.Crema.Tools.Runtime (net452)' 프로젝트에서 병합되지 않은 변경 내용
이전:
            get { return this.contextMenus; }
이후:
            get => this.contextMenus; }
*/
            get => this.contextMenus;
            set
            {
                this.contextMenus = value;
                foreach (var item in this)
                {
                    item.ContextMenus = this.contextMenus;
                }
            }
        }

        private void CodeGenerationItemInfoCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.Write();
        }
    }
}
