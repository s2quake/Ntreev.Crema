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
        public static void AddMember(this ITypeTemplate template, Authentication authentication, string name, long value, string comment)
        {
            var member = template.AddNew(authentication);
            member.SetName(authentication, name);
            member.SetValue(authentication, value);
            member.SetComment(authentication, comment);
            template.EndNew(authentication, member);
        }

        public static void AddKey(this ITableTemplate template, Authentication authentication, string name, string typeName)
        {
            var column = template.AddNew(authentication);
            column.SetName(authentication, name);
            column.SetIsKey(authentication, true);
            column.SetDataType(authentication, typeName);
            column.SetComment(authentication, string.Format("Key : {0}", typeName));
            template.EndNew(authentication, column);
        }

        public static void AddColumn(this ITableTemplate template, Authentication authentication, string name, string typeName)
        {
            var column = template.AddNew(authentication);
            column.SetName(authentication, name);
            column.SetDataType(authentication, typeName);
            template.EndNew(authentication, column);
        }

        public static ITypeCategory AddNewCategory(this ITypeCategory category, Authentication authentication)
        {
            var newName = NameUtility.GenerateNewName("Folder", category.Categories.Select(item => item.Name));
            return category.AddNewCategory(authentication, newName);
        }

        public static ITableCategory AddNewCategory(this ITableCategory category, Authentication authentication)
        {
            var newName = NameUtility.GenerateNewName("Folder", category.Categories.Select(item => item.Name));
            return category.AddNewCategory(authentication, newName);
        }

        public static Authentication LoginAdmin(this ICremaHost cremaHost)
        {
            return cremaHost.Dispatcher.Invoke(() =>
            {
                var userContext = cremaHost.GetService<IUserContext>();
                var user = userContext.Users.Random(item => item.Authority == Authority.Admin);
                return cremaHost.Login(user.ID, user.Authority.ToString().ToLower());
            });
        }
    }
}
