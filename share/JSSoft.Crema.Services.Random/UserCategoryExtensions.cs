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

using JSSoft.Crema.Data.Xml.Schema;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services.Extensions;
using JSSoft.Library;
using JSSoft.Library.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Random
{
    public static class UserCategoryExtensions
    {
        public static Task<IUser> GenerateUserAsync(this IUserCategory userCategory, Authentication authentication)
        {
            var authorities = new Authority[] { Authority.Admin, Authority.Member, Authority.Guest };
            var authority = authorities.Random();
            return GenerateUserAsync(userCategory, authentication, authority);
        }

        public static async Task<IUser> GenerateUserAsync(this IUserCategory userCategory, Authentication authentication, Authority authority)
        {
            var userCollection = userCategory.GetService(typeof(IUserCollection)) as IUserCollection;
            var newID = await userCollection.GenerateNewUserIDAsync("user");
            var newName = newID.Replace("user", $"{authority}User");
            var password = authority.ToString().ToLower().ToSecureString();
            return await userCategory.AddNewUserAsync(authentication, newID, password, newName, authority);
        }

        public static async Task<IUserCategory> GenerateUserCategoryAsync(this IUserCategory userCategory, Authentication authentication)
        {
            var name = await userCategory.GenerateNewCategoryNameAsync();
            return await userCategory.AddNewCategoryAsync(authentication, name);
        }
    }
}
