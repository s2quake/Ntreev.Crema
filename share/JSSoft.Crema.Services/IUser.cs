// Released under the MIT License.
// 
// Copyright (c) 2018 Ntreev Soft co., Ltd.
// Copyright (c) 2020 Jeesu Choi
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit
// persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the
// Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// Forked from https://github.com/NtreevSoft/Crema
// Namespaces and files starting with "Ntreev" have been renamed to "JSSoft".

using JSSoft.Crema.ServiceModel;
using System;
using System.Security;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services
{
    public interface IUser : IServiceProvider, IDispatcherObject, IExtendedProperties
    {
        Task MoveAsync(Authentication authentication, string categoryPath);

        Task DeleteAsync(Authentication authentication);

        Task SetUserNameAsync(Authentication authentication, SecureString password, string userName);

        Task SetPasswordAsync(Authentication authentication, SecureString password, SecureString newPassword);

        Task ResetPasswordAsync(Authentication authentication);

        Task SendMessageAsync(Authentication authentication, string message);

        Task KickAsync(Authentication authentication, string comment);

        Task BanAsync(Authentication authentication, string comment);

        Task UnbanAsync(Authentication authentication);

        string ID { get; }

        string UserName { get; }

        string Path { get; }

        Authority Authority { get; }

        IUserCategory Category { get; }

        UserInfo UserInfo { get; }

        UserState UserState { get; }

        BanInfo BanInfo { get; }

        UserFlags UserFlags { get; }

        [DispatcherEvent(typeof(IUserContext))]
        event EventHandler Renamed;

        [DispatcherEvent(typeof(IUserContext))]
        event EventHandler Moved;

        [DispatcherEvent(typeof(IUserContext))]
        event EventHandler Deleted;

        [DispatcherEvent(typeof(IUserContext))]
        event EventHandler UserInfoChanged;

        [DispatcherEvent(typeof(IUserContext))]
        event EventHandler UserStateChanged;

        [DispatcherEvent(typeof(IUserContext))]
        event EventHandler UserBanInfoChanged;
    }
}
