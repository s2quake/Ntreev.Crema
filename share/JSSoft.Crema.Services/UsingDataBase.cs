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
using System;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services
{
    public class UsingDataBase : IDisposable
    {
        private readonly Action action;

        private UsingDataBase(Action action)
        {
            this.action = action;
        }

        public static async Task<UsingDataBase> SetAsync(ICremaHost cremaHost, string dataBaseName, Authentication authentication)
        {
            if (cremaHost.GetService(typeof(IDataBaseContext)) is IDataBaseContext dataBaseContext)
            {
                var dataBase = await dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext[dataBaseName]);
                if (dataBase == null)
                    throw new DataBaseNotFoundException(dataBaseName);

                return await SetAsync(dataBase, authentication);
            }
            throw new NotImplementedException();
        }

        public static async Task<UsingDataBase> SetAsync(IDataBase dataBase, Authentication authentication)
        {
            if (dataBase.IsLoaded == false)
                await dataBase.LoadAsync(authentication);
            var contains = await dataBase.Dispatcher.InvokeAsync(() => dataBase.Contains(authentication));
            if (contains == false)
                await dataBase.EnterAsync(authentication);
            return new UsingDataBase(() =>
            {
                if (contains == false)
                    dataBase.LeaveAsync(authentication).Wait();
            })
            {
                DataBase = dataBase
            };
        }

        public static Task<UsingDataBase> SetAsync(IServiceProvider serviceProvider, Authentication authentication)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            return SetAsync(serviceProvider.GetService(typeof(IDataBase)) as IDataBase, authentication);
        }

        public IDataBase DataBase { get; private set; }

        #region IDisposable

        void IDisposable.Dispose()
        {
            this.action();
        }

        #endregion
    }
}
