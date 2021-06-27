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
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using System;
using JSSoft.Crema.Services.Users.Serializations;
using System.Collections.Generic;
using JSSoft.Crema.Services.Extensions;
using JSSoft.Library.Random;
using JSSoft.Crema.Services.Test.Extensions;
using JSSoft.Crema.Services.Random;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JSSoft.Library.IO;
using JSSoft.Crema.Data;
using JSSoft.Library;
using System.Linq;
using JSSoft.Crema.Services.Test.Filters;

namespace JSSoft.Crema.Services.Test
{
    partial class TestApplication : CremaBootstrapper
    {
        public async Task InitializeAsync(TestContext context)
        {
            var repositoryPath = DirectoryUtility.Prepare(context.TestRunDirectory, "repo", context.FullyQualifiedTestClassName);
            var userInfos = UserInfoGenerator.Generate(0, 0);
            var dataSet = new CremaDataSet();
            await Task.Run(() => CremaBootstrapper.CreateRepositoryInternal(this, repositoryPath, "git", "xml", string.Empty, userInfos, dataSet));
            this.BasePath = repositoryPath;
            this.cremaHost = this.GetService(typeof(ICremaHost)) as ICremaHost;
        }

        public async Task ReleaseAsync()
        {
            await Task.Run(() => DirectoryUtility.Delete(this.BasePath));
            this.Dispose();
        }

        // public Task<IUser> PrepareUserAsync()
        // {
        //     return PrepareUserAsync(UserFilter.Empty);
        // }

        // public Task<IUser> PrepareUserAsync(UserFlags userFlags)
        // {
        //     return PrepareUserAsync(userFlags, item => true);
        // }

        // public Task<IUser> PrepareUserAsync(UserFlags userFlags, Func<IUser, bool> predicate)
        // {
        //     return PrepareUserAsync(new UserFilter(userFlags, predicate));
        // }

        // public Task<IUser> PrepareUserAsync(Func<IUser, bool> predicate)
        // {
        //     return PrepareUserAsync(UserFlags.None, predicate);
        // }

        // public async Task<IUser> PrepareUserAsync(UserFilter filter)
        // {
        //     var userCollection = this.cremaHost.GetService(typeof(IUserCollection)) as IUserCollection;
        //     var userCategoryCollection = this.cremaHost.GetService(typeof(IUserCategoryCollection)) as IUserCategoryCollection;
        //     var userContext = this.cremaHost.GetService(typeof(IUserContext)) as IUserContext;
        //     var user = await userCollection.GetRandomUserAsync(filter.UserFlags, filter);
        //     if (user is null)
        //     {
        //         var authority = GetAuthorities(filter.UserFlags).Random();
        //         var userStates = SelectUserState(filter.UserFlags);
        //         var banStates = SelectBanState(filter.UserFlags);
        //         var category = await userCategoryCollection.GetRandomUserCategoryAsync();
        //         var newUser = await category.GenerateUserAsync(Authentication.System, authority);
        //         var password = newUser.GetPassword();
        //         if (userStates.Any() == true && userStates.Random() == UserState.Online)
        //             await this.cremaHost.LoginAsync(newUser.ID, password);
        //         if (banStates.Any() == true && banStates.Random() == true)
        //             await newUser.BanAsync(Authentication.System, RandomUtility.NextString());
        //         return newUser;
        //     }
        //     return user;
        // }

        public Task<IUserItem> PrepareUserItemAsync()
        {
            return PrepareUserItemAsync(new UserItemFilter());
        }

        public Task<IUserItem> PrepareUserItemAsync(Type type)
        {
            return PrepareUserItemAsync(new UserItemFilter(type));
        }

