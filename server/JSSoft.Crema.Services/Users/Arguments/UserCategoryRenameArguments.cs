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
using JSSoft.Library.ObjectModel;
using System.Data;
using System.Linq;

namespace JSSoft.Crema.Services.Users.Arguments
{
    class UserCategoryRenameArguments : UserCategoryArgumentsBase
    {
        public UserCategoryRenameArguments(UserCategory userCategory, string name)
        {
            var items = EnumerableUtility.One(userCategory).ToArray();
            var oldNames = items.Select(item => item.Name).ToArray();
            var oldPaths = items.Select(item => item.Path).ToArray();
            var path = userCategory.Path;
            var targetName = new CategoryName(userCategory.Path) { Name = name };
            var (userPaths, lockPaths) = GetPathForData(userCategory, targetName);
            this.Name = name;
            this.CategoryPath = userCategory.Path;
            this.NewCategoryPath = new CategoryName(userCategory.Path) { Name = name, };
            this.Items = items;
            this.OldNames = oldNames;
            this.OldPaths = oldPaths;
            this.UserPaths = userPaths;
            this.LockPaths = lockPaths;
        }

        public UserSet Read(Authentication authentication, UserRepositoryHost repository)
        {
            return ReadDataForPath(authentication, repository, this.UserPaths, this.LockPaths);
        }

        public string Name { get; }

        public string CategoryPath { get; }

        public string NewCategoryPath { get; }

        public UserCategory[] Items { get; }

        public string[] OldNames { get; }

        public string[] OldPaths { get; }

        public string[] UserPaths { get; }

        public string[] LockPaths { get; }
    }
}
