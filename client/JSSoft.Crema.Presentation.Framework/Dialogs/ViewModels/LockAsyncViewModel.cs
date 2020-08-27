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

using JSSoft.Crema.Presentation.Framework.Properties;
using JSSoft.ModernUI.Framework;
using System;
using System.Threading.Tasks;

namespace JSSoft.Crema.Presentation.Framework.Dialogs.ViewModels
{
    public class LockAsyncViewModel : LockViewModel
    {
        private bool isValid;

        public LockAsyncViewModel()
        {
            this.PropertyChanged += LockAsyncViewModel_PropertyChanged;
        }

        public async Task LockAsync()
        {
            try
            {
                this.BeginProgress(Resources.Message_Locking);
                await this.LockAsync(this.Comment);
                this.EndProgress();
                await this.TryCloseAsync(true);
            }
            catch (Exception e)
            {
                this.EndProgress();
                await AppMessageBox.ShowErrorAsync(e);
            }
        }

        protected sealed override bool VerifyLock(string comment)
        {
            if (base.VerifyLock(comment) == false)
                return false;
            return this.isValid;
        }

        protected virtual void VerifyLock(string comment, Action<bool> isValid)
        {
            isValid(true);
        }

        protected virtual Task LockAsync(string comment)
        {
            return Task.Delay(1);
        }

        private void VerifyAction(bool isValid)
        {
            this.isValid = isValid;
            this.NotifyOfPropertyChange(nameof(this.CanLock));
        }

        private void LockAsyncViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.Comment))
            {
                this.VerifyLock(this.Comment, this.VerifyAction);
            }
        }
    }
}
