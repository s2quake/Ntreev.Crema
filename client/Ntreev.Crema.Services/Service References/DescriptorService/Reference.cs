﻿//------------------------------------------------------------------------------
// <auto-generated>
//     이 코드는 도구를 사용하여 생성되었습니다.
//     런타임 버전:4.0.30319.42000
//
//     파일 내용을 변경하면 잘못된 동작이 발생할 수 있으며, 코드를 다시 생성하면
//     이러한 변경 내용이 손실됩니다.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Ntreev.Crema.Services.DescriptorService {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="http://www.ntreev.com", ConfigurationName="DescriptorService.IDescriptorService")]
    internal interface IDescriptorService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/IDescriptorService/GetVersion", ReplyAction="http://www.ntreev.com/IDescriptorService/GetVersionResponse")]
        string GetVersion();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/IDescriptorService/GetVersion", ReplyAction="http://www.ntreev.com/IDescriptorService/GetVersionResponse")]
        System.Threading.Tasks.Task<string> GetVersionAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/IDescriptorService/IsOnline", ReplyAction="http://www.ntreev.com/IDescriptorService/IsOnlineResponse")]
        bool IsOnline(string userID, byte[] password);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/IDescriptorService/IsOnline", ReplyAction="http://www.ntreev.com/IDescriptorService/IsOnlineResponse")]
        System.Threading.Tasks.Task<bool> IsOnlineAsync(string userID, byte[] password);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/IDescriptorService/GetDataBaseInfos", ReplyAction="http://www.ntreev.com/IDescriptorService/GetDataBaseInfosResponse")]
        Ntreev.Crema.ServiceModel.DataBaseInfo[] GetDataBaseInfos();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/IDescriptorService/GetDataBaseInfos", ReplyAction="http://www.ntreev.com/IDescriptorService/GetDataBaseInfosResponse")]
        System.Threading.Tasks.Task<Ntreev.Crema.ServiceModel.DataBaseInfo[]> GetDataBaseInfosAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/IDescriptorService/GetServiceInfos", ReplyAction="http://www.ntreev.com/IDescriptorService/GetServiceInfosResponse")]
        Ntreev.Crema.ServiceModel.ServiceInfo[] GetServiceInfos();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/IDescriptorService/GetServiceInfos", ReplyAction="http://www.ntreev.com/IDescriptorService/GetServiceInfosResponse")]
        System.Threading.Tasks.Task<Ntreev.Crema.ServiceModel.ServiceInfo[]> GetServiceInfosAsync();
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    internal interface IDescriptorServiceChannel : Ntreev.Crema.Services.DescriptorService.IDescriptorService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    internal partial class DescriptorServiceClient : System.ServiceModel.ClientBase<Ntreev.Crema.Services.DescriptorService.IDescriptorService>, Ntreev.Crema.Services.DescriptorService.IDescriptorService {
        
        public DescriptorServiceClient() {
        }
        
        public DescriptorServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public DescriptorServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public DescriptorServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public DescriptorServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public string GetVersion() {
            return base.Channel.GetVersion();
        }
        
        public System.Threading.Tasks.Task<string> GetVersionAsync() {
            return base.Channel.GetVersionAsync();
        }
        
        public bool IsOnline(string userID, byte[] password) {
            return base.Channel.IsOnline(userID, password);
        }
        
        public System.Threading.Tasks.Task<bool> IsOnlineAsync(string userID, byte[] password) {
            return base.Channel.IsOnlineAsync(userID, password);
        }
        
        public Ntreev.Crema.ServiceModel.DataBaseInfo[] GetDataBaseInfos() {
            return base.Channel.GetDataBaseInfos();
        }
        
        public System.Threading.Tasks.Task<Ntreev.Crema.ServiceModel.DataBaseInfo[]> GetDataBaseInfosAsync() {
            return base.Channel.GetDataBaseInfosAsync();
        }
        
        public Ntreev.Crema.ServiceModel.ServiceInfo[] GetServiceInfos() {
            return base.Channel.GetServiceInfos();
        }
        
        public System.Threading.Tasks.Task<Ntreev.Crema.ServiceModel.ServiceInfo[]> GetServiceInfosAsync() {
            return base.Channel.GetServiceInfosAsync();
        }
    }
}
