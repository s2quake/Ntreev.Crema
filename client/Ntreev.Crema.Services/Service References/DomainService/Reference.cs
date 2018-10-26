﻿//------------------------------------------------------------------------------
// <auto-generated>
//     이 코드는 도구를 사용하여 생성되었습니다.
//     런타임 버전:4.0.30319.42000
//
//     파일 내용을 변경하면 잘못된 동작이 발생할 수 있으며, 코드를 다시 생성하면
//     이러한 변경 내용이 손실됩니다.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Ntreev.Crema.Services.DomainService {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="http://www.ntreev.com", ConfigurationName="DomainService.IDomainService", CallbackContract=typeof(Ntreev.Crema.Services.DomainService.IDomainServiceCallback), SessionMode=System.ServiceModel.SessionMode.Required)]
    internal interface IDomainService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/IDomainService/Subscribe", ReplyAction="http://www.ntreev.com/IDomainService/SubscribeResponse")]
        Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.DomainContextMetaData> Subscribe(System.Guid authenticationToken);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/IDomainService/Unsubscribe", ReplyAction="http://www.ntreev.com/IDomainService/UnsubscribeResponse")]
        Ntreev.Crema.ServiceModel.ResultBase Unsubscribe();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/IDomainService/GetMetaData", ReplyAction="http://www.ntreev.com/IDomainService/GetMetaDataResponse")]
        Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.DomainMetaData[]> GetMetaData(System.Guid dataBaseID);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/IDomainService/SetUserLocation", ReplyAction="http://www.ntreev.com/IDomainService/SetUserLocationResponse")]
        Ntreev.Crema.ServiceModel.ResultBase SetUserLocation(System.Guid domainID, Ntreev.Crema.ServiceModel.DomainLocationInfo location);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/IDomainService/NewRow", ReplyAction="http://www.ntreev.com/IDomainService/NewRowResponse")]
        Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.DomainRowResultInfo> NewRow(System.Guid domainID, Ntreev.Crema.ServiceModel.DomainRowInfo[] rows);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/IDomainService/RemoveRow", ReplyAction="http://www.ntreev.com/IDomainService/RemoveRowResponse")]
        Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.DomainRowResultInfo> RemoveRow(System.Guid domainID, Ntreev.Crema.ServiceModel.DomainRowInfo[] rows);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/IDomainService/SetRow", ReplyAction="http://www.ntreev.com/IDomainService/SetRowResponse")]
        Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.DomainRowResultInfo> SetRow(System.Guid domainID, Ntreev.Crema.ServiceModel.DomainRowInfo[] rows);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/IDomainService/SetProperty", ReplyAction="http://www.ntreev.com/IDomainService/SetPropertyResponse")]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(System.DBNull))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.DomainContextMetaData>))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.CremaFault))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Library.SignatureDate))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainContextMetaData))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainMetaData[]))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainMetaData))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainState))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainUserMetaData[]))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainUserMetaData))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainUserInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainAccessType))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainLocationInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainFieldInfo[]))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainFieldInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainUserState))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.ResultBase))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.DomainMetaData[]>))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainRowInfo[]))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainRowInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.DomainRowResultInfo>))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainRowResultInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.DomainUserInfo>))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.Data.TableInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.Data.ColumnInfo[]))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.Data.ColumnInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Library.TagInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.Data.TableInfo[]))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.Data.TypeInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.Data.TypeMemberInfo[]))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.Data.TypeMemberInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.Data.TypeInfo[]))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.ResultBase<object>))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.CloseInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.CloseReason))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.RemoveInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.RemoveReason))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(string[]))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(System.Guid[]))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(bool[]))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(object[]))]
        Ntreev.Crema.ServiceModel.ResultBase SetProperty(System.Guid domainID, string propertyName, object value);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/IDomainService/BeginUserEdit", ReplyAction="http://www.ntreev.com/IDomainService/BeginUserEditResponse")]
        Ntreev.Crema.ServiceModel.ResultBase BeginUserEdit(System.Guid domainID, Ntreev.Crema.ServiceModel.DomainLocationInfo location);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/IDomainService/EndUserEdit", ReplyAction="http://www.ntreev.com/IDomainService/EndUserEditResponse")]
        Ntreev.Crema.ServiceModel.ResultBase EndUserEdit(System.Guid domainID);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/IDomainService/Kick", ReplyAction="http://www.ntreev.com/IDomainService/KickResponse")]
        Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.DomainUserInfo> Kick(System.Guid domainID, string userID, string comment);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/IDomainService/SetOwner", ReplyAction="http://www.ntreev.com/IDomainService/SetOwnerResponse")]
        Ntreev.Crema.ServiceModel.ResultBase SetOwner(System.Guid domainID, string userID);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/IDomainService/DeleteDomain", ReplyAction="http://www.ntreev.com/IDomainService/DeleteDomainResponse")]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(System.DBNull))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.DomainContextMetaData>))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.CremaFault))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Library.SignatureDate))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainContextMetaData))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainMetaData[]))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainMetaData))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainState))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainUserMetaData[]))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainUserMetaData))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainUserInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainAccessType))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainLocationInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainFieldInfo[]))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainFieldInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainUserState))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.ResultBase))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.DomainMetaData[]>))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainRowInfo[]))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainRowInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.DomainRowResultInfo>))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainRowResultInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.DomainUserInfo>))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.Data.TableInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.Data.ColumnInfo[]))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.Data.ColumnInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Library.TagInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.Data.TableInfo[]))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.Data.TypeInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.Data.TypeMemberInfo[]))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.Data.TypeMemberInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.Data.TypeInfo[]))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.CloseInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.CloseReason))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.RemoveInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.RemoveReason))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(string[]))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(System.Guid[]))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(bool[]))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(object[]))]
        Ntreev.Crema.ServiceModel.ResultBase<object> DeleteDomain(System.Guid domainID, bool force);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/IDomainService/IsAlive", ReplyAction="http://www.ntreev.com/IDomainService/IsAliveResponse")]
        bool IsAlive();
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    internal interface IDomainServiceCallback {
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://www.ntreev.com/IDomainService/OnServiceClosed")]
        void OnServiceClosed(Ntreev.Library.SignatureDate signatureDate, Ntreev.Crema.ServiceModel.CloseInfo closeInfo);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://www.ntreev.com/IDomainService/OnDomainsCreated")]
        void OnDomainsCreated(Ntreev.Library.SignatureDate signatureDate, Ntreev.Crema.ServiceModel.DomainMetaData[] metaDatas);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://www.ntreev.com/IDomainService/OnDomainsDeleted")]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(System.DBNull))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.DomainContextMetaData>))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.CremaFault))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Library.SignatureDate))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainContextMetaData))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainMetaData[]))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainMetaData))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainState))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainUserMetaData[]))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainUserMetaData))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainUserInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainAccessType))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainLocationInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainFieldInfo[]))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainFieldInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainUserState))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.ResultBase))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.DomainMetaData[]>))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainRowInfo[]))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainRowInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.DomainRowResultInfo>))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainRowResultInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.DomainUserInfo>))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.Data.TableInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.Data.ColumnInfo[]))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.Data.ColumnInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Library.TagInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.Data.TableInfo[]))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.Data.TypeInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.Data.TypeMemberInfo[]))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.Data.TypeMemberInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.Data.TypeInfo[]))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.ResultBase<object>))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.CloseInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.CloseReason))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.RemoveInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.RemoveReason))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(string[]))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(System.Guid[]))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(bool[]))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(object[]))]
        void OnDomainsDeleted(Ntreev.Library.SignatureDate signatureDate, System.Guid[] domainIDs, bool[] IsCanceleds, object[] results);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://www.ntreev.com/IDomainService/OnDomainInfoChanged")]
        void OnDomainInfoChanged(Ntreev.Library.SignatureDate signatureDate, System.Guid domainID, Ntreev.Crema.ServiceModel.DomainInfo domainInfo);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://www.ntreev.com/IDomainService/OnDomainStateChanged")]
        void OnDomainStateChanged(Ntreev.Library.SignatureDate signatureDate, System.Guid domainID, Ntreev.Crema.ServiceModel.DomainState domainState);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://www.ntreev.com/IDomainService/OnUserAdded")]
        void OnUserAdded(Ntreev.Library.SignatureDate signatureDate, System.Guid domainID, Ntreev.Crema.ServiceModel.DomainUserInfo domainUserInfo, Ntreev.Crema.ServiceModel.DomainUserState domainUserState);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://www.ntreev.com/IDomainService/OnUserChanged")]
        void OnUserChanged(Ntreev.Library.SignatureDate signatureDate, System.Guid domainID, Ntreev.Crema.ServiceModel.DomainUserInfo domainUserInfo, Ntreev.Crema.ServiceModel.DomainUserState domainUserState);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://www.ntreev.com/IDomainService/OnUserRemoved")]
        void OnUserRemoved(Ntreev.Library.SignatureDate signatureDate, System.Guid domainID, Ntreev.Crema.ServiceModel.DomainUserInfo domainUserInfo, Ntreev.Crema.ServiceModel.RemoveInfo removeInfo);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://www.ntreev.com/IDomainService/OnRowAdded")]
        void OnRowAdded(Ntreev.Library.SignatureDate signatureDate, System.Guid domainID, Ntreev.Crema.ServiceModel.DomainRowResultInfo info);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://www.ntreev.com/IDomainService/OnRowChanged")]
        void OnRowChanged(Ntreev.Library.SignatureDate signatureDate, System.Guid domainID, Ntreev.Crema.ServiceModel.DomainRowResultInfo info);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://www.ntreev.com/IDomainService/OnRowRemoved")]
        void OnRowRemoved(Ntreev.Library.SignatureDate signatureDate, System.Guid domainID, Ntreev.Crema.ServiceModel.DomainRowResultInfo info);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://www.ntreev.com/IDomainService/OnPropertyChanged")]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(System.DBNull))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.DomainContextMetaData>))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.CremaFault))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Library.SignatureDate))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainContextMetaData))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainMetaData[]))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainMetaData))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainState))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainUserMetaData[]))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainUserMetaData))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainUserInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainAccessType))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainLocationInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainFieldInfo[]))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainFieldInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainUserState))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.ResultBase))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.DomainMetaData[]>))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainRowInfo[]))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainRowInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.DomainRowResultInfo>))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.DomainRowResultInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.DomainUserInfo>))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.Data.TableInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.Data.ColumnInfo[]))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.Data.ColumnInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Library.TagInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.Data.TableInfo[]))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.Data.TypeInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.Data.TypeMemberInfo[]))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.Data.TypeMemberInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.Data.TypeInfo[]))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.ResultBase<object>))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.CloseInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.CloseReason))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.RemoveInfo))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Ntreev.Crema.ServiceModel.RemoveReason))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(string[]))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(System.Guid[]))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(bool[]))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(object[]))]
        void OnPropertyChanged(Ntreev.Library.SignatureDate signatureDate, System.Guid domainID, string propertyName, object value);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    internal interface IDomainServiceChannel : Ntreev.Crema.Services.DomainService.IDomainService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    internal partial class DomainServiceClient : System.ServiceModel.DuplexClientBase<Ntreev.Crema.Services.DomainService.IDomainService>, Ntreev.Crema.Services.DomainService.IDomainService {
        
        public DomainServiceClient(System.ServiceModel.InstanceContext callbackInstance) : 
                base(callbackInstance) {
        }
        
        public DomainServiceClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName) : 
                base(callbackInstance, endpointConfigurationName) {
        }
        
        public DomainServiceClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, string remoteAddress) : 
                base(callbackInstance, endpointConfigurationName, remoteAddress) {
        }
        
        public DomainServiceClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(callbackInstance, endpointConfigurationName, remoteAddress) {
        }
        
        public DomainServiceClient(System.ServiceModel.InstanceContext callbackInstance, System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(callbackInstance, binding, remoteAddress) {
        }
        
        public Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.DomainContextMetaData> Subscribe(System.Guid authenticationToken) {
            return base.Channel.Subscribe(authenticationToken);
        }
        
        public Ntreev.Crema.ServiceModel.ResultBase Unsubscribe() {
            return base.Channel.Unsubscribe();
        }
        
        public Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.DomainMetaData[]> GetMetaData(System.Guid dataBaseID) {
            return base.Channel.GetMetaData(dataBaseID);
        }
        
        public Ntreev.Crema.ServiceModel.ResultBase SetUserLocation(System.Guid domainID, Ntreev.Crema.ServiceModel.DomainLocationInfo location) {
            return base.Channel.SetUserLocation(domainID, location);
        }
        
        public Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.DomainRowResultInfo> NewRow(System.Guid domainID, Ntreev.Crema.ServiceModel.DomainRowInfo[] rows) {
            return base.Channel.NewRow(domainID, rows);
        }
        
        public Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.DomainRowResultInfo> RemoveRow(System.Guid domainID, Ntreev.Crema.ServiceModel.DomainRowInfo[] rows) {
            return base.Channel.RemoveRow(domainID, rows);
        }
        
        public Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.DomainRowResultInfo> SetRow(System.Guid domainID, Ntreev.Crema.ServiceModel.DomainRowInfo[] rows) {
            return base.Channel.SetRow(domainID, rows);
        }
        
        public Ntreev.Crema.ServiceModel.ResultBase SetProperty(System.Guid domainID, string propertyName, object value) {
            return base.Channel.SetProperty(domainID, propertyName, value);
        }
        
        public Ntreev.Crema.ServiceModel.ResultBase BeginUserEdit(System.Guid domainID, Ntreev.Crema.ServiceModel.DomainLocationInfo location) {
            return base.Channel.BeginUserEdit(domainID, location);
        }
        
        public Ntreev.Crema.ServiceModel.ResultBase EndUserEdit(System.Guid domainID) {
            return base.Channel.EndUserEdit(domainID);
        }
        
        public Ntreev.Crema.ServiceModel.ResultBase<Ntreev.Crema.ServiceModel.DomainUserInfo> Kick(System.Guid domainID, string userID, string comment) {
            return base.Channel.Kick(domainID, userID, comment);
        }
        
        public Ntreev.Crema.ServiceModel.ResultBase SetOwner(System.Guid domainID, string userID) {
            return base.Channel.SetOwner(domainID, userID);
        }
        
        public Ntreev.Crema.ServiceModel.ResultBase<object> DeleteDomain(System.Guid domainID, bool force) {
            return base.Channel.DeleteDomain(domainID, force);
        }
        
        public bool IsAlive() {
            return base.Channel.IsAlive();
        }
    }
}
