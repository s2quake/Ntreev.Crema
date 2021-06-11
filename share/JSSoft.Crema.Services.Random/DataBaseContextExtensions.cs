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
            return dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext.Random(predicate));
        }

        public static Task<IDataBase> GetRandomDataBaseAsync(this IDataBaseContext dataBaseContext, DataBaseState dataBaseState)
        {
            return GetRandomDataBaseAsync(dataBaseContext, dataBaseState, DefaultPredicate);
        }

        public static Task<IDataBase> GetRandomDataBaseAsync(this IDataBaseContext dataBaseContext, DataBaseState dataBaseState, Func<IDataBase, bool> predicate)
        {
            return dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext.Random(item => item.DataBaseState == dataBaseState && predicate(item) == true));
        }

        public static async Task<IDataBase> AddNewRandomDataBaseAsync(this IDataBaseContext dataBaseContext, Authentication authentication)
        {
            var dataBaseName = await dataBaseContext.GenerateNewDataBaseNameAsync();
            var comment = RandomUtility.NextString();
            return await dataBaseContext.AddNewDataBaseAsync(authentication, dataBaseName, comment);
        }

        private static bool DefaultPredicate(IDataBase dataBase) => true;
    }
}