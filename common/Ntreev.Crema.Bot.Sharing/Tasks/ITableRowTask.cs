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

using Ntreev.Crema.Data;
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services;
using Ntreev.Library;
using Ntreev.Library.Random;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;
using Ntreev.Crema.Services.Random;
using System.Threading.Tasks;

namespace Ntreev.Crema.Bot.Tasks
{
    [Export(typeof(ITaskProvider))]
    [Export(typeof(ITableRowTask))]
    [TaskClass]
    class ITableRowTask : ITaskProvider
    {
        public async Task InvokeAsync(TaskContext context)
        {
            var row = context.Target as ITableRow;
            var content = row.Content;
            if (context.IsCompleted(row) == true)
            {
                if (object.Equals(context.State, System.Data.DataRowState.Detached) == true)
                {
                    try
                    {
                        if (Verify() == true)
                            await content.EndNewAsync(context.Authentication, row);
                    }
                    catch
                    {

                    }
                }

                context.State = null;
                context.Pop(row);
            }

            bool Verify()
            {
                if (context.AllowException == true)
                    return true;

                var domain = content.Domain;
                var dataSet = domain.Source as CremaDataSet;
                var dataTable = dataSet.Tables[content.Table.Name];

                foreach (var item in dataTable.Columns)
                {
                    if (item.AllowDBNull == false && row[item.ColumnName] == null)
                        return false;
                }

                return true;
            }
        }

        public Type TargetType
        {
            get { return typeof(ITableRow); }
        }

        public bool IsEnabled
        {
            get { return false; }
        }

        [TaskMethod(Weight = 1)]
        public async Task DeleteAsync(ITableRow row, TaskContext context)
        {
            if (object.Equals(context.State, System.Data.DataRowState.Detached) == false)
            {
                await row.DeleteAsync(context.Authentication);
                context.State = System.Data.DataRowState.Deleted;
                context.Complete(row);
            }
        }

        [TaskMethod(Weight = 5)]
        public async Task SetIsEnabledAsync(ITableRow row, TaskContext context)
        {
            if (object.Equals(context.State, System.Data.DataRowState.Detached) == true)
                return;
            var isEnabled = RandomUtility.NextBoolean();
            await row.SetIsEnabledAsync(context.Authentication, isEnabled);
        }

        [TaskMethod(Weight = 5)]
        public async Task SetTagsAsync(ITableRow row, TaskContext context)
        {
            if (object.Equals(context.State, System.Data.DataRowState.Detached) == true)
                return;
            var tags = TagInfoUtility.Names.Random();
            await row.SetTagsAsync(context.Authentication, (TagInfo)tags);
        }

        [TaskMethod]
        public async Task SetFieldAsync(ITableRow row, TaskContext context)
        {
            if (object.Equals(context.State, System.Data.DataRowState.Detached) == true)
                return;
            await Task.Delay(0);
        }

        [TaskMethod]
        public async Task MoveLeftAsync(ITableRow row, TaskContext context)
        {
            if (object.Equals(context.State, System.Data.DataRowState.Detached) == true)
                return;
            await Task.Delay(0);
        }
    }
}
