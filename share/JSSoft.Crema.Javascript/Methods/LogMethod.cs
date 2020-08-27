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

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.ComponentModel.Composition;

namespace JSSoft.Crema.Javascript.Methods
{
    [Export(typeof(IScriptMethod))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    class LogMethod : ScriptMethodBase
    {
        public LogMethod()
        {

        }

        protected override Delegate CreateDelegate()
        {
            return new Action<object>(this.WriteLine);
        }

        private void WriteLine(object value)
        {
            if (value != null && ScriptContextBase.IsDictionaryType(value.GetType()))
            {
                var text = JsonConvert.SerializeObject(value, Formatting.Indented);
                this.Context.Out.WriteLine(text);
            }
            else if (value is System.Dynamic.ExpandoObject exobj)
            {
                var text = JsonConvert.SerializeObject(exobj, Formatting.Indented, new ExpandoObjectConverter());
                this.Context.Out.WriteLine(text);
            }
            else if (value != null && value.GetType().IsArray)
            {
                var text = JsonConvert.SerializeObject(value, Formatting.Indented);
                this.Context.Out.WriteLine(text);
            }
            else if (value is bool b)
            {
                this.Context.Out.WriteLine(b.ToString().ToLower());
            }
            else
            {
                this.Context.Out.WriteLine(value);
            }
        }
    }
}
