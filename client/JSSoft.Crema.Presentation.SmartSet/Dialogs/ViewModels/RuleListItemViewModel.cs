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
using System.Linq;

namespace JSSoft.Crema.Presentation.SmartSet.Dialogs.ViewModels
{
    class RuleListItemViewModel : PropertyChangedBase
    {
        private readonly ObservableCollection<RuleListItemViewModel> itemsSource;
        private readonly Dictionary<IRule, IRuleItem> ruleToItem = new();
        private IRule rule;

        public RuleListItemViewModel(ObservableCollection<RuleListItemViewModel> ruleItems, IEnumerable<IRule> rules, IRuleItem ruleItem)
        {
            this.itemsSource = ruleItems;
            this.itemsSource.CollectionChanged += ItemsSource_CollectionChanged;
            this.Rules = rules;
            if (ruleItem != null)
            {
                this.rule = rules.First(item => item.Name == ruleItem.RuleName);
                this.RuleItem = ruleItem;
            }
        }

        private void ItemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.NotifyOfPropertyChange(nameof(this.CanDelete));
        }

        public string Name => this.RuleItem.GetType().Name;

        public string Text
        {
            get
            {
                if (this.rule == null)
                    return "unknown";
                return this.rule.DisplayName;
            }
        }

        public IRule Rule
        {
            get => this.rule;
            set
            {
                this.rule = value;

                if (this.ruleToItem.ContainsKey(this.rule) == false)
                {
                    this.ruleToItem.Add(this.rule, this.rule.CreateItem());
                }
                this.RuleItem = this.ruleToItem[this.rule];
                this.NotifyOfPropertyChange(nameof(this.Rule));
                this.NotifyOfPropertyChange(nameof(this.RuleItem));
            }
        }

        public IRuleItem RuleItem { get; private set; }

        public bool CanDelete => this.itemsSource.Count != 1;

        public void Delete()
        {
            this.itemsSource.Remove(this);
        }

        public void Insert()
        {
            var index = this.itemsSource.IndexOf(this) + 1;
            this.itemsSource.Insert(index, new RuleListItemViewModel(this.itemsSource, this.Rules, null));
        }

        public IEnumerable<IRule> Rules { get; }
    }
}
