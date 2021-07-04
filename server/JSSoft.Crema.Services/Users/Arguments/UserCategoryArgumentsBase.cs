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

using JSSoft.Crema.Services.Users.Serializations;
using JSSoft.Library;
using JSSoft.Library.Linq;
using JSSoft.Library.ObjectModel;
using System.Collections.Generic;
using System.Linq;

namespace JSSoft.Crema.Services.Users.Arguments
{
    abstract class UserCategoryArgumentsBase
    {
        protected static (string[] userPaths, string[] lockPaths) GetPathForData(UserCategory uesrCategory, CategoryName targetName)
        {
            var targetPaths = new string[]
            {
                targetName.ParentPath,
                targetName,
            };
            var items = EnumerableUtility.FamilyTree(uesrCategory as IUserItem, item => item.Childs);
            var users = items.Where(item => item is User).Select(item => item as User).ToArray();
            var userPaths = users.Select(item => item.Path).ToArray();
            var itemPaths = items.Select(item => item.Path).ToArray();
            var lockPaths = itemPaths.Concat(targetPaths).Distinct().OrderBy(item => item).ToArray();
            return (userPaths, lockPaths);
        }

        protected static UserSet ReadDataForPath(Authentication authentication, UserRepositoryHost repository, string[] userPaths, string[] lockPaths)
        {
            var userInfoList = new List<UserSerializationInfo>(userPaths.Length);
            foreach (var item in userPaths)
            {
                var userInfo = repository.Read(item);
                userInfoList.Add(userInfo);
            }
            var dataSet = new UserSet()
            {
                ItemPaths = lockPaths,
                Infos = userInfoList.ToArray(),
                SignatureDateProvider = new SignatureDateProvider(authentication.ID),
            };
            return dataSet;
        }

        protected static UserSet ReadDataForChange(Authentication authentication, UserRepositoryHost repository, string userPath, string[] lockPaths)
        {
            var userInfo = repository.Read(userPath);
            var dataSet = new UserSet()
            {
                ItemPaths = lockPaths,
                Infos = new[] { userInfo },
                SignatureDateProvider = new SignatureDateProvider(authentication.ID),
            };
            return dataSet;
        }
    }
}
