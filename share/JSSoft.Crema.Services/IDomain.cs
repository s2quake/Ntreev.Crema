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
using System.Threading.Tasks;

namespace JSSoft.Crema.Services
{
    public interface IDomain : IServiceProvider, IDispatcherObject, IExtendedProperties
    {
        Task DeleteAsync(Authentication authentication, bool isCancel);

        Task BeginUserEditAsync(Authentication authentication, DomainLocationInfo location);

        Task EndUserEditAsync(Authentication authentication);

        Task NewRowAsync(Authentication authentication, DomainRowInfo[] rows);

        Task SetRowAsync(Authentication authentication, DomainRowInfo[] rows);

        Task RemoveRowAsync(Authentication authentication, DomainRowInfo[] rows);

        Task SetPropertyAsync(Authentication authentication, string propertyName, object value);

        Task SetUserLocationAsync(Authentication authentication, DomainLocationInfo location);

        Task KickAsync(Authentication authentication, string userID, string comment);

        Task SetOwnerAsync(Authentication authentication, string userID);

        Guid ID { get; }

        Guid DataBaseID { get; }

        object Source { get; }

        object Result { get; }

        object Host { get; }

        DomainInfo DomainInfo { get; }

        DomainState DomainState { get; }

        IDomainUserCollection Users { get; }

        event EventHandler<DomainUserEventArgs> UserAdded;

        event EventHandler<DomainUserRemovedEventArgs> UserRemoved;

        event EventHandler<DomainUserLocationEventArgs> UserLocationChanged;

        event EventHandler<DomainUserEventArgs> UserStateChanged;

        event EventHandler<DomainUserLocationEventArgs> UserEditBegun;

        event EventHandler<DomainUserEventArgs> UserEditEnded;

        event EventHandler<DomainUserEventArgs> OwnerChanged;

        event EventHandler<DomainRowEventArgs> RowAdded;

        event EventHandler<DomainRowEventArgs> RowRemoved;

        event EventHandler<DomainRowEventArgs> RowChanged;

        event EventHandler<DomainPropertyEventArgs> PropertyChanged;

        event EventHandler DomainInfoChanged;

        event EventHandler DomainStateChanged;

        event EventHandler<DomainDeletedEventArgs> Deleted;

        Task<DomainMetaData> GetMetaDataAsync(Authentication authentication);
    }
}
