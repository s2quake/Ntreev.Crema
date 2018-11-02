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

using Ntreev.Library.IO;
using Ntreev.Library.ObjectModel;
using System;
using System.IO;

namespace Ntreev.Crema.Services
{
    class RepositoryPath
    {
        private readonly string basePath;
        private readonly string path;

        public RepositoryPath(string basePath, string path)
        {
            this.basePath = basePath;
            this.path = this.GeneratePath(path);
        }

        public RepositoryPath(string basePath, string kind, string path)
        {
            this.basePath = Path.Combine(basePath, kind);
            this.path = this.GeneratePath(path);
        }

        public override string ToString()
        {
            return $"\"{this.path}\"";
        }

        public static implicit operator string(RepositoryPath path)
        {
            return path.ToString();
        }

        private string GenerateCategoryPath(string parentPath, string name)
        {
            var value = new CategoryName(parentPath, name);
            return this.GenerateCategoryPath(value.Path);
        }

        private string GenerateCategoryPath(string categoryPath)
        {
            NameValidator.ValidateCategoryPath(categoryPath);
            var baseUri = new Uri(this.basePath);
            var uri = new Uri(baseUri + categoryPath);
            return uri.LocalPath;
        }

        private string GenerateUserPath(string categoryPath, string userID)
        {
            return Path.Combine(this.GenerateCategoryPath(categoryPath), userID);
        }

        private string GeneratePath(string path)
        {
            if (NameValidator.VerifyCategoryPath(path) == true)
                return this.GenerateCategoryPath(path);
            var itemName = new ItemName(path);
            return this.GenerateUserPath(itemName.CategoryPath, itemName.Name);
        }
    }
}
