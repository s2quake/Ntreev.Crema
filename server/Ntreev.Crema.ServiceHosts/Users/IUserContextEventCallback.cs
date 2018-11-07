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
using System.Linq;
using System.Text;
using System.ServiceModel;
using Ntreev.Crema.Services;
using Ntreev.Crema.ServiceModel;
using System.Threading.Tasks;
using Ntreev.Library;

namespace Ntreev.Crema.ServiceHosts.Users
{
    public interface IUserContextEventCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnServiceClosed(CallbackInfo callbackInfo, CloseInfo closeInfo);

        [OperationContract(IsOneWay = true)]
        void OnUsersChanged(CallbackInfo callbackInfo, UserInfo[] userInfos);

        [OperationContract(IsOneWay = true)]
        void OnUsersStateChanged(CallbackInfo callbackInfo, string[] userIDs, UserState[] states);

        [OperationContract(IsOneWay = true)]
        void OnUserItemsCreated(CallbackInfo callbackInfo, string[] itemPaths, UserInfo?[] args);

        [OperationContract(IsOneWay = true)]
        void OnUserItemsRenamed(CallbackInfo callbackInfo, string[] itemPaths, string[] newNames);

        [OperationContract(IsOneWay = true)]
        void OnUserItemsMoved(CallbackInfo callbackInfo, string[] itemPaths, string[] parentPaths);

        [OperationContract(IsOneWay = true)]
        void OnUserItemsDeleted(CallbackInfo callbackInfo, string[] itemPaths);

        [OperationContract(IsOneWay = true)]
        void OnUsersLoggedIn(CallbackInfo callbackInfo, string[] userIDs);

        [OperationContract(IsOneWay = true)]
        void OnUsersLoggedOut(CallbackInfo callbackInfo, string[] userIDs, CloseInfo closeInfo);

        [OperationContract(IsOneWay = true)]
        void OnUsersKicked(CallbackInfo callbackInfo, string[] userIDs, string[] comments);

        [OperationContract(IsOneWay = true)]
        void OnUsersBanChanged(CallbackInfo callbackInfo, BanInfo[] banInfos, BanChangeType changeType, string[] comments);

        [OperationContract(IsOneWay = true)]
        void OnMessageReceived(CallbackInfo callbackInfo, string[] userIDs, string message, MessageType messageType);
    }
}