        public async Task<IUserItem> PrepareUserItemAsync(UserItemFilter filter)
        {
            var userCollection = this.cremaHost.GetService(typeof(IUserCollection)) as IUserCollection;
            var userCategoryCollection = this.cremaHost.GetService(typeof(IUserCategoryCollection)) as IUserCategoryCollection;
            var userContext = this.cremaHost.GetService(typeof(IUserContext)) as IUserContext;
            var userItem = await userContext.GetRandomUserItemAsync(filter);
            if (userItem is null)
            {
                var s = RandomUtility.Within(50);
                if (filter.Type == typeof(IUser) || (filter.Type == null && s == true))
                {
                    var category = await userCategoryCollection.GetRandomUserCategoryAsync();
                    var user = await category.GenerateUserAsync(Authentication.System);
                    userItem = user as IUserItem;
                }
                else if (filter.Type == typeof(IUserCategory) || (filter.Type == null && s == false))
                {
                    var category = await PrepareUserCategoryAsync(filter);
                    userItem = category as IUserItem;
                }
            }
            return userItem;
        }

        public Task<IUserCategory> PrepareUserCategoryAsync()
        {
            return PrepareUserCategoryAsync(UserCategoryFilter.Empty);
        }

        public async Task<IUserCategory> PrepareUserCategoryAsync(UserCategoryFilter filter)
        {
            var userCollection = this.cremaHost.GetService(typeof(IUserCollection)) as IUserCollection;
            var userCategoryCollection = this.cremaHost.GetService(typeof(IUserCategoryCollection)) as IUserCategoryCollection;
            var userContext = this.cremaHost.GetService(typeof(IUserContext)) as IUserContext;
            var userCategory = await userCategoryCollection.GetRandomUserCategoryAsync(filter);
            var rootCategory = userCategoryCollection.Root;
            if (userCategory is null)
            {
                var parent = await userCategoryCollection.GetRandomUserCategoryAsync();
                var name = await parent.GenerateNewCategoryNameAsync();
                var category = await parent.AddNewCategoryAsync(Authentication.System, name);
                if (filter.HasCategories == true)
                {
                    await category.GenerateUserCategoryAsync(Authentication.System);
                }
                if (filter.HasUsers == true)
                {
                    await category.GenerateUserAsync(Authentication.System);
                }
                if (filter.CategoryToMove != null)
                {
                    return await rootCategory.GenerateUserCategoryAsync(Authentication.System);
                }
                return category;
            }
            return userCategory;
        }

        private static UserState[] SelectUserState(UserFlags userFlags)
        {
            var userStateList = new List<UserState>();
            var userStateFlag = userFlags & (UserFlags.Online | UserFlags.Offline);
            if (userFlags.HasFlag(UserFlags.Online) == true)
            {
                userStateList.Add(UserState.Online);
            }
            if (userFlags.HasFlag(UserFlags.Offline) == true)
            {
                userStateList.Add(UserState.None);
            }
            return userStateList.ToArray();
        }

        private static bool[] SelectBanState(UserFlags userFlags)
        {
            var banStateList = new List<bool>();
            var banStateFlag = userFlags & (UserFlags.NotBanned | UserFlags.Banned);
            if (userFlags.HasFlag(UserFlags.NotBanned) == true)
            {
                banStateList.Add(false);
            }
            if (userFlags.HasFlag(UserFlags.Banned) == true)
            {
                banStateList.Add(true);
            }
            return banStateList.ToArray();
        }

        private static Authority[] GetAuthorities(UserFlags userFlags)
        {
            var authorityList = new List<Authority>();
            var authorityFlag = userFlags & (UserFlags.Admin | UserFlags.Member | UserFlags.Guest);
            if (userFlags.HasFlag(UserFlags.Admin) == true || authorityFlag == UserFlags.None)
            {
                authorityList.Add(Authority.Admin);
            }
            if (userFlags.HasFlag(UserFlags.Member) == true || authorityFlag == UserFlags.None)
            {
                authorityList.Add(Authority.Member);
            }
            if (userFlags.HasFlag(UserFlags.Guest) == true || authorityFlag == UserFlags.None)
            {
                authorityList.Add(Authority.Guest);
            }
            return authorityList.ToArray();
        }
    }
}
