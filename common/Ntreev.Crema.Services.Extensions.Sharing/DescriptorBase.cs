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

using System;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Threading;

namespace Ntreev.Crema.Services.Extensions
{
    public abstract class DescriptorBase : IDescriptorBase, INotifyPropertyChanged
    {
        protected readonly Authentication authentication;
        protected readonly object target;
        protected readonly DescriptorTypes descriptorTypes;
        protected readonly IDescriptorBase referenceTarget;
        protected readonly Dispatcher dispatcher;
        private readonly DescriptorPropertyNotifier notifier;
        
        private bool isDisposed;

        protected DescriptorBase(Authentication authentication, Dispatcher dispatcher, object target, DescriptorTypes descriptorTypes)
        {
            this.authentication = authentication;
            this.target = target;
            this.descriptorTypes = descriptorTypes;
            this.dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            if (this.descriptorTypes.HasFlag(DescriptorTypes.IsSubscriptable) == true)
            {
                this.notifier = new DescriptorPropertyNotifier(this.dispatcher, this, item => this.NotifyOfPropertyChange(item));
                this.dispatcher.InvokeAsync(this.notifier.Save);
            }
        }

        protected DescriptorBase(Authentication authentication, Dispatcher dispatcher, object target, IDescriptorBase referenceTarget, bool isSubscriptable)
        {
            this.authentication = authentication;
            this.referenceTarget = referenceTarget;
            this.target = target;
            this.descriptorTypes = isSubscriptable == true ? DescriptorTypes.IsSubscriptable : DescriptorTypes.None;
            this.dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            this.Initialize();

            if (this.descriptorTypes.HasFlag(DescriptorTypes.IsSubscriptable) == true)
            {
                this.notifier = new DescriptorPropertyNotifier(this.dispatcher, this, item => this.NotifyOfPropertyChange(item));
                this.dispatcher.InvokeAsync(this.notifier.Save);
                this.referenceTarget.PropertyChanged += ReferenceTarget_PropertyChanged;
                this.referenceTarget.Disposed += ReferenceTarget_Disposed;
            }
        }

        public void Dispose()
        {
            if (this.isDisposed == true)
                throw new InvalidOperationException();
            if (this.referenceTarget != null)
            {
                if (this.descriptorTypes.HasFlag(DescriptorTypes.IsSubscriptable) == true)
                {
                    this.referenceTarget.PropertyChanged -= ReferenceTarget_PropertyChanged;
                    this.referenceTarget.Disposed -= ReferenceTarget_Disposed;
                }
            }
            this.notifier?.Dispose();
            this.isDisposed = true;
            this.OnDisposed(EventArgs.Empty);
        }

        public async Task RefreshAsync()
        {
            if (this.notifier != null)
            {
                await this.notifier.RefreshAsync();
            }
        }

        public object Host { get; set; }

        public object Target => this.target;

        public Dispatcher Dispatcher => this.dispatcher;

        public event EventHandler Disposed;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnDisposed(EventArgs e)
        {
            this.Disposed?.Invoke(this, e);
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }

        protected void NotifyOfPropertyChange(string propertyName)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        private void Initialize()
        {
            foreach (var item in this.GetType().GetProperties())
            {
                if (item.CanRead == true && item.GetCustomAttribute<DescriptorPropertyAttribute>() is DescriptorPropertyAttribute propAttr)
                {
                    if (propAttr.FieldName != string.Empty)
                    {
                        var sourceProp = this.referenceTarget.GetType().GetProperty(item.Name);
                        if (sourceProp == null)
                            continue;
                        var sourceValue = sourceProp.GetValue(this.referenceTarget);
                        var fieldInfo = this.GetType().GetField(propAttr.FieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        fieldInfo.SetValue(this, sourceValue);
                    }
                }
            }
        }

        private void ReferenceTarget_Disposed(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void ReferenceTarget_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.notifier.UpdateProperty(this.referenceTarget, e.PropertyName);
        }
    }
}
