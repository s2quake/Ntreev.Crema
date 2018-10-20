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
using System.Threading.Tasks;
using Ntreev.Crema.Services.Extensions;

namespace Ntreev.Crema.Bot.Tasks
{
    [Export(typeof(ITaskProvider))]
    [Export(typeof(ITypeTemplateTask))]
    [TaskClass]
    class ITypeTemplateTask : ITaskProvider
    {
        public async Task InvokeAsync(TaskContext context)
        {
            var authentication = context.Authentication;
            var template = context.Target as ITypeTemplate;
            if (context.IsCompleted(template) == true)
            {
                try
                {
                    var domain = template.Domain;
                    if (domain != null && await domain.Users.ContainsAsync(authentication.ID) == true)
                    {
                        if (await template.Dispatcher.InvokeAsync(() => template.Any()))
                            await template.EndEditAsync(authentication);
                        else
                            await template.CancelEditAsync(authentication);
                    }
                }
                catch
                {
                    await template.CancelEditAsync(authentication);
                }
                finally
                {
                    context.Pop(template);
                    context.Complete(context.Target);
                }
            }
            else
            {
                if (await template.Dispatcher.InvokeAsync(() => template.VerifyAccessType(authentication, AccessType.Developer)) == false)
                {
                    context.Pop(template);
                    return;
                }

                var domain = template.Domain;
                if (domain == null)
                {
                    if (context.AllowException == false)
                    {
                        if ((await this.GetTablesAsync(template, item => item.TableState != TableState.None)).Any() == true)
                        {
                            context.Pop(template);
                            return;
                        }
                    }
                    await template.BeginEditAsync(authentication);
                    domain = template.Domain;
                }

                if (await domain.Users.ContainsAsync(authentication.ID) == false)
                {
                    context.Pop(template);
                    return;
                }


                if (template.IsNew == true ||
                    await template.Dispatcher.InvokeAsync(() => template.Any()) == false ||
                    RandomUtility.Within(25) == true)
                {
                    var member = await template.AddNewAsync(authentication);
                    context.Push(member);
                    context.State = System.Data.DataRowState.Detached;
                }
                else
                {
                    var member = template.Random();
                    context.Push(member);
                }
            }
        }

        public Type TargetType
        {
            get { return typeof(ITypeTemplate); }
        }

        [TaskMethod(Weight = 10)]
        public async Task SetTypeNameAsync(ITypeTemplate template, TaskContext context)
        {
            var authentication = context.Authentication;
            var tableName = RandomUtility.NextIdentifier();
            await template.SetTypeNameAsync(authentication, tableName);
        }

        [TaskMethod(Weight = 10)]
        public async Task SetIsFlagAsync(ITypeTemplate template, TaskContext context)
        {
            var authentication = context.Authentication;
            var isFlag = RandomUtility.NextBoolean();
            await template.SetIsFlagAsync(authentication, isFlag);
        }

        [TaskMethod(Weight = 10)]
        public async Task SetCommentAsync(ITypeTemplate template, TaskContext context)
        {
            var authentication = context.Authentication;
            var comment = RandomUtility.NextString();
            await template.SetCommentAsync(authentication, comment);
        }

        private bool CanEdit(ITypeTemplate template)
        {
            var type = template.Type;
            var tables = type.GetService(typeof(ITableCollection)) as ITableCollection;

            var query = from table in tables
                        from column in table.TableInfo.Columns
                        where column.DataType == type.Path
                        where table.TableState != TableState.None
                        select table;

            if (query.Any() == true)
                return false;
            return true;
        }

        private Task<ITable[]> GetTablesAsync(ITypeTemplate template, Func<ITable, bool> predicate)
        {
            return template.Dispatcher.InvokeAsync(()=>
            {
                var type = template.Type;
                var typePath = type.Path;
                var tables = type.GetService(typeof(ITableCollection)) as ITableCollection;

                var query = from item in tables
                            from columnInfo in item.TableInfo.Columns
                            where columnInfo.DataType == typePath
                            select item;

                return query.Distinct().Where(predicate).ToArray();
            });
        }
    }
}
