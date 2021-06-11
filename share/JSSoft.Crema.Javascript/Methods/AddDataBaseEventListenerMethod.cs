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

using JSSoft.Crema.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace JSSoft.Crema.Javascript.Methods
{
    [Export(typeof(IScriptMethod))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    class AddDataBaseEventListenerMethod : ScriptMethodBase
    {
        private readonly ICremaHost cremaHost;
        private readonly DataBaseEventListenerHost[] eventListeners;
        private DataBaseEventListenerContext eventListenerContext;

        [ImportingConstructor]
        public AddDataBaseEventListenerMethod(ICremaHost cremaHost, [ImportMany] IEnumerable<DataBaseEventListenerHost> eventListeners)
        {
            this.cremaHost = cremaHost;
            this.eventListeners = eventListeners.ToArray();
        }

        protected override Delegate CreateDelegate()
        {
            return new Action<DataBaseEvents, DataBaseEventListener>(this.AddDataBaseEventListener);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }

        protected override void OnDisposed()
        {
            base.OnDisposed();
            this.eventListenerContext?.DisposeAsync();
        }

        private void AddDataBaseEventListener(DataBaseEvents eventName, DataBaseEventListener listener)
        {
            if (this.Context.Properties.ContainsKey(typeof(DataBaseEventListenerContext)) == false)
            {
                this.eventListenerContext = new DataBaseEventListenerContext(this.cremaHost, this.eventListeners);
                this.Context.Properties[typeof(DataBaseEventListenerContext)] = this.eventListenerContext;
            }

            if (this.eventListenerContext != null)
            {
                this.eventListenerContext.AddEventListenerAsync(eventName, listener);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
