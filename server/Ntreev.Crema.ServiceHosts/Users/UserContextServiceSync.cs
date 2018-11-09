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

using Ntreev.Crema.ServiceModel;
using Ntreev.Library.ObjectModel;
using Ntreev.Library;
using Ntreev.Crema.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Security;
using System.Runtime.InteropServices;
using Ntreev.Crema.ServiceHosts.Properties;

namespace Ntreev.Crema.ServiceHosts.Users
{
    partial class UserContextService
    {
        public ResultBase<UserContextMetaData> Subscribe(Guid authenticationToken)
        {
            return this.InvokeTask(Task.Run(() =>  this.SubscribeAsync(authenticationToken)));
        }

        public ResultBase Unsubscribe()
        {
            return this.InvokeTask(Task.Run(() =>  this.UnsubscribeAsync()));
        }

        //public ResultBase Shutdown(int milliseconds, ShutdownType shutdownType, string message)
        //{
        //    return this.InvokeTask(Task.Run(() =>  this.ShutdownAsync(milliseconds, shutdownType, message)));
        //}

        //public ResultBase CancelShutdown()
        //{
        //    return this.InvokeTask(Task.Run(() =>  this.CancelShutdownAsync()));
        //}

        public ResultBase<UserInfo> NewUser(string userID, string categoryPath, byte[] password, string userName, Authority authority)
        {
            return this.InvokeTask(Task.Run(() =>  this.NewUserAsync(userID, categoryPath, password, userName, authority)));
        }

        public ResultBase NewUserCategory(string categoryPath)
        {
            return this.InvokeTask(Task.Run(() =>  this.NewUserCategoryAsync(categoryPath)));
        }

        public ResultBase RenameUserItem(string itemPath, string newName)
        {
            return this.InvokeTask(Task.Run(() =>  this.RenameUserItemAsync(itemPath, newName)));
        }

        public ResultBase MoveUserItem(string itemPath, string parentPath)
        {
            return this.InvokeTask(Task.Run(() =>  this.MoveUserItemAsync(itemPath, parentPath)));
        }

        public ResultBase DeleteUserItem(string itemPath)
        {
            return this.InvokeTask(Task.Run(() =>  this.DeleteUserItemAsync(itemPath)));
        }

        public ResultBase<UserInfo> ChangeUserInfo(string userID, byte[] password, byte[] newPassword, string userName, Authority? authority)
        {
            return this.InvokeTask(Task.Run(() =>  this.ChangeUserInfoAsync(userID, password, newPassword, userName, authority)));
        }

        public ResultBase Kick(string userID, string comment)
        {
            return this.InvokeTask(Task.Run(() =>  this.KickAsync(userID, comment)));
        }

        public ResultBase<BanInfo> Ban(string userID, string comment)
        {
            return this.InvokeTask(Task.Run(() =>  this.BanAsync(userID, comment)));
        }

        public ResultBase Unban(string userID)
        {
            return this.InvokeTask(Task.Run(() =>  this.UnbanAsync(userID)));
        }

        public ResultBase SendMessage(string userID, string message)
        {
            return this.InvokeTask(Task.Run(() =>  this.SendMessageAsync(userID, message)));
        }

        public ResultBase NotifyMessage(string[] userIDs, string message)
        {
            return this.InvokeTask(Task.Run(() =>  this.NotifyMessageAsync(userIDs, message)));
        }

        public bool IsAlive()
        {
            return this.InvokeTask(Task.Run(() =>  this.IsAliveAsync()));
        }

        private T InvokeTask<T>(Task<T> task)
        {
            task.Wait();
            return task.Result;
        }
    }
}
