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

namespace Ntreev.Crema.Bot.Tasks
{
    [Export(typeof(ITaskProvider))]
    [Export(typeof(ITableTemplateTask))]
    [TaskClass]
    public class ITableTemplateTask : ITaskProvider
    {
        public async Task InvokeAsync(TaskContext context)
        {
            var authentication = context.Authentication;
            var template = context.Target as ITableTemplate;
            if (context.IsCompleted(template) == true)
            {
                try
                {
                    var domain = template.Domain;
                    if (domain != null && await domain.Users.ContainsAsync(authentication.ID) == true)
                    {
                        var keys = await template.Dispatcher.InvokeAsync(() => template.PrimaryKey.ToArray());
                        if (keys.Length > 0)
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

                if (await template.Dispatcher.InvokeAsync(() => template.Target is ITable table && table.TemplatedParent != null) == true)
                {
                    context.Pop(template);
                    return;
                }

                var domain = template.Domain;
                if (domain == null)
                {
                    await template.BeginEditAsync(authentication);
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
            get { return typeof(ITableTemplate); }
        }

        public bool IsEnabled
        {
            get { return false; }
        }

        [TaskMethod(Weight = 10)]
        public async Task SetTableNameAsync(ITableTemplate template, TaskContext context)
        {
            var authentication = context.Authentication;
            var tableName = RandomUtility.NextIdentifier();
            await template.SetTableNameAsync(authentication, tableName);
        }

        [TaskMethod(Weight = 10)]
        public async Task SetTagsAsync(ITableTemplate template, TaskContext context)
        {
            var authentication = context.Authentication;
            var tags = (TagInfo)TagInfoUtility.Names.Random();
            await template.SetTagsAsync(authentication, tags);
        }

        [TaskMethod(Weight = 10)]
        public async Task SetCommentAsync(ITableTemplate template, TaskContext context)
        {
            var authentication = context.Authentication;
            var comment = RandomUtility.NextString();
            await template.SetCommentAsync(authentication, comment);
        }
    }
}
