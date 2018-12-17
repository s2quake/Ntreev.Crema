﻿//Released under the MIT License.
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

//------------------------------------------------------------------------------
// <auto-generated>
//     이 코드는 도구를 사용하여 생성되었습니다.
//     런타임 버전:4.0.30319.42000
//
//     파일 내용을 변경하면 잘못된 동작이 발생할 수 있으며, 코드를 다시 생성하면
//     이러한 변경 내용이 손실됩니다.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Ntreev.Crema.Services.DataBaseContextService {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="http://www.ntreev.com", ConfigurationName="DataBaseContextService.IDataBaseContextService", CallbackContract=typeof(Ntreev.Crema.Services.DataBaseContextService.IDataBaseContextServiceCallback), SessionMode=System.ServiceModel.SessionMode.Required)]
    internal interface IDataBaseContextService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/IDataBaseContextService/DefinitionType", ReplyAction="http://www.ntreev.com/IDataBaseContextService/DefinitionTypeResponse")]
        Ntreev.Crema.ServiceModel.ResultBase DefinitionType(Ntreev.Crema.ServiceModel.LogInfo[] param1);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/IDataBaseContextService/Subscribe", ReplyAction="http://www.ntreev.com/IDataBaseContextService/SubscribeResponse")]
        Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.DataBaseContextMetaData> Subscribe(System.Guid authenticationToken);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/IDataBaseContextService/Unsubscribe", ReplyAction="http://www.ntreev.com/IDataBaseContextService/UnsubscribeResponse")]
        Ntreev.Crema.ServiceModel.ResultBase Unsubscribe();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/IDataBaseContextService/GetDataSet", ReplyAction="http://www.ntreev.com/IDataBaseContextService/GetDataSetResponse")]
        Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.Data.CremaDataSet> GetDataSet(string dataBaseName, Ntreev.Crema.ServiceModel.DataSetType dataSetType, string filterExpression, string revision);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/IDataBaseContextService/SetPublic", ReplyAction="http://www.ntreev.com/IDataBaseContextService/SetPublicResponse")]
        Ntreev.Crema.ServiceModel.ResultBase SetPublic(string dataBaseName);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/IDataBaseContextService/SetPrivate", ReplyAction="http://www.ntreev.com/IDataBaseContextService/SetPrivateResponse")]
        Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.AccessInfo> SetPrivate(string dataBaseName);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/IDataBaseContextService/AddAccessMember", ReplyAction="http://www.ntreev.com/IDataBaseContextService/AddAccessMemberResponse")]
        Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.AccessMemberInfo> AddAccessMember(string dataBaseName, string memberID, Ntreev.Crema.ServiceModel.AccessType accessType);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/IDataBaseContextService/SetAccessMember", ReplyAction="http://www.ntreev.com/IDataBaseContextService/SetAccessMemberResponse")]
        Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.AccessMemberInfo> SetAccessMember(string dataBaseName, string memberID, Ntreev.Crema.ServiceModel.AccessType accessType);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/IDataBaseContextService/RemoveAccessMember", ReplyAction="http://www.ntreev.com/IDataBaseContextService/RemoveAccessMemberResponse")]
        Ntreev.Crema.ServiceModel.ResultBase RemoveAccessMember(string dataBaseName, string memberID);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/IDataBaseContextService/Lock", ReplyAction="http://www.ntreev.com/IDataBaseContextService/LockResponse")]
        Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.LockInfo> Lock(string dataBaseName, string comment);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/IDataBaseContextService/Unlock", ReplyAction="http://www.ntreev.com/IDataBaseContextService/UnlockResponse")]
        Ntreev.Crema.ServiceModel.ResultBase Unlock(string dataBaseName);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/IDataBaseContextService/Load", ReplyAction="http://www.ntreev.com/IDataBaseContextService/LoadResponse")]
        Ntreev.Crema.ServiceModel.ResultBase Load(string dataBaseName);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/IDataBaseContextService/Unload", ReplyAction="http://www.ntreev.com/IDataBaseContextService/UnloadResponse")]
        Ntreev.Crema.ServiceModel.ResultBase Unload(string dataBaseName);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/IDataBaseContextService/Create", ReplyAction="http://www.ntreev.com/IDataBaseContextService/CreateResponse")]
        Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.DataBaseInfo> Create(string dataBaseName, string comment);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/IDataBaseContextService/Copy", ReplyAction="http://www.ntreev.com/IDataBaseContextService/CopyResponse")]
        Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.DataBaseInfo> Copy(string dataBaseName, string newDataBaseName, string comment, bool force);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/IDataBaseContextService/Rename", ReplyAction="http://www.ntreev.com/IDataBaseContextService/RenameResponse")]
        Ntreev.Crema.ServiceModel.ResultBase Rename(string dataBaseName, string newDataBaseName);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/IDataBaseContextService/Delete", ReplyAction="http://www.ntreev.com/IDataBaseContextService/DeleteResponse")]
        Ntreev.Crema.ServiceModel.ResultBase Delete(string dataBaseName);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/IDataBaseContextService/GetLog", ReplyAction="http://www.ntreev.com/IDataBaseContextService/GetLogResponse")]
        Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.LogInfo[]> GetLog(string dataBaseName, string revision);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/IDataBaseContextService/Revert", ReplyAction="http://www.ntreev.com/IDataBaseContextService/RevertResponse")]
        Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.DataBaseInfo> Revert(string dataBaseName, string revision);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/IDataBaseContextService/BeginTransaction", ReplyAction="http://www.ntreev.com/IDataBaseContextService/BeginTransactionResponse")]
        Ntreev.Crema.ServiceModel.ResultBase<System.Guid> BeginTransaction(string dataBaseName);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/IDataBaseContextService/EndTransaction", ReplyAction="http://www.ntreev.com/IDataBaseContextService/EndTransactionResponse")]
        Ntreev.Crema.ServiceModel.ResultBase EndTransaction(System.Guid transactionID);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/IDataBaseContextService/CancelTransaction", ReplyAction="http://www.ntreev.com/IDataBaseContextService/CancelTransactionResponse")]
        Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.DataBaseMetaData> CancelTransaction(System.Guid transactionID);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/IDataBaseContextService/IsAlive", ReplyAction="http://www.ntreev.com/IDataBaseContextService/IsAliveResponse")]
        bool IsAlive();
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    internal interface IDataBaseContextServiceCallback {
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://www.ntreev.com/IDataBaseContextService/OnServiceClosed")]
        void OnServiceClosed(Ntreev.Crema.ServiceModel.CallbackInfo callbackInfo, Ntreev.Crema.ServiceModel.CloseInfo closeInfo);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://www.ntreev.com/IDataBaseContextService/OnDataBasesCreated")]
        void OnDataBasesCreated(Ntreev.Crema.ServiceModel.CallbackInfo callbackInfo, string[] dataBaseNames, Ntreev.Crema.ServiceModel.DataBaseInfo[] dataBaseInfos, string comment);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://www.ntreev.com/IDataBaseContextService/OnDataBasesRenamed")]
        void OnDataBasesRenamed(Ntreev.Crema.ServiceModel.CallbackInfo callbackInfo, string[] dataBaseNames, string[] newDataBaseNames);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://www.ntreev.com/IDataBaseContextService/OnDataBasesDeleted")]
        void OnDataBasesDeleted(Ntreev.Crema.ServiceModel.CallbackInfo callbackInfo, string[] dataBaseNames);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://www.ntreev.com/IDataBaseContextService/OnDataBasesLoaded")]
        void OnDataBasesLoaded(Ntreev.Crema.ServiceModel.CallbackInfo callbackInfo, string[] dataBaseNames);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://www.ntreev.com/IDataBaseContextService/OnDataBasesUnloaded")]
        void OnDataBasesUnloaded(Ntreev.Crema.ServiceModel.CallbackInfo callbackInfo, string[] dataBaseNames);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://www.ntreev.com/IDataBaseContextService/OnDataBasesResetting")]
        void OnDataBasesResetting(Ntreev.Crema.ServiceModel.CallbackInfo callbackInfo, string[] dataBaseNames);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://www.ntreev.com/IDataBaseContextService/OnDataBasesReset")]
        void OnDataBasesReset(Ntreev.Crema.ServiceModel.CallbackInfo callbackInfo, string[] dataBaseNames, Ntreev.Crema.ServiceModel.DataBaseMetaData[] metaDatas);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://www.ntreev.com/IDataBaseContextService/OnDataBasesAuthenticationEntered")]
        void OnDataBasesAuthenticationEntered(Ntreev.Crema.ServiceModel.CallbackInfo callbackInfo, string[] dataBaseNames, Ntreev.Crema.ServiceModel.AuthenticationInfo authenticationInfo);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://www.ntreev.com/IDataBaseContextService/OnDataBasesAuthenticationLeft")]
        void OnDataBasesAuthenticationLeft(Ntreev.Crema.ServiceModel.CallbackInfo callbackInfo, string[] dataBaseNames, Ntreev.Crema.ServiceModel.AuthenticationInfo authenticationInfo);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://www.ntreev.com/IDataBaseContextService/OnDataBasesInfoChanged")]
        void OnDataBasesInfoChanged(Ntreev.Crema.ServiceModel.CallbackInfo callbackInfo, Ntreev.Crema.ServiceModel.DataBaseInfo[] dataBaseInfos);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://www.ntreev.com/IDataBaseContextService/OnDataBasesStateChanged")]
        void OnDataBasesStateChanged(Ntreev.Crema.ServiceModel.CallbackInfo callbackInfo, string[] dataBaseNames, Ntreev.Crema.ServiceModel.DataBaseState[] dataBaseStates);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://www.ntreev.com/IDataBaseContextService/OnDataBasesAccessChanged")]
        void OnDataBasesAccessChanged(Ntreev.Crema.ServiceModel.CallbackInfo callbackInfo, Ntreev.Crema.ServiceModel.AccessChangeType changeType, Ntreev.Crema.ServiceModel.AccessInfo[] accessInfos, string[] memberIDs, Ntreev.Crema.ServiceModel.AccessType[] accessTypes);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://www.ntreev.com/IDataBaseContextService/OnDataBasesLockChanged")]
        void OnDataBasesLockChanged(Ntreev.Crema.ServiceModel.CallbackInfo callbackInfo, Ntreev.Crema.ServiceModel.LockChangeType changeType, Ntreev.Crema.ServiceModel.LockInfo[] lockInfos, string[] comments);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://www.ntreev.com/IDataBaseContextService/OnTaskCompleted")]
        void OnTaskCompleted(Ntreev.Crema.ServiceModel.CallbackInfo callbackInfo, System.Guid[] taskIDs);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    internal interface IDataBaseContextServiceChannel : Ntreev.Crema.Services.DataBaseContextService.IDataBaseContextService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    internal partial class DataBaseContextServiceClient : System.ServiceModel.DuplexClientBase<Ntreev.Crema.Services.DataBaseContextService.IDataBaseContextService>, Ntreev.Crema.Services.DataBaseContextService.IDataBaseContextService {
        
        public DataBaseContextServiceClient(System.ServiceModel.InstanceContext callbackInstance) : 
                base(callbackInstance) {
        }
        
        public DataBaseContextServiceClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName) : 
                base(callbackInstance, endpointConfigurationName) {
        }
        
        public DataBaseContextServiceClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, string remoteAddress) : 
                base(callbackInstance, endpointConfigurationName, remoteAddress) {
        }
        
        public DataBaseContextServiceClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(callbackInstance, endpointConfigurationName, remoteAddress) {
        }
        
        public DataBaseContextServiceClient(System.ServiceModel.InstanceContext callbackInstance, System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(callbackInstance, binding, remoteAddress) {
        }
        
        public Ntreev.Crema.ServiceModel.ResultBase DefinitionType(Ntreev.Crema.ServiceModel.LogInfo[] param1) {
            return base.Channel.DefinitionType(param1);
        }
        
        public Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.DataBaseContextMetaData> Subscribe(System.Guid authenticationToken) {
            return base.Channel.Subscribe(authenticationToken);
        }
        
        public Ntreev.Crema.ServiceModel.ResultBase Unsubscribe() {
            return base.Channel.Unsubscribe();
        }
        
        public Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.Data.CremaDataSet> GetDataSet(string dataBaseName, Ntreev.Crema.ServiceModel.DataSetType dataSetType, string filterExpression, string revision) {
            return base.Channel.GetDataSet(dataBaseName, dataSetType, filterExpression, revision);
        }
        
        public Ntreev.Crema.ServiceModel.ResultBase SetPublic(string dataBaseName) {
            return base.Channel.SetPublic(dataBaseName);
        }
        
        public Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.AccessInfo> SetPrivate(string dataBaseName) {
            return base.Channel.SetPrivate(dataBaseName);
        }
        
        public Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.AccessMemberInfo> AddAccessMember(string dataBaseName, string memberID, Ntreev.Crema.ServiceModel.AccessType accessType) {
            return base.Channel.AddAccessMember(dataBaseName, memberID, accessType);
        }
        
        public Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.AccessMemberInfo> SetAccessMember(string dataBaseName, string memberID, Ntreev.Crema.ServiceModel.AccessType accessType) {
            return base.Channel.SetAccessMember(dataBaseName, memberID, accessType);
        }
        
        public Ntreev.Crema.ServiceModel.ResultBase RemoveAccessMember(string dataBaseName, string memberID) {
            return base.Channel.RemoveAccessMember(dataBaseName, memberID);
        }
        
        public Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.LockInfo> Lock(string dataBaseName, string comment) {
            return base.Channel.Lock(dataBaseName, comment);
        }
        
        public Ntreev.Crema.ServiceModel.ResultBase Unlock(string dataBaseName) {
            return base.Channel.Unlock(dataBaseName);
        }
        
        public Ntreev.Crema.ServiceModel.ResultBase Load(string dataBaseName) {
            return base.Channel.Load(dataBaseName);
        }
        
        public Ntreev.Crema.ServiceModel.ResultBase Unload(string dataBaseName) {
            return base.Channel.Unload(dataBaseName);
        }
        
        public Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.DataBaseInfo> Create(string dataBaseName, string comment) {
            return base.Channel.Create(dataBaseName, comment);
        }
        
        public Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.DataBaseInfo> Copy(string dataBaseName, string newDataBaseName, string comment, bool force) {
            return base.Channel.Copy(dataBaseName, newDataBaseName, comment, force);
        }
        
        public Ntreev.Crema.ServiceModel.ResultBase Rename(string dataBaseName, string newDataBaseName) {
            return base.Channel.Rename(dataBaseName, newDataBaseName);
        }
        
        public Ntreev.Crema.ServiceModel.ResultBase Delete(string dataBaseName) {
            return base.Channel.Delete(dataBaseName);
        }
        
        public Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.LogInfo[]> GetLog(string dataBaseName, string revision) {
            return base.Channel.GetLog(dataBaseName, revision);
        }
        
        public Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.DataBaseInfo> Revert(string dataBaseName, string revision) {
            return base.Channel.Revert(dataBaseName, revision);
        }
        
        public Ntreev.Crema.ServiceModel.ResultBase<System.Guid> BeginTransaction(string dataBaseName) {
            return base.Channel.BeginTransaction(dataBaseName);
        }
        
        public Ntreev.Crema.ServiceModel.ResultBase EndTransaction(System.Guid transactionID) {
            return base.Channel.EndTransaction(transactionID);
        }
        
        public Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.DataBaseMetaData> CancelTransaction(System.Guid transactionID) {
            return base.Channel.CancelTransaction(transactionID);
        }
        
        public bool IsAlive() {
            return base.Channel.IsAlive();
        }
    }
}
