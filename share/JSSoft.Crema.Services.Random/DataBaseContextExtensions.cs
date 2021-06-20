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

using System;
using System.Threading.Tasks;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.ServiceModel.Extensions;
using JSSoft.Crema.Services.Extensions;
using JSSoft.Library.Random;

namespace JSSoft.Crema.Services.Random
{
    public static class DataBaseContextExtensions
    {
        public static Task<IDataBase> GetRandomDataBaseAsync(this IDataBaseContext dataBaseContext)
        {
            return GetRandomDataBaseAsync(dataBaseContext, DefaultPredicate);
        }

        public static Task<IDataBase> GetRandomDataBaseAsync(this IDataBaseContext dataBaseContext, Func<IDataBase, bool> predicate)
        {
            return dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext.RandomOrDefault(predicate));
        }

        public static Task<IDataBase> GetRandomDataBaseAsync(this IDataBaseContext dataBaseContext, DataBaseFlags dataBaseFlags)
        {
            return GetRandomDataBaseAsync(dataBaseContext, dataBaseFlags, DefaultPredicate);
        }

        public static Task<IDataBase> GetRandomDataBaseAsync(this IDataBaseContext dataBaseContext, DataBaseFlags dataBaseFlags, Func<IDataBase, bool> predicate)
        {
            return dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext.RandomOrDefault(item => TestFlags(item, dataBaseFlags) && predicate(item) == true));

            static bool TestFlags(IDataBase dataBase, DataBaseFlags dataBaseFlags)
            {
                if (TestDataBaseStateFlags(dataBase, dataBaseFlags) == false)
                    return false;
                if (TestDataBaseLockedFlags(dataBase, dataBaseFlags) == false)
                    return false;
                if (TestDataBaseAccessFlags(dataBase, dataBaseFlags) == false)
                    return false;
                return true;
            }

            static bool TestDataBaseStateFlags(IDataBase dataBase, DataBaseFlags dataBaseFlags)
            {
                if (dataBaseFlags.HasFlag(DataBaseFlags.NotLoaded) == true && dataBase.DataBaseState != DataBaseState.None)
                    return false;
                if (dataBaseFlags.HasFlag(DataBaseFlags.Loaded) == true && dataBase.DataBaseState != DataBaseState.Loaded)
                    return false;
                return true;
            }

            static bool TestDataBaseLockedFlags(IDataBase dataBase, DataBaseFlags dataBaseFlags)
            {
                if (dataBaseFlags.HasFlag(DataBaseFlags.NotLocked) == true && dataBase.IsLocked == true)
                    return false;
                if (dataBaseFlags.HasFlag(DataBaseFlags.Locked) == true && dataBase.IsLocked == false)
                    return false;
                return true;
            }

            static bool TestDataBaseAccessFlags(IDataBase dataBase, DataBaseFlags dataBaseFlags)
            {
                if (dataBaseFlags.HasFlag(DataBaseFlags.Private) == true && dataBase.IsPrivate == false)
                    return false;
                if (dataBaseFlags.HasFlag(DataBaseFlags.Public) == true && dataBase.IsPrivate == true)
                    return false;
                return true;
            }
        }

        public static async Task<IDataBase> AddNewRandomDataBaseAsync(this IDataBaseContext dataBaseContext, Authentication authentication)
        {
            var name = RandomUtility.NextName();
            var dataBaseName = await dataBaseContext.GenerateNewDataBaseNameAsync(name);
            var comment = RandomUtility.NextString();
            return await dataBaseContext.AddNewDataBaseAsync(authentication, dataBaseName, comment);
        }

        public static async Task<IDataBase> AddNewRandomDataBaseAsync(this IDataBaseContext dataBaseContext, DataBaseFlags dataBaseFlags, Authentication authentication)
        {
            dataBaseFlags.Validate();
            var dataBase = await AddNewRandomDataBaseAsync(dataBaseContext, authentication);
            if (dataBaseFlags.HasFlag(DataBaseFlags.Loaded) == true)
                await dataBase.LoadAsync(authentication);
            if (dataBaseFlags.HasFlag(DataBaseFlags.Private) == true)
                await dataBase.SetPrivateAsync(authentication);
            if (dataBaseFlags.HasFlag(DataBaseFlags.Locked) == true)
                await dataBase.LockAsync(authentication, RandomUtility.NextString());
            return dataBase;
        }

        private static bool DefaultPredicate(IDataBase dataBase) => true;
    }
}