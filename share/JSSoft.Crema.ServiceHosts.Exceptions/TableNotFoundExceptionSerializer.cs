// MIT License
// 
// Copyright (c) 2019 Jeesu Choi
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using JSSoft.Communication;
using JSSoft.Crema.ServiceModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace JSSoft.Crema.ServiceHosts.Exceptions
{
    [Export(typeof(IExceptionDescriptor))]
    [Export(typeof(IDataSerializer))]
    class TableNotFoundExceptionSerializer : ExceptionSerializerBase<TableNotFoundException>
    {
        public TableNotFoundExceptionSerializer()
            : base(new Guid("1b5ab86e-1803-4ff1-8f49-79cc13d25f61"))
        {

        }

        public override Type[] PropertyTypes => new Type[] { typeof(string) };

        protected override TableNotFoundException CreateInstance(object[] args)
        {
            if (args[0] is string tableName)
                return new TableNotFoundException(tableName);
            throw new NotImplementedException();
        }

        protected override object[] SelectProperties(TableNotFoundException e)
        {
            return new object[] { e.TableName };
        }
    }
}
