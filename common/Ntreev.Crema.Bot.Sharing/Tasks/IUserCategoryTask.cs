﻿//Released under the MIT License.
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

using Ntreev.Crema.Data;
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services;
using Ntreev.Library;
using Ntreev.Library.Random;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;
using System.Security;
using System.Threading.Tasks;
using Ntreev.Library.Linq;

namespace Ntreev.Crema.Bot.Tasks
{
    [Export(typeof(ITaskProvider))]
    [Export(typeof(IUserCategoryTask))]
    [TaskClass(Weight = 10)]
    public class IUserCategoryTask : ITaskProvider
    {
        public IUserCategoryTask()
        {

        }

        public async Task InvokeAsync(TaskContext context)
        {
            var category = context.Target as IUserCategory;
            if (context.IsCompleted(category) == true)
            {
                context.Pop(category);
            }
            await Task.Delay(0);
        }

        public Type TargetType
        {
            get { return typeof(IUserCategory); }
        }

        public bool IsEnabled
        {
            get { return false; }
        }

        [TaskMethod]
        public async Task RenameAsync(IUserCategory category, TaskContext context)
        {
            if (category.Parent == null)
                return;
            var categoryName = RandomUtility.NextIdentifier();
            await category.RenameAsync(context.Authentication, categoryName);
        }

        [TaskMethod]
        public async Task MoveAsync(IUserCategory category, TaskContext context)
        {
            var categories = category.GetService(typeof(IUserCategoryCollection)) as IUserCategoryCollection;
            var categoryPath = await categories.Dispatcher.InvokeAsync(() => categories.Random().Path);

            if (context.AllowException == false)
            {
                if (category.Parent == null)
                    return;
                if (categoryPath.StartsWith(category.Path) == true)
                    return;
                if (category.Path == categoryPath)
                    return;
                if (category.Parent.Path == categoryPath)
                    return;
            }
            
            await category.MoveAsync(context.Authentication, categoryPath);
        }

        [TaskMethod]
        public async Task DeleteAsync(IUserCategory category, TaskContext context)
        {
            if (category.Parent == null)
                return;
            if (context.AllowException == false)
            {
                if (category.Parent == null)
                    return;
                if (EnumerableUtility.Descendants<IUserItem, IUser>(category as IUserItem, item => item.Childs).Any() == true)
                    return;
            }
            await category.DeleteAsync(context.Authentication);
            context.Complete(category);
        }

        [TaskMethod(Weight = 10)]
        public async Task AddNewCategoryAsync(IUserCategory category, TaskContext context)
        {
            var categoryName = RandomUtility.NextIdentifier();
            await category.AddNewCategoryAsync(context.Authentication, categoryName);
        }

        [TaskMethod]
        public async Task AddNewUserAsync(IUserCategory category, TaskContext context)
        {
            var index = RandomUtility.Next(int.MaxValue);
            var authority = RandomUtility.NextEnum<Authority>();
            var userID = $"{authority.ToString().ToLower()}_bot_{index}";
            var userName = "봇" + index;
            await category.AddNewUserAsync(context.Authentication, userID, ToSecureString("1111"), userName, authority);
        }

        private static SecureString ToSecureString(string value)
        {
            var secureString = new SecureString();
            foreach (var item in value)
            {
                secureString.AppendChar(item);
            }
            return secureString;
        }
    }
}
