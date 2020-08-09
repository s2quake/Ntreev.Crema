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
using Ntreev.Crema.Presentation.Users.Properties;
using Ntreev.Crema.Services;
using Ntreev.ModernUI.Framework.Dialogs.ViewModels;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ntreev.Crema.Presentation.Users.Dialogs.ViewModels
{
    public class MoveUserCategoryViewModel : MoveAsyncViewModel
    {
        private readonly Authentication authentication;
        private readonly IUserCategory category;

        private MoveUserCategoryViewModel(Authentication authentication, IUserCategory category, string[] targetPaths)
            : base(category.Path, targetPaths)
        {
            this.authentication = authentication;
            this.authentication.Expired += Authentication_Expired;
            this.category = category;
            this.category.Dispatcher.VerifyAccess();
            this.DisplayName = Resources.Title_MoveUserFolder;
        }

        public static Task<MoveUserCategoryViewModel> CreateInstanceAsync(Authentication authentication, IUserCategoryDescriptor descriptor)
        {
            if (authentication == null)
                throw new ArgumentNullException(nameof(authentication));
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            if (descriptor.Target is IUserCategory category)
            {
                return category.Dispatcher.InvokeAsync(() =>
                {
                    var categories = category.GetService(typeof(IUserCategoryCollection)) as IUserCategoryCollection;
                    var targetPaths = categories.Select(item => item.Path).ToArray();
                    return new MoveUserCategoryViewModel(authentication, category, targetPaths);
                });
            }
            else
            {
                throw new ArgumentException("Invalid Target of Descriptor", nameof(descriptor));
            }
        }

        protected async override void VerifyMove(string targetPath, Action<bool> isVerify)
        {
            var result = await this.category.Dispatcher.InvokeAsync(() =>
            {
                var categories = this.category.GetService(typeof(IUserCategoryCollection)) as IUserCategoryCollection;
                var targetCategory = categories[targetPath];
                if (targetCategory == null)
                    return false;
                if (targetCategory.Categories.ContainsKey(this.category.Name) == true)
                    return false;
                return targetCategory.Users.ContainsKey(this.category.Name) == false;
            });
            isVerify(result);
        }

        protected override Task OnMoveAsync(string targetPath)
        {
            return this.category.MoveAsync(this.authentication, targetPath);
        }

        protected async override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            await base.OnDeactivateAsync(close, cancellationToken);
            if (close == true)
            {
                this.authentication.Expired -= Authentication_Expired;
            }
        }

        private async void Authentication_Expired(object sender, EventArgs e)
        {
            await this.TryCloseAsync();
        }
    }
}
