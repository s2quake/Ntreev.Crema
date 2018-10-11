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
using System.Threading.Tasks;
using Ntreev.Library.ObjectModel;
using Ntreev.Library.Linq;

namespace Ntreev.Crema.Bot.Tasks
{
    [Export(typeof(ITaskProvider))]
    [Export(typeof(ITableCategoryTask))]
    [TaskClass]
    public class ITableCategoryTask : ITaskProvider
    {
        public async Task InvokeAsync(TaskContext context)
        {
            var authentication = context.Authentication;
            var category = context.Target as ITableCategory;
            var tables = category.GetService(typeof(ITableCollection)) as ITableCollection;
            var categories = category.GetService(typeof(ITableCategoryCollection)) as ITableCategoryCollection;
            if (context.IsCompleted(category) == true)
            {
                context.Pop(category);
            }
            else if (categories.Count < RandomUtility.Next(Math.Max(10, categories.Count + 1)))
            {
                await this.AddNewCategoryAsync(category, context);
                context.Complete(category);
            }
            else if (tables.Count < RandomUtility.Next(Math.Max(10, tables.Count + 1)))
            {
                var template = await category.NewTableAsync(authentication);
                context.Push(template);
            }
            else
            {
                context.Complete(category);
            }
        }

        public Type TargetType
        {
            get { return typeof(ITableCategory); }
        }

        public bool IsEnabled
        {
            get { return false; }
        }

        [TaskMethod]
        public async Task GetAccessTypeAsync(ITableCategory category, TaskContext context)
        {
            var authentication = context.Authentication;
            await category.Dispatcher.InvokeAsync(() => category.GetAccessType(authentication));
        }

        //[TaskMethod(Weight = 10)]
        public async Task LockAsync(ITableCategory category, TaskContext context)
        {
            var authentication = context.Authentication;
            var comment = RandomUtility.NextString();
            if (context.AllowException == false)
            {
                if (category.Parent == null)
                    return;
                if (comment == string.Empty)
                    return;
                if (category.IsLocked == true)
                    return;
            }
            await category.LockAsync(authentication, comment);
        }

        //[TaskMethod]
        public async Task UnlockAsync(ITableCategory category, TaskContext context)
        {
            var authentication = context.Authentication;
            if (context.AllowException == false)
            {
                if (category.Parent == null)
                    return;
                if (await category.Dispatcher.InvokeAsync(()=> category.IsLocked) == false)
                    return;
            }
            await category.UnlockAsync(authentication);
        }

        //[TaskMethod]
        public async Task SetPublicAsync(ITableCategory category, TaskContext context)
        {
            var authentication = context.Authentication;
            if (context.AllowException == false)
            {
                if (category.Parent == null)
                    return;
                if (category.IsPrivate == false)
                    return;
            }
            await category.SetPublicAsync(authentication);
        }

        //[TaskMethod(Weight = 10)]
        public async Task SetPrivateAsync(ITableCategory category, TaskContext context)
        {
            var authentication = context.Authentication;
            if (category.Parent == null)
                return;
            await category.SetPrivateAsync(authentication);
        }

        //[TaskMethod(Weight = 10)]
        public async Task AddAccessMemberAsync(ITableCategory category, TaskContext context)
        {
            var authentication = context.Authentication;
            if (category.Parent == null)
                return;
            var userContext = category.GetService(typeof(IUserContext)) as IUserContext;
            var memberID = await userContext.Dispatcher.InvokeAsync(() => userContext.Select(item => item.Path).Random());
            var accessType = RandomUtility.NextEnum<AccessType>();
            if (NameValidator.VerifyItemPath(memberID) == true)
            {
                await category.AddAccessMemberAsync(authentication, new ItemName(memberID).Name, accessType);
            }
            else
            {
                await category.AddAccessMemberAsync(authentication, memberID, accessType);
            }
        }

