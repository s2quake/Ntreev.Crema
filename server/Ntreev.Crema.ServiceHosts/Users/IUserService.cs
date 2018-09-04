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
using System.Security;

namespace Ntreev.Crema.ServiceHosts.Users
{
    [ServiceContract(Namespace = CremaService.Namespace, SessionMode = SessionMode.Required, CallbackContract = typeof(IUserEventCallback))]
    public interface IUserService
    {
        [OperationContract]
        Task<ResultBase<UserContextMetaData>> SubscribeAsync(string userID, byte[] password, string version, string platformID, string culture);

        [OperationContract]
        Task<ResultBase> UnsubscribeAsync();

        [OperationContract]
        Task<ResultBase> ShutdownAsync(int milliseconds, ShutdownType shutdownType, string message);

        [OperationContract]
        Task<ResultBase> CancelShutdownAsync();

        [OperationContract]
        Task<ResultBase<UserInfo>> NewUserAsync(string userID, string categoryPath, byte[] password, string userName, Authority authority);

        [OperationContract]
        Task<ResultBase> NewUserCategoryAsync(string categoryPath);

        [OperationContract]
        Task<ResultBase> RenameUserItemAsync(string itemPath, string newName);

        [OperationContract]
        Task<ResultBase> MoveUserItemAsync(string itemPath, string parentPath);

        [OperationContract]
        Task<ResultBase> DeleteUserItemAsync(string itemPath);

        [OperationContract]
        Task<ResultBase<UserInfo>> ChangeUserInfoAsync(string userID, byte[] password, byte[] newPassword, string userName, Authority? authority);

        [OperationContract]
        Task<ResultBase> KickAsync(string userID, string comment);

        [OperationContract]
        Task<ResultBase<BanInfo>> BanAsync(string userID, string comment);

        [OperationContract]
        Task<ResultBase> UnbanAsync(string userID);

        [OperationContract]
        Task<ResultBase> SendMessageAsync(string userID, string message);

        [OperationContract]
        Task<ResultBase> NotifyMessageAsync(string[] userIDs, string message);

        [OperationContract]
        bool IsAlive();
    }
}
