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
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Random
{
    public static class UserCollectionExtensions
    {
        public static Task<IUser> GetRandomUserAsync(this IUserCollection userCollection)
        {
            return GetRandomUserAsync(userCollection, DefaultPredicate);
        }

        public static Task<IUser> GetRandomUserAsync(this IUserCollection userCollection, Func<IUser, bool> predicate)
        {
            return userCollection.Dispatcher.InvokeAsync(() => userCollection.Random(predicate));
        }

        public static Task<IUser> GetRandomUserAsync(this IUserCollection userCollection, Authority authority)
        {
            return GetRandomUserAsync(userCollection, authority, DefaultPredicate);
        }

        public static Task<IUser> GetRandomUserAsync(this IUserCollection userCollection, Authority authority, Func<IUser, bool> predicate)
        {
            return userCollection.Dispatcher.InvokeAsync(() => userCollection.Random(item => item.Authority == authority && predicate(item) == true));
        }

        public static Task<IUser> GetRandomUserAsync(this IUserCollection userCollection, Authority authority, UserState userState)
        {
            return GetRandomUserAsync(userCollection, authority, userState, DefaultPredicate);
        }

        public static Task<IUser> GetRandomUserAsync(this IUserCollection userCollection, Authority authority, UserState userState, Func<IUser, bool> predicate)
        {
            return userCollection.Dispatcher.InvokeAsync(() => userCollection.Random(item => item.Authority == authority && item.UserState == userState && predicate(item) == true));
        }

        public static Task<IUser> GetRandomUserAsync(this IUserCollection userCollection, UserState userState)
        {
            return GetRandomUserAsync(userCollection, userState, DefaultPredicate);
        }

        public static Task<IUser> GetRandomUserAsync(this IUserCollection userCollection, UserState userState, Func<IUser, bool> predicate)
        {
            return userCollection.Dispatcher.InvokeAsync(() => userCollection.Random(item => item.UserState == userState && predicate(item) == true));
        }

        private static bool DefaultPredicate(IUser _) => true;
    }
}
