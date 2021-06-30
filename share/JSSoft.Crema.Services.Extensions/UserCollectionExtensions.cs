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

using System;
using System.Linq;
using System.Threading.Tasks;
using JSSoft.Library;

namespace JSSoft.Crema.Services.Extensions
{
    public static class UserCollectionExtensions
    {
        public static Task<bool> ContainsAsync(this IUserCollection userCollection, string userID)
        {
            return userCollection.Dispatcher.InvokeAsync(() => userCollection.Contains(userID));
        }

        public static Task<IUser> GetUserAsync(this IUserCollection userCollection, string userID)
        {
            return userCollection.Dispatcher.InvokeAsync(() => userCollection[userID]);
        }

        public static Task<IUser[]> GetUsersAsync(this IUserCollection userCollection)
        {
            return userCollection.Dispatcher.InvokeAsync(() => userCollection.ToArray());
        }

        public static Task<string> GenerateNewUserIDAsync(this IUserCollection userCollection)
        {
            return GenerateNewUserIDAsync(userCollection, "user");
        }

        public static Task<string> GenerateNewUserIDAsync(this IUserCollection userCollection, string userID)
        {
            return userCollection.Dispatcher.InvokeAsync(() => NameUtility.GenerateNewName(userID, userCollection.Select(item => item.ID)));
        }
        
        public static Task AddUsersCreatedEventHandlerAsync(this IUserCollection userCollection, ItemsCreatedEventHandler<IUser> handler)
        {
            return userCollection.Dispatcher.InvokeAsync(() => userCollection.UsersCreated += handler);
        }

        public static Task AddUsersMovedEventHandlerAsync(this IUserCollection userCollection, ItemsMovedEventHandler<IUser> handler)
        {
            return userCollection.Dispatcher.InvokeAsync(() => userCollection.UsersMoved += handler);
        }

        public static Task AddUsersDeletedEventHandlerAsync(this IUserCollection userCollection, ItemsDeletedEventHandler<IUser> handler)
        {
            return userCollection.Dispatcher.InvokeAsync(() => userCollection.UsersDeleted += handler);
        }

        public static Task AddUsersStateChangedEventHandlerAsync(this IUserCollection userCollection, ItemsEventHandler<IUser> handler)
        {
            return userCollection.Dispatcher.InvokeAsync(() => userCollection.UsersStateChanged += handler);
        }

        public static Task AddUsersChangedEventHandlerAsync(this IUserCollection userCollection, ItemsEventHandler<IUser> handler)
        {
            return userCollection.Dispatcher.InvokeAsync(() => userCollection.UsersChanged += handler);
        }

        public static Task AddUsersLoggedInEventHandlerAsync(this IUserCollection userCollection, ItemsEventHandler<IUser> handler)
        {
            return userCollection.Dispatcher.InvokeAsync(() => userCollection.UsersLoggedIn += handler);
        }

        public static Task AddUsersLoggedOutEventHandlerAsync(this IUserCollection userCollection, ItemsEventHandler<IUser> handler)
        {
            return userCollection.Dispatcher.InvokeAsync(() => userCollection.UsersLoggedOut += handler);
        }

        public static Task AddUsersKickedEventHandlerAsync(this IUserCollection userCollection, ItemsEventHandler<IUser> handler)
        {
            return userCollection.Dispatcher.InvokeAsync(() => userCollection.UsersKicked += handler);
        }

        public static Task AddUsersBanChangedEventHandlerAsync(this IUserCollection userCollection, ItemsEventHandler<IUser> handler)
        {
            return userCollection.Dispatcher.InvokeAsync(() => userCollection.UsersBanChanged += handler);
        }

        public static Task AddMessageReceivedEventHandlerAsync(this IUserCollection userCollection, EventHandler<MessageEventArgs> handler)
        {
            return userCollection.Dispatcher.InvokeAsync(() => userCollection.MessageReceived += handler);
        }

        public static Task RemoveUsersCreatedEventHandlerAsync(this IUserCollection userCollection, ItemsCreatedEventHandler<IUser> handler)
        {
            return userCollection.Dispatcher.InvokeAsync(() => userCollection.UsersCreated -= handler);
        }

        public static Task RemoveUsersMovedEventHandlerAsync(this IUserCollection userCollection, ItemsMovedEventHandler<IUser> handler)
        {
            return userCollection.Dispatcher.InvokeAsync(() => userCollection.UsersMoved -= handler);
        }

        public static Task RemoveUsersDeletedEventHandlerAsync(this IUserCollection userCollection, ItemsDeletedEventHandler<IUser> handler)
        {
            return userCollection.Dispatcher.InvokeAsync(() => userCollection.UsersDeleted -= handler);
        }

        public static Task RemoveUsersStateChangedEventHandlerAsync(this IUserCollection userCollection, ItemsEventHandler<IUser> handler)
        {
            return userCollection.Dispatcher.InvokeAsync(() => userCollection.UsersStateChanged -= handler);
        }

        public static Task RemoveUsersChangedEventHandlerAsync(this IUserCollection userCollection, ItemsEventHandler<IUser> handler)
        {
            return userCollection.Dispatcher.InvokeAsync(() => userCollection.UsersChanged -= handler);
        }

        public static Task RemoveUsersLoggedInEventHandlerAsync(this IUserCollection userCollection, ItemsEventHandler<IUser> handler)
        {
            return userCollection.Dispatcher.InvokeAsync(() => userCollection.UsersLoggedIn -= handler);
        }

        public static Task RemoveUsersLoggedOutEventHandlerAsync(this IUserCollection userCollection, ItemsEventHandler<IUser> handler)
        {
            return userCollection.Dispatcher.InvokeAsync(() => userCollection.UsersLoggedOut -= handler);
        }

        public static Task RemoveUsersKickedEventHandlerAsync(this IUserCollection userCollection, ItemsEventHandler<IUser> handler)
        {
            return userCollection.Dispatcher.InvokeAsync(() => userCollection.UsersKicked -= handler);
        }

        public static Task RemoveUsersBanChangedEventHandlerAsync(this IUserCollection userCollection, ItemsEventHandler<IUser> handler)
        {
            return userCollection.Dispatcher.InvokeAsync(() => userCollection.UsersBanChanged -= handler);
        }

        public static Task RemoveMessageReceivedEventHandlerAsync(this IUserCollection userCollection, EventHandler<MessageEventArgs> handler)
        {
            return userCollection.Dispatcher.InvokeAsync(() => userCollection.MessageReceived -= handler);
        }
    }
}
