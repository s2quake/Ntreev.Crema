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

using JSSoft.Library;
using JSSoft.ModernUI.Framework;
using System;

namespace JSSoft.Crema.Presentation.SmartSet.Rules.ViewModels
{
    public abstract class StringCompareRuleViewModel : PropertyChangedBase, IRule
    {
        public virtual bool Verify(object target, IRuleItem ruleItem)
        {
            if (ruleItem is StringCompareRuleItemViewModel == false)
                return false;

            var viewModel = ruleItem as StringCompareRuleItemViewModel;

            var sourceValue = this.GetSourceValue(target);
            var caseSenstive = (ruleItem as StringCompareRuleItemViewModel).CaseSensitive;
            var globPattern = (ruleItem as StringCompareRuleItemViewModel).GlobPattern;
            var targetValue = (ruleItem as StringCompareRuleItemViewModel).Value;

            if (globPattern == true)
                return sourceValue.Glob(targetValue, caseSenstive);

            if (caseSenstive == true)
                return sourceValue.IndexOf(targetValue, StringComparison.CurrentCulture) >= 0;

            return sourceValue.IndexOf(targetValue, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        public abstract Type SupportType
        {
            get;
        }

        protected abstract string GetSourceValue(object target);

        public string DisplayName
        {
            get;
            set;
        }

        public IRuleItem CreateItem()
        {
            return new StringCompareRuleItemViewModel() { RuleName = this.Name, };
        }


        public string Name => this.GetType().Name;
    }
}