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
using JSSoft.Library.Random;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace JSSoft.Crema.Bot.Tasks
{
    [Export(typeof(ITaskProvider))]
    [Export(typeof(ITypeMemberTask))]
    [TaskClass]
    class ITypeMemberTask : ITaskProvider
    {
        public async Task InvokeAsync(TaskContext context)
        {
            var authentication = context.Authentication;
            var member = context.Target as ITypeMember;
            var template = member.Template;
            if (context.IsCompleted(member) == true)
            {
                if (object.Equals(context.State, System.Data.DataRowState.Detached) == true)
                {
                    try
                    {
                        await template.EndNewAsync(authentication, member);
                    }
                    catch
                    {

                    }
                }

                context.State = null;
                context.Pop(member);
            }
        }

        public Type TargetType => typeof(ITypeMember);

        public bool IsEnabled => false;

        [TaskMethod(Weight = 1)]
        public async Task DeleteAsync(ITypeMember member, TaskContext context)
        {
            var authentication = context.Authentication;
            if (object.Equals(context.State, System.Data.DataRowState.Detached) == false)
            {
                await member.DeleteAsync(authentication);
                context.State = System.Data.DataRowState.Deleted;
                context.Complete(member);
            }
        }

        [TaskMethod]
        public async Task SetIndexAsync(ITypeMember member, TaskContext context)
        {
            var authentication = context.Authentication;
            if (context.AllowException == false)
            {
                if (object.Equals(context.State, System.Data.DataRowState.Detached) == true)
                    return;
            }
            var index = RandomUtility.Next(member.Template.Count);
            await member.SetIndexAsync(authentication, index);
        }

        [TaskMethod(Weight = 20)]
        public async Task SetNameAsync(ITypeMember member, TaskContext context)
        {
            var authentication = context.Authentication;
            var memberName = RandomUtility.NextIdentifier();
            await member.SetNameAsync(authentication, memberName);
        }

        [TaskMethod]
        public async Task SetValueAsync(ITypeMember member, TaskContext context)
        {
            var authentication = context.Authentication;
            var template = member.Template;
            if (template.IsFlag == true)
            {
                var value = RandomUtility.NextBit();
                await member.SetValueAsync(authentication, value);
            }
            else
            {
                var value = RandomUtility.NextLong(long.MaxValue);
                await member.SetValueAsync(authentication, value);
            }
        }

        [TaskMethod]
        public async Task SetCommentAsync(ITypeMember member, TaskContext context)
        {
            var authentication = context.Authentication;
            var comment = RandomUtility.NextString();
            await member.SetCommentAsync(authentication, comment);
        }
    }
}
