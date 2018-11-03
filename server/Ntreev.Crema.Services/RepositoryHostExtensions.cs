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

using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services.Users;
using Ntreev.Library;
using Ntreev.Library.ObjectModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services
{
    static class RepositoryHostExtentions
    {
        public static void Add(this RepositoryHost repositoryHost, RepositoryPath path)
        {
            var files = path.GetFiles();
            var status = repositoryHost.Status(files);
            foreach (var item in status)
            {
                if (item.Status == RepositoryItemStatus.Untracked)
                {
                    repositoryHost.Add(item.Path);
                }
            }
        }

        public static void AddRange(this RepositoryHost repositoryHost, RepositoryPath[] paths)
        {
            repositoryHost.AddRange(paths.Select(item => item.Path).ToArray());
        }

        public static void Add(this RepositoryHost repositoryHost, RepositoryPath path, string contents)
        {
            repositoryHost.Add(path.Path);
        }

        public static void Modify(this RepositoryHost repositoryHost, RepositoryPath path, string contents)
        {
            repositoryHost.Modify(path.Path, contents);
        }

        public static void Move(this RepositoryHost repositoryHost, RepositoryPath srcPath, RepositoryPath toPath)
        {
            if (srcPath.IsDirectory == true)
            {
                repositoryHost.Move(srcPath.Path, toPath.Path);
            }
            else
            {
                var files = srcPath.GetFiles();

                for (var i = 0; i < files.Length; i++)
                {
                    var path1 = files[i];
                    var extension = Path.GetExtension(path1);
                    var path2 = toPath.Path + extension;
                    repositoryHost.Move(path1, path2);
                }
            }
        }

        public static void Delete(this RepositoryHost repositoryHost, RepositoryPath path)
        {
            var files = path.GetFiles();
            foreach (var item in files)
            {
                if (File.Exists(item) == false)
                    throw new FileNotFoundException();
            }
            repositoryHost.DeleteRange(files);
        }

        public static void DeleteRange(this RepositoryHost repositoryHost, RepositoryPath[] paths)
        {
            var files = paths.SelectMany(item => item.GetFiles()).ToArray();
            foreach (var item in files)
            {
                if (File.Exists(item) == false)
                    throw new FileNotFoundException();
            }
            repositoryHost.DeleteRange(files);
        }

        public static void Copy(this RepositoryHost repositoryHost, RepositoryPath srcPath, RepositoryPath toPath)
        {
            repositoryHost.Copy(srcPath.Path, toPath.Path);
        }
    }
}
