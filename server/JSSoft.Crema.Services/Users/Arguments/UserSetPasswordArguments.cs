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
using JSSoft.Library.Linq;
using System.Linq;
using System.Security;

namespace JSSoft.Crema.Services.Users.Arguments
{
    class UserSetPasswordArguments : UserArgumentsBase
    {
        public UserSetPasswordArguments(User user, SecureString password)
        {
            var items = EnumerableUtility.One(user).ToArray();
            var userID = user.ID;
            var userInfo = user.UserInfo;
            var userPath = user.Path;
            var lockPaths = new[] { user.Path };
            this.UserID = userInfo.ID;
            this.UserName = userInfo.Name;
            this.Password = password;
            this.Items = items;
            this.UserInfo = userInfo;
            this.UserPath = userPath;
            this.LockPaths = lockPaths;
        }

        public UserSet Read(Authentication authentication, UserRepositoryHost repository)
        {
            return ReadDataForChange(authentication, repository, this.UserPath, this.LockPaths);
        }

        public string UserID { get; }

        public string UserName { get; }

        public SecureString Password { get; }

        public User[] Items { get; }

        public UserInfo UserInfo { get; }

        public string UserPath { get; }

        public string[] LockPaths { get; }
    }
}
