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
using System;
using System.Collections.Generic;

namespace JSSoft.Crema.Presentation.SmartSet.Rules.ViewModels
{
    public abstract class OperatorRule<T> : PropertyChangedBase, IRule where T : IComparable
    {
        public virtual IEnumerable<OperatorType> Operators
        {
            get
            {
                foreach (var item in Enum.GetValues(typeof(OperatorType)))
                {
                    yield return (OperatorType)item;
                }
            }
        }

        protected abstract T GetObjectValue(object obj);

        public bool Verify(object obj, IRuleItem item)
        {
            if (item is OperatorRuleItemViewModel<T> == false)
                return false;

            var objValue = this.GetObjectValue(obj);
            var operatorType = (item as OperatorRuleItemViewModel<T>).OperatorType;
            var itemValue = (item as OperatorRuleItemViewModel<T>).Value;

            if (operatorType == OperatorType.Equal)
                return objValue.CompareTo(itemValue) == 0;
            else if (operatorType == OperatorType.GreaterThan)
                return objValue.CompareTo(itemValue) > 0;
            else if (operatorType == OperatorType.GreaterThanEqual)
                return objValue.CompareTo(itemValue) >= 0;
            else if (operatorType == OperatorType.LessThan)
                return objValue.CompareTo(itemValue) < 0;
            else if (operatorType == OperatorType.LessThan)
                return objValue.CompareTo(itemValue) <= 0;
            else if (operatorType == OperatorType.NotEqual)
                return objValue.CompareTo(itemValue) != 0;

            throw new NotImplementedException();
        }

        public abstract Type SupportType
        {
            get;
        }

        public T DefaultValue
        {
            get;
            set;
        }

        public Type ValueType => typeof(T);

        public string DisplayName
        {
            get;
            set;
        }

        public IRuleItem CreateItem()
        {
            var item = this.OnCreateItem();
            item.RuleName = this.Name;
            item.Value = this.DefaultValue;
            return item;
        }

        public string Name => this.GetType().Name;

        protected virtual OperatorRuleItemViewModel<T> OnCreateItem()
        {
            return new OperatorRuleItemViewModel<T>();
        }
    }
}