        //[TaskMethod(Weight = 10)]
        public async Task RemoveAccessMemberAsync(ITableCategory category, TaskContext context)
        {
            var authentication = context.Authentication;
            if (category.Parent == null)
                return;
            var userContext = category.GetService(typeof(IUserContext)) as IUserContext;
            var memberID = await userContext.Dispatcher.InvokeAsync(() => userContext.Select(item => item.Path).Random());
            if (NameValidator.VerifyItemPath(memberID) == true)
            {
                await category.RemoveAccessMemberAsync(authentication, new ItemName(memberID).Name);
            }
            else
            {
                await category.RemoveAccessMemberAsync(authentication, memberID);
            }
        }



        [TaskMethod]
        public async Task RenameAsync(ITableCategory category, TaskContext context)
        {
            var authentication = context.Authentication;
            var categoryName = RandomUtility.NextIdentifier();
            if (context.AllowException == false)
            {
                if (category.Parent == null)
                    return;
                if ((await this.GetTablesAsync(category, item => item.TableState != TableState.None)).Any() == true)
                    return;
            }
            await category.RenameAsync(authentication, categoryName);
        }

        [TaskMethod]
        public async Task MoveAsync(ITableCategory category, TaskContext context)
        {
            var authentication = context.Authentication;
            var categories = category.GetService(typeof(ITableCategoryCollection)) as ITableCategoryCollection;
            var categoryPath = await categories.Dispatcher.InvokeAsync(() => categories.Random().Path);
            if (context.AllowException == false)
            {
                if (category.Parent == null)
                    return;
                if (categoryPath.StartsWith(category.Path) == true)
                    return;
                if (category.Parent.Path == categoryPath)
                    return;
                if (categoryPath.StartsWith(category.Path) == true)
                    return;
                if ((await this.GetTablesAsync(category, item => item.TableState != TableState.None)).Any() == true)
                    return;
            }
            await category.MoveAsync(authentication, categoryPath);
        }

        //[TaskMethod(Weight = 1)]
        public async Task DeleteAsync(ITableCategory category, TaskContext context)
        {
            var authentication = context.Authentication;
            if (context.AllowException == false)
            {
                if (category.Parent == null)
                    return;
                if ((await this.GetTablesAsync(category, item => true)).Any() == true)
                    return;
            }
            await category.DeleteAsync(authentication);
            context.Pop(category);
        }

        [TaskMethod(Weight = 10)]
        public async Task AddNewCategoryAsync(ITableCategory category, TaskContext context)
        {
            var authentication = context.Authentication;
            var categoryNanme = RandomUtility.NextIdentifier();
            if (context.AllowException == false)
            {
                if (category.GetAccessType(authentication) < AccessType.Master)
                    return;
            }
            await category.AddNewCategoryAsync(authentication, categoryNanme);
        }

        [TaskMethod(Weight = 10)]
        public async Task NewTableAsync(ITableCategory category, TaskContext context)
        {
            var authentication = context.Authentication;
            var template = await category.NewTableAsync(authentication);
            context.Push(template);
        }

        //[TaskMethod]
        public async Task GetDataSetAsync(ITableCategory category, TaskContext context)
        {
            var authentication = context.Authentication;
            await category.GetDataSetAsync(authentication, null);
        }

        //[TaskMethod]
        public async Task GetLogAsync(ITableCategory category, TaskContext context)
        {
            var authentication = context.Authentication;
            await category.GetLogAsync(authentication, null);
        }

        //[TaskMethod]
        public async Task FindAsync(ITableCategory category, TaskContext context)
        {
            var authentication = context.Authentication;
            var text = RandomUtility.NextWord();
            var option = RandomUtility.NextEnum<FindOptions>();
            await category.FindAsync(authentication, text, option);
        }

        public Task<ITable[]> GetTablesAsync(ITableCategory category, Func<ITable, bool> predicate)
        {
            return category.Dispatcher.InvokeAsync(() =>
            {
                var query = from item in EnumerableUtility.FamilyTree<ITableItem, ITable>(category as ITableItem, item => item.Childs)
                            where predicate(item)
                            select item;

                return query.ToArray();
            });
        }
    }
}
