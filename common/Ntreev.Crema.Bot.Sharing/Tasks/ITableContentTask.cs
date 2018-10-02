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
    [Export(typeof(ITableContentTask))]
    [TaskClass]
    class ITableContentTask : ITaskProvider
    {
        public async Task InvokeAsync(TaskContext context)
        {
            var authentication = context.Authentication;
            var content = context.Target as ITableContent;
            if (context.IsCompleted(content) == true)
            {
                try
                {

                }
                catch
                {

                }
                var editableState = await content.Dispatcher.InvokeAsync(() => content.ServiceState);
                if (editableState == ServiceState.None)
                {
                    context.Pop(content);
                    context.Complete(context.Target);
                    return;
                }
                else if (editableState != ServiceState.Opened)
                {
                    return;
                }

                var domain = content.Domain;
                if (domain != null && RandomUtility.Within(50) == true)
                {
                    if (await domain.Users.ContainsAsync(context.Authentication.ID) == false)
                        return;
                    var userState = await domain.Dispatcher.InvokeAsync(() => domain.Users[authentication.ID].DomainUserState);
                    var isMember = userState.HasFlag(DomainUserState.Online);
                    if (isMember == true)
                    {
                        await content.LeaveEditAsync(authentication);
                    }
                    if (await domain.Dispatcher.InvokeAsync(() => domain.Users.Any()) == false)
                    {
                        await content.EndEditAsync(authentication);
                    }
                }
                context.Pop(content);
                context.Complete(context.Target);
            }
            else
            {
                var editableState = await content.Dispatcher.InvokeAsync(() => content.ServiceState);
                if (editableState == ServiceState.None)
                {
                    await content.BeginEditAsync(authentication);
                }

                var domain = content.Domain;

                if (await domain.Users.ContainsAsync(authentication.ID) == false)
                {
                    await content.EnterEditAsync(authentication);
                }
                else
                {
                    var userState = await domain.Dispatcher.InvokeAsync(() => domain.Users[authentication.ID].DomainUserState);
                    if (userState.HasFlag(DomainUserState.Online) == false)
                    {
                        await content.EndEditAsync(authentication);
                    }
                }

                if (RandomUtility.Within(25) == true || await content.Dispatcher.InvokeAsync(() => content.Any()) == false)
                {
                    var row = await this.AddNewRowAsync(context.Authentication, content);
                    if (row != null)
                    {
                        await row.InitializeRandomAsync(context.Authentication);
                        context.Push(row);
                        context.State = System.Data.DataRowState.Detached;
                    }
                }
                else
                {
                    var member = await content.Dispatcher.InvokeAsync(() => content.Random());
                    context.Push(member);
                }
            }
        }

        private async Task<ITableRow> AddNewRowAsync(Authentication authentication, ITableContent content)
        {
            var table = content.Table;
            var parent = table.Parent;
            if (parent != null)
            {
                if (parent.Content.Any() == false)
                    return null;
                var relationID = parent.Content.Random().RelationID;
                return await content.AddNewAsync(authentication, relationID);
            }
            return await content.AddNewAsync(authentication, null);
        }

        public Type TargetType
        {
            get { return typeof(ITableContent); }
        }

        public bool IsEnabled
        {
            get { return false; }
        }
    }
}
