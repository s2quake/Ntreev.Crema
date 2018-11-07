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
using Ntreev.Crema.Services.DataBaseContextService;
using Ntreev.Library;
using System;
using System.ServiceModel;
using System.Threading;

namespace Ntreev.Crema.Services.Data
{
    class DataBaseContextServiceFactory : IDataBaseContextServiceCallback
    {
        private static readonly DataBaseContextServiceFactory empty = new DataBaseContextServiceFactory();

        private DataBaseContextServiceFactory()
        {

        }

        public static DataBaseContextServiceClient CreateServiceClient(string address, ServiceInfo serviceInfo, IDataBaseContextServiceCallback callback)
        {
            var binding = CremaHost.CreateBinding(serviceInfo);

            var endPointAddress = new EndpointAddress($"net.tcp://{address}:{serviceInfo.Port}/DataBaseContextService");
            var instanceContext = new InstanceContext(callback ?? empty);
            //if (Environment.OSVersion.Platform != PlatformID.Unix)
            //    instanceContext.SynchronizationContext = SynchronizationContext.Current;

            return new DataBaseContextServiceClient(instanceContext, binding, endPointAddress);
        }

        #region IDataBaseContextServiceCallback

        void IDataBaseContextServiceCallback.OnServiceClosed(CallbackInfo callbackInfo, CloseInfo closeInfo)
        {
            throw new NotImplementedException();
        }

        void IDataBaseContextServiceCallback.OnDataBasesCreated(CallbackInfo callbackInfo, string[] dataBaseNames, DataBaseInfo[] dataBaseInfos, string comment)
        {
            throw new NotImplementedException();
        }

        void IDataBaseContextServiceCallback.OnDataBasesDeleted(CallbackInfo callbackInfo, string[] dataBaseNames)
        {
            throw new NotImplementedException();
        }

        void IDataBaseContextServiceCallback.OnDataBasesRenamed(CallbackInfo callbackInfo, string[] dataBaseNames, string[] newDataBaseNames)
        {
            throw new NotImplementedException();
        }

        void IDataBaseContextServiceCallback.OnDataBasesLoaded(CallbackInfo callbackInfo, string[] dataBaseNames)
        {
            throw new NotImplementedException();
        }

        void IDataBaseContextServiceCallback.OnDataBasesUnloaded(CallbackInfo callbackInfo, string[] dataBaseNames)
        {
            throw new NotImplementedException();
        }

        void IDataBaseContextServiceCallback.OnDataBasesResetting(CallbackInfo callbackInfo, string[] dataBaseNames)
        {
            throw new NotImplementedException();
        }

        void IDataBaseContextServiceCallback.OnDataBasesReset(CallbackInfo callbackInfo, string[] dataBaseNames, DataBaseMetaData[] metaDatas)
        {
            throw new NotImplementedException();
        }

        public void OnDataBasesAuthenticationEntered(CallbackInfo callbackInfo, string[] dataBaseNames, AuthenticationInfo authenticationInfo)
        {
            throw new NotImplementedException();
        }

        public void OnDataBasesAuthenticationLeft(CallbackInfo callbackInfo, string[] dataBaseNames, AuthenticationInfo authenticationInfo)
        {
            throw new NotImplementedException();
        }

        void IDataBaseContextServiceCallback.OnDataBasesInfoChanged(CallbackInfo callbackInfo, DataBaseInfo[] dataBaseInfos)
        {
            throw new NotImplementedException();
        }

        void IDataBaseContextServiceCallback.OnDataBasesStateChanged(CallbackInfo callbackInfo, string[] dataBaseNames, DataBaseState[] dataBaseStates)
        {
            throw new NotImplementedException();
        }

        void IDataBaseContextServiceCallback.OnDataBasesAccessChanged(CallbackInfo callbackInfo, AccessChangeType changeType, AccessInfo[] accessInfos, string[] memberIDs, AccessType[] accessTypes)
        {
            throw new NotImplementedException();
        }

        void IDataBaseContextServiceCallback.OnDataBasesLockChanged(CallbackInfo callbackInfo, LockChangeType changeType, LockInfo[] lockInfos, string[] comments)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}