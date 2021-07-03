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
using JSSoft.Crema.Services.Users.Serializations;
using JSSoft.Library;
using JSSoft.Library.Linq;
using JSSoft.Library.ObjectModel;
using System.Data;
using System.Linq;
using System.Security;

namespace JSSoft.Crema.Services.Users.Arguments
{
    class UserCreateArguments : UserArgumentsBase
    {
        public UserCreateArguments(string userID, string categoryPath, SecureString password, string userName, Authority authority)
        {
            this.UserID = userID;
            this.CategoryPath = categoryPath;
            this.Password = password;
            this.UserName = userName;
            this.Authority = authority;
            this.UserPath = categoryPath + userID;
            this.LockPaths = new[] { categoryPath, categoryPath + userID, };
        }

        public UserSet Create(Authentication authentication)
        {
            var userInfo = new UserSerializationInfo()
            {
                ID = this.UserID,
                Password = UserContext.SecureStringToString(this.Password).Encrypt(),
                Name = this.UserName,
                Authority = this.Authority,
                CategoryPath = this.CategoryPath,
            };
            var dataSet = new UserSet()
            {
                ItemPaths = this.LockPaths,
                Infos = new UserSerializationInfo[] { userInfo },
                SignatureDateProvider = new SignatureDateProvider(authentication.ID),
            };
            return dataSet;
        }

        public string UserID { get; }

        public string CategoryPath { get; }

        public SecureString Password { get; }

        public string UserName { get; }

        public Authority Authority { get; }

        public string UserPath { get; }

        public string[] LockPaths { get; }
    }
}
