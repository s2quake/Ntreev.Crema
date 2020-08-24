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

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace Ntreev.Crema.Javascript.Methods
{
    [Export(typeof(IScriptMethod))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    class AddEventListenerMethod : ScriptMethodBase
    {
        private readonly CremaEventListenerHost[] eventListeners;
        private CremaEventListenerContext eventListenerContext;

        [ImportingConstructor]
        public AddEventListenerMethod([ImportMany] IEnumerable<CremaEventListenerHost> eventListeners)
        {
            this.eventListeners = eventListeners.ToArray();
        }

        protected override Delegate CreateDelegate()
        {
            return new Action<CremaEvents, CremaEventListener>(this.AddEventListener);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }

        protected override void OnDisposed()
        {
            base.OnDisposed();
            this.eventListenerContext?.Dispose();
        }

        private void AddEventListener(CremaEvents eventName, CremaEventListener listener)
        {
            if (this.Context.Properties.ContainsKey(typeof(CremaEventListenerContext)) == false)
            {
                this.eventListenerContext = new CremaEventListenerContext(this.eventListeners);
                this.Context.Properties[typeof(CremaEventListenerContext)] = this.eventListenerContext;
            }

            if (this.eventListenerContext != null)
            {
                this.eventListenerContext.AddEventListener(eventName, listener);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
