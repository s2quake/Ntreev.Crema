//Released under the MIT License.
//
//Copyright (c) 2018 Ntreev Soft co., Ltd.
//
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
//documentation files (the "Software"), to deal in the Software without restriction, including without limitation the 
//rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit 
//persons to whom the Software is furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the 
//Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
//WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR 
//COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
//OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services;
using Ntreev.Library;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services
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
            if (cremaHost.GetService(typeof(IDataBaseCollection)) is IDataBaseCollection dataBases)
            {
                var dataBase = await dataBases.Dispatcher.InvokeAsync(() => dataBases[dataBaseName]);
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
            if (await dataBase.ContainsAsync(authentication) == false)
                await dataBase.EnterAsync(authentication);
            return new UsingDataBase(() => dataBase.LeaveAsync(authentication).Wait()) { DataBase = dataBase };
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
