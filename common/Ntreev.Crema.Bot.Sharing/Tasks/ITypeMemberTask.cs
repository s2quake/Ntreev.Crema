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
    [Export(typeof(ITypeMemberTask))]
    [TaskClass]
    class ITypeMemberTask : ITaskProvider
    {
        public async Task InvokeAsync(TaskContext context)
        {
            var member = context.Target as ITypeMember;
            var template = member.Template;
            if (context.IsCompleted(member) == true)
            {
                if (object.Equals(context.State, System.Data.DataRowState.Detached) == true)
                {
                    try
                    {
                        await template.EndNewAsync(context.Authentication, member);
                    }
                    catch
                    {

                    }
                }

                context.State = null;
                context.Pop(member);
            }
        }

        public Type TargetType
        {
            get { return typeof(ITypeMember); }
        }

        public bool IsEnabled
        {
            get { return false; }
        }

        [TaskMethod(Weight = 1)]
        public async Task DeleteAsync(ITypeMember member, TaskContext context)
        {
            if (object.Equals(context.State, System.Data.DataRowState.Detached) == false)
            {
                await member.DeleteAsync(context.Authentication);
                context.State = System.Data.DataRowState.Deleted;
                context.Complete(member);
            }
        }

        [TaskMethod]
        public async Task SetIndexAsync(ITypeMember member, TaskContext context)
        {
            var index = RandomUtility.Next(member.Template.Count);
            await member.SetIndexAsync(context.Authentication, index);
        }

        [TaskMethod(Weight = 20)]
        public async Task SetNameAsync(ITypeMember member, TaskContext context)
        {
            var memberName = RandomUtility.NextIdentifier();
            await member.SetNameAsync(context.Authentication, memberName);
        }

        [TaskMethod]
        public async Task SetValueAsync(ITypeMember member, TaskContext context)
        {
            var template = member.Template;
            if (template.IsFlag == true)
            {
                var value = RandomUtility.NextBit();
                await member.SetValueAsync(context.Authentication, value);
            }
            else
            {
                var value = RandomUtility.NextLong(long.MaxValue);
                await member.SetValueAsync(context.Authentication, value);
            }
        }

        [TaskMethod]
        public async Task SetCommentAsync(ITypeMember member, TaskContext context)
        {
            var comment = RandomUtility.NextString();
            await member.SetCommentAsync(context.Authentication, comment);
        }
    }
}
