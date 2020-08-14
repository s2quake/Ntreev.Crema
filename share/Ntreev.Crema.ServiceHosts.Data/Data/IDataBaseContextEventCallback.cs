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

using JSSoft.Communication;
using Ntreev.Crema.ServiceModel;
using System;

namespace Ntreev.Crema.ServiceHosts.Data
{
    public interface IDataBaseContextEventCallback
    {
        [OperationContract]
        void OnServiceClosed(CallbackInfo callbackInfo, CloseInfo closeInfo);

        [OperationContract]
        void OnDataBasesCreated(CallbackInfo callbackInfo, string[] dataBaseNames, DataBaseInfo[] dataBaseInfos, string comment);

        [OperationContract]
        void OnDataBasesRenamed(CallbackInfo callbackInfo, string[] dataBaseNames, string[] newDataBaseNames);

        [OperationContract]
        void OnDataBasesDeleted(CallbackInfo callbackInfo, string[] dataBaseNames);

        [OperationContract]
        void OnDataBasesLoaded(CallbackInfo callbackInfo, string[] dataBaseNames);

        [OperationContract]
        void OnDataBasesUnloaded(CallbackInfo callbackInfo, string[] dataBaseNames);

        [OperationContract]
        void OnDataBasesResetting(CallbackInfo callbackInfo, string[] dataBaseNames);

        [OperationContract]
        void OnDataBasesReset(CallbackInfo callbackInfo, string[] dataBaseNames, DataBaseMetaData[] metaDatas);

        [OperationContract]
        void OnDataBasesAuthenticationEntered(CallbackInfo callbackInfo, string[] dataBaseNames, AuthenticationInfo authenticationInfo);

        [OperationContract]
        void OnDataBasesAuthenticationLeft(CallbackInfo callbackInfo, string[] dataBaseNames, AuthenticationInfo authenticationInfo);

        [OperationContract]
        void OnDataBasesInfoChanged(CallbackInfo callbackInfo, DataBaseInfo[] dataBaseInfos);

        [OperationContract]
        void OnDataBasesStateChanged(CallbackInfo callbackInfo, string[] dataBaseNames, DataBaseState[] dataBaseStates);

        [OperationContract]
        void OnDataBasesAccessChanged(CallbackInfo callbackInfo, AccessChangeType changeType, AccessInfo[] accessInfos, string[] memberIDs, AccessType[] accessTypes);

        [OperationContract]
        void OnDataBasesLockChanged(CallbackInfo callbackInfo, LockChangeType changeType, LockInfo[] lockInfos, string[] comments);

        [OperationContract]
        void OnTaskCompleted(CallbackInfo callbackInfo, Guid[] taskIDs);
    }
}
