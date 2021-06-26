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
    public static class UserContextExtensions
    {
        public static Task<IUser> GetRandomUserAsync(this IUserContext userContext)
        {
            return GetRandomUserAsync(userContext, DefaultPredicate);
        }

        public static Task<IUser> GetRandomUserAsync(this IUserContext userContext, Func<IUser, bool> predicate)
        {
            if (userContext.GetService(typeof(IUserCollection)) is IUserCollection userCollection)
            {
                return userCollection.Dispatcher.InvokeAsync(() => userCollection.Random(predicate));
            }
            throw new NotImplementedException();
        }

        public static Task<IUser> GetRandomUserAsync(this IUserContext userContext, UserFlags userFlags)
        {
            return GetRandomUserAsync(userContext, userFlags, DefaultPredicate);
        }

        public static Task<IUser> GetRandomUserAsync(this IUserContext userContext, UserFlags userFlags, Func<IUser, bool> predicate)
        {
            if (userContext.GetService(typeof(IUserCollection)) is IUserCollection userCollection)
            {
                return userCollection.GetRandomUserAsync(userFlags, predicate);
            }
            throw new NotImplementedException();
        }

        public static Task<IUserCategory> GetRandomUserCategoryAsync(this IUserContext userContext)
        {
            return GetRandomUserCategoryAsync(userContext, DefaultPredicate);
        }

        public static Task<IUserCategory> GetRandomUserCategoryAsync(this IUserContext userContext, Func<IUserCategory, bool> predicate)
        {
            if (userContext.GetService(typeof(IUserCategoryCollection)) is IUserCategoryCollection userCategoryCollection)
            {
                return userCategoryCollection.Dispatcher.InvokeAsync(() => userCategoryCollection.Random(predicate));
            }
            throw new NotImplementedException();
        }

        public static Task<IUserItem> GetRandomUserItemAsync(this IUserContext userContext)
        {
            return GetRandomUserItemAsync(userContext, DefaultPredicate);
        }

        public static Task<IUserItem> GetRandomUserItemAsync(this IUserContext userContext, Func<IUserItem, bool> predicate)
        {
            return userContext.Dispatcher.InvokeAsync(() => userContext.Random(predicate));
        }

        public static Task<IUserItem> GetRandomUserItemAsync(this IUserContext userContext, Type type)
        {
            return GetRandomUserItemAsync(userContext, type, DefaultPredicate);
        }

        public static Task<IUserItem> GetRandomUserItemAsync(this IUserContext userContext, Type type, Func<IUserItem, bool> predicate)
        {
            return userContext.Dispatcher.InvokeAsync(() => userContext.Random(item => type.IsAssignableFrom(item.GetType()) && predicate(item)));
        }

        public static async Task<IUserItem> GenerateAsync(this IUserContext context, Authentication authentication)
        {
            if (RandomUtility.Within(25) == true)
                return (await context.GenerateCategoryAsync(authentication)) as IUserItem;
            else
                return (await context.GenerateUserAsync(authentication)) as IUserItem;
        }

        public static async Task<IUserItem[]> GenerateManyAsync(this IUserContext context, Authentication authentication, int count)
        {
            var itemList = new List<IUserItem>(count);
            for (var i = 0; i < count; i++)
            {
                var item = await context.GenerateAsync(authentication);
                itemList.Add(item);
            }
            return itemList.ToArray();
        }

        public static async Task<IUserCategory[]> GenerateCategoriesAsync(this IUserContext userContext, Authentication authentication, int count)
        {
            var itemList = new List<IUserCategory>(count);
            for (var i = 0; i < count; i++)
            {
                var item = await userContext.GenerateCategoryAsync(authentication);
                itemList.Add(item);
            }
            return itemList.ToArray();
        }

        public static async Task<IUserCategory> GenerateCategoryAsync(this IUserContext userContext, Authentication authentication)
        {
            if (RandomUtility.Within(50) == true)
            {
                return await userContext.Root.AddNewCategoryAsync(authentication, RandomUtility.NextIdentifier());
            }
            else
            {
                var category = await userContext.GetRandomUserCategoryAsync();
                // if (GetLevel(category, (i) => i.Parent) > 4)
                //     return false;
                return await category.AddNewCategoryAsync(authentication, RandomUtility.NextIdentifier());
            }
        }

        public static async Task<IUser[]> GenerateUsersAsync(this IUserContext userContext, Authentication authentication, int count)
        {
            var itemList = new List<IUser>(count);
            for (var i = 0; i < count; i++)
            {
                var item = await userContext.GenerateUserAsync(authentication);
                itemList.Add(item);
            }
            return itemList.ToArray();
        }

        public static async Task<IUser> GenerateUserAsync(this IUserContext userContext, Authentication authentication)
        {
            var category = await userContext.GetRandomUserCategoryAsync();
            return await category.GenerateUserAsync(authentication);
        }

        public static async Task<IUser> GenerateUserAsync(this IUserContext userContext, Authentication authentication, Authority authority)
        {
            var category = await userContext.GetRandomUserCategoryAsync();
            return await category.GenerateUserAsync(authentication, authority);
        }

        public static Task<string> GenerateUserIDAsync(this IUserContext userContext)
        {
            return GenerateUserIDAsync(userContext, "user");
        }

        public static async Task<string> GenerateUserIDAsync(this IUserContext userContext, string name)
        {
            var query = from item in await userContext.GetUsersAsync()
                        select item.ID;
            return NameUtility.GenerateNewName(name, query);
        }

        public static Authority GetRandomAuthority(this IUserContext userContext)
        {
            var items = new Authority[] { Authority.Admin, Authority.Member, Authority.Guest };
            return items.Random();
        }

        public static SecureString GetPassword(this IUserContext userContext, Authority authority)
        {
            return authority.ToString().ToLower().ToSecureString();
        }

        private static bool DefaultPredicate(IUserItem _) => true;

        private static bool DefaultPredicate(IUserCategory _) => true;

        private static bool DefaultPredicate(IUser _) => true;
    }
}
