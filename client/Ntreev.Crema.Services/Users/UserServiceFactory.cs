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
using Ntreev.Crema.Services.UserContextService;
using Ntreev.Library;
using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;

namespace Ntreev.Crema.Services.Users
{
    class UserContextServiceFactory : IUserContextServiceCallback
    {
        private readonly EndpointAddress endPointAddress;
        private readonly Binding binding;
        private readonly InstanceContext instanceContext;

        private UserContextServiceFactory(string address, ServiceInfo serviceInfo, IUserContextServiceCallback userServiceCallback)
        {
            this.binding = CremaHost.CreateBinding(serviceInfo);
            this.endPointAddress = new EndpointAddress($"net.tcp://{address}:{serviceInfo.Port}/UserContextService");
            this.instanceContext = new InstanceContext(userServiceCallback ?? (this));
            //if (Environment.OSVersion.Platform != PlatformID.Unix)
            //    this.instanceContext.SynchronizationContext = SynchronizationContext.Current;
        }

        public static UserContextServiceClient CreateServiceClient(string address, ServiceInfo serviceInfo, IUserContextServiceCallback userServiceCallback)
        {
            var factory = new UserContextServiceFactory(address, serviceInfo, userServiceCallback);
            return new UserContextServiceClient(factory.instanceContext, factory.binding, factory.endPointAddress);
        }

        #region IUserContextServiceCallback

        void IUserContextServiceCallback.OnServiceClosed(CallbackInfo callbackInfo, CloseInfo closeInfo)
        {
            throw new NotImplementedException();
        }

        void IUserContextServiceCallback.OnUsersChanged(CallbackInfo callbackInfo, UserInfo[] userInfos)
        {
            throw new NotImplementedException();
        }

        void IUserContextServiceCallback.OnUsersStateChanged(CallbackInfo callbackInfo, string[] userIDs, UserState[] states)
        {
            throw new NotImplementedException();
        }

        void IUserContextServiceCallback.OnUserItemsCreated(CallbackInfo callbackInfo, string[] itemPaths, UserInfo?[] args)
        {
            throw new NotImplementedException();
        }

        void IUserContextServiceCallback.OnUserItemsRenamed(CallbackInfo callbackInfo, string[] itemPaths, string[] newNames)
        {
            throw new NotImplementedException();
        }

        void IUserContextServiceCallback.OnUserItemsMoved(CallbackInfo callbackInfo, string[] itemPaths, string[] parentPaths)
        {
            throw new NotImplementedException();
        }

        void IUserContextServiceCallback.OnUserItemsDeleted(CallbackInfo callbackInfo, string[] itemPaths)
        {
            throw new NotImplementedException();
        }

        void IUserContextServiceCallback.OnUsersLoggedIn(CallbackInfo callbackInfo, string[] userIDs)
        {
            throw new NotImplementedException();
        }

        void IUserContextServiceCallback.OnUsersLoggedOut(CallbackInfo callbackInfo, string[] userIDs, CloseInfo closeInfo)
        {
            throw new NotImplementedException();
        }

        void IUserContextServiceCallback.OnUsersKicked(CallbackInfo callbackInfo, string[] userIDs, string[] comments)
        {
            throw new NotImplementedException();
        }

        void IUserContextServiceCallback.OnUsersBanChanged(CallbackInfo callbackInfo, BanInfo[] banInfos, BanChangeType changeType, string[] comments)
        {
            throw new NotImplementedException();
        }

        void IUserContextServiceCallback.OnMessageReceived(CallbackInfo callbackInfo, string[] userIDs, string message, MessageType messageType)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
