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

using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using JSSoft.Library;
using JSSoft.Library.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSSoft.Crema.ServerService.Test
{
    static class Extensions
    {
        public static async Task AddMemberAsync(this ITypeTemplate template, Authentication authentication, string name, long value, string comment)
        {
            var member = await template.AddNewAsync(authentication);
            await member.SetNameAsync(authentication, name);
            await member.SetValueAsync(authentication, value);
            await member.SetCommentAsync(authentication, comment);
            await template.EndNewAsync(authentication, member);
        }

        public static async Task AddKeyAsync(this ITableTemplate template, Authentication authentication, string name, string typeName)
        {
            var column = await template.AddNewAsync(authentication);
            await column.SetNameAsync(authentication, name);
            await column.SetIsKeyAsync(authentication, true);
            await column.SetDataTypeAsync(authentication, typeName);
            await column.SetCommentAsync(authentication, string.Format("Key : {0}", typeName));
            await template.EndNewAsync(authentication, column);
        }

        public static async Task AddColumnAsync(this ITableTemplate template, Authentication authentication, string name, string typeName)
        {
            var column = await template.AddNewAsync(authentication);
            await column.SetNameAsync(authentication, name);
            await column.SetDataTypeAsync(authentication, typeName);
            await template.EndNewAsync(authentication, column);
        }

        public static async Task<ITypeCategory> AddNewCategoryAsync(this ITypeCategory category, Authentication authentication)
        {
            var newName = NameUtility.GenerateNewName("Folder", category.Categories.Select(item => item.Name));
            return await category.AddNewCategoryAsync(authentication, newName);
        }

        public static async Task<ITableCategory> AddNewCategoryAsync(this ITableCategory category, Authentication authentication)
        {
            var newName = NameUtility.GenerateNewName("Folder", category.Categories.Select(item => item.Name));
            return await category.AddNewCategoryAsync(authentication, newName);
        }

        public static async Task<Authentication> LoginAdminAsync(this ICremaHost cremaHost)
        {
            var users = cremaHost.GetService(typeof(IUserCollection)) as IUserCollection;
            var user = await users.Dispatcher.InvokeAsync(() => users.Random(item => item.Authority == Authority.Admin));
            var password = StringUtility.ToSecureString(user.Authority.ToString().ToLower());
            var token = await cremaHost.LoginAsync(user.ID, password);
            return await cremaHost.AuthenticateAsync(token);
        }
    }
}
