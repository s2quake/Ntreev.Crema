﻿// Released under the MIT License.
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

using System;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Extensions
{
    public static class UserContextExtensions
    {
        public static Task NotifyMessageAsync(this IUserContext userContext, Authentication authentication, string message)
        {
            return userContext.NotifyMessageAsync(authentication, new string[] { }, message);
        }

        public static Task<bool> ContainsUserItemAsync(this IUserContext userContext, string itemPath)
        {
            return userContext.Dispatcher.InvokeAsync(() => userContext.Contains(itemPath));
        }

        public static Task<bool> ContainsUserAsync(this IUserContext userContext, string userID)
        {
            if (userContext.GetService(typeof(IUserCollection)) is IUserCollection userCollection)
            {
                return userCollection.ContainsAsync(userID);
            }
            throw new NotImplementedException();
        }

        public static Task<bool> ContainsUserCategoryAsync(this IUserContext userContext, string categoryPath)
        {
            if (userContext.GetService(typeof(IUserCategoryCollection)) is IUserCategoryCollection userCategoryCollection)
            {
                return userCategoryCollection.ContainsAsync(categoryPath);
            }
            throw new NotImplementedException();
        }

        public static Task<IUserItem> GetUserItemAsync(this IUserContext userContext, string itemPath)
        {
            return userContext.Dispatcher.InvokeAsync(() => userContext[itemPath]);
        }

        public static Task<IUserItem[]> GetUserItemsAsync(this IUserContext userContext)
        {
            return userContext.Dispatcher.InvokeAsync(() => userContext.ToArray());
        }

        public static Task<IUser> GetUserAsync(this IUserContext userContext, string userID)
        {
            if (userContext.GetService(typeof(IUserCollection)) is IUserCollection userCollection)
            {
                return userCollection.Dispatcher.InvokeAsync(() => userCollection[userID]);
            }
            throw new NotImplementedException();
        }

        public static Task<IUser[]> GetUsersAsync(this IUserContext userContext)
        {
            if (userContext.GetService(typeof(IUserCollection)) is IUserCollection userCollection)
            {
                return userCollection.Dispatcher.InvokeAsync(() => userCollection.ToArray());
            }
            throw new NotImplementedException();
        }

        public static Task<IUserCategory> GetUserCategoryAsync(this IUserContext userContext, string categoryPath)
        {
            if (userContext.GetService(typeof(IUserCategoryCollection)) is IUserCategoryCollection userCategoryCollection)
            {
                return userCategoryCollection.Dispatcher.InvokeAsync(() => userCategoryCollection[categoryPath]);
            }
            throw new NotImplementedException();
        }

        public static Task<IUserCategory[]> GetUserCategoriesAsync(this IUserContext userContext)
        {
            if (userContext.GetService(typeof(IUserCategoryCollection)) is IUserCategoryCollection userCategoryCollection)
            {
                return userCategoryCollection.Dispatcher.InvokeAsync(() => userCategoryCollection.ToArray());
            }
            throw new NotImplementedException();
        }
    }
}
