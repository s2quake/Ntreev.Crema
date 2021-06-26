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
using System.Linq;
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
            return userCollection.Dispatcher.InvokeAsync(() => userCollection.RandomOrDefault(predicate));
        }

        public static Task<IUser> GetRandomUserAsync(this IUserCollection userCollection, UserFlags userFlags)
        {
            return GetRandomUserAsync(userCollection, userFlags, DefaultPredicate);
        }

        public static Task<IUser> GetRandomUserAsync(this IUserCollection userCollection, UserFlags userFlags, Func<IUser, bool> predicate)
        {
            return userCollection.Dispatcher.InvokeAsync(() => userCollection.RandomOrDefault(item => TestFlags(item, userFlags) == true && predicate(item) == true));
        }

        public static Task<IUser[]> GetRandomUsersAsync(this IUserCollection userCollection)
        {
            return GetRandomUsersAsync(userCollection, DefaultPredicate);
        }

        public static Task<IUser[]> GetRandomUsersAsync(this IUserCollection userCollection, Func<IUser, bool> predicate)
        {
            return GetRandomUsersAsync(userCollection, UserFlags.None, predicate);
        }

        public static Task<IUser[]> GetRandomUsersAsync(this IUserCollection userCollection, UserFlags userFlags)
        {
            return GetRandomUsersAsync(userCollection, userFlags, DefaultPredicate);
        }

        public static Task<IUser[]> GetRandomUsersAsync(this IUserCollection userCollection, UserFlags userFlags, Func<IUser, bool> predicate)
        {
            return userCollection.Dispatcher.InvokeAsync(() =>
            {
                var query = from item in userCollection
                            where TestFlags(item, userFlags) == true && predicate(item) == true
                            let i = RandomUtility.Next<int>()
                            orderby i
                            select item;
                return query.ToArray();
            });
        }

        private static bool TestFlags(IUser user, UserFlags userFlags)
        {
            return TestAuthorityFlags(user, userFlags) && TestUserStateFlags(user, userFlags) && TestBanInfoFlags(user, userFlags);
        }

        private static bool TestAuthorityFlags(IUser user, UserFlags userFlags)
        {
            var mask = userFlags & (UserFlags.Admin | UserFlags.Member | UserFlags.Guest);
            if (mask.HasFlag(UserFlags.Admin) == true && user.Authority == Authority.Admin)
                return true;
            if (mask.HasFlag(UserFlags.Member) == true && user.Authority == Authority.Member)
                return true;
            if (mask.HasFlag(UserFlags.Guest) == true && user.Authority == Authority.Guest)
                return true;
            return mask == UserFlags.None;
        }

        private static bool TestUserStateFlags(IUser user, UserFlags userFlags)
        {
            if (userFlags.HasFlag(UserFlags.Offline) == true && user.UserState != UserState.None)
                return false;
            if (userFlags.HasFlag(UserFlags.Online) == true && user.UserState != UserState.Online)
                return false;
            return true;
        }

        private static bool TestBanInfoFlags(IUser user, UserFlags userFlags)
        {
            if (userFlags.HasFlag(UserFlags.NotBanned) == true && user.BanInfo.IsBanned == true)
                return false;
            if (userFlags.HasFlag(UserFlags.Banned) == true && user.BanInfo.IsNotBanned == true)
                return false;
            return true;
        }

        private static bool DefaultPredicate(IUser _) => true;
    }
}
