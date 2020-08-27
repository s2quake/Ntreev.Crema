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

using JSSoft.Crema.Data.Xml.Schema;
using JSSoft.Library.IO;
using JSSoft.Library.ObjectModel;
using System;
using System.IO;
using System.Linq;

namespace JSSoft.Crema.Services
{
    struct RepositoryPath
    {
        public RepositoryPath(string basePath, string path)
        {
            this.Path = GeneratePath(basePath, path);
        }

        internal RepositoryPath(JSSoft.Crema.Services.Users.UserContext userContext, string path)
        {
            this.Path = GeneratePath(userContext.BasePath, path);
        }

        internal RepositoryPath(JSSoft.Crema.Services.Data.DataBase dataBase, string path)
        {
            this.Path = GeneratePath(dataBase.BasePath, path);
        }

        internal RepositoryPath(JSSoft.Crema.Services.Data.TypeContext typeContext, string path)
        {
            if (path.StartsWith(PathUtility.Separator + CremaSchema.TypeDirectory) == true ||
                path.StartsWith(PathUtility.Separator + CremaSchema.TableDirectory) == true)
                throw new ArgumentException(nameof(path));
            this.Path = GeneratePath(typeContext.BasePath, path);
        }

        internal RepositoryPath(JSSoft.Crema.Services.Data.TableContext tableContext, string path)
        {
            if (path.StartsWith(PathUtility.Separator + CremaSchema.TypeDirectory) == true ||
                path.StartsWith(PathUtility.Separator + CremaSchema.TableDirectory) == true)
                throw new ArgumentException(nameof(path));
            this.Path = GeneratePath(tableContext.BasePath, path);
        }

        public override string ToString()
        {
            return this.Path;
        }

        public override int GetHashCode()
        {
            if (this.Path == null)
                return 0;
            return this.Path.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is RepositoryPath path)
            {
                return this.Path == path.Path;
            }
            return false;
        }

        public string[] GetFiles()
        {
            if (this.IsDirectory == true)
                throw new InvalidOperationException();
            var directoryName = System.IO.Path.GetDirectoryName(this.Path);
            if (Directory.Exists(directoryName) == false)
                return new string[] { };
            var name = System.IO.Path.GetFileName(this.Path);
            var files = Directory.GetFiles(directoryName, $"{name}.*").Where(item => System.IO.Path.GetFileNameWithoutExtension(item) == name).ToArray();
            return files;
        }

        public void ValidateExists()
        {
            if (this.IsDirectory == true)
            {
                if (this.IsExists == false)
                    throw new DirectoryNotFoundException();
            }
            else
            {
                if (this.IsExists == false)
                    throw new FileNotFoundException();
            }
        }

        public void ValidateNotExists()
        {
            if (this.IsDirectory == true)
            {
                if (this.IsExists == true)
                    throw new IOException();
            }
            else
            {
                if (this.IsExists == true)
                    throw new IOException();
            }
        }

        public void ValidateExists(IObjectSerializer serializer, Type type)
        {
            this.ValidateExists(serializer, type, ObjectSerializerSettings.Empty);
        }

        public void ValidateExists(IObjectSerializer serializer, Type type, ObjectSerializerSettings settings)
        {
            var files = serializer.GetPath(this.Path, type, settings);
            foreach (var item in files)
            {
                if (File.Exists(item) == false)
                    throw new FileNotFoundException();
            }
        }

        public void ValidateNotExists(IObjectSerializer serializer, Type type)
        {
            this.ValidateNotExists(serializer, type, ObjectSerializerSettings.Empty);
        }

        public void ValidateNotExists(IObjectSerializer serializer, Type type, ObjectSerializerSettings settings)
        {
            var files = serializer.GetPath(this.Path, type, settings);
            foreach (var item in files)
            {
                if (File.Exists(item) == true)
                    throw new FileNotFoundException();
            }
        }

        public string Path { get; set; }

        public bool IsDirectory => this.Path.EndsWith($"{System.IO.Path.DirectorySeparatorChar}");

        public bool IsExists
        {
            get
            {
                if (this.IsDirectory == true)
                {
                    return DirectoryUtility.Exists(this.Path);
                }
                else
                {
                    return this.GetFiles().Any();
                }
            }
        }

        public RepositoryPath ParentPath
        {
            get
            {
                if (this.IsDirectory == true)
                {
                    var path = this.Path.TrimEnd(System.IO.Path.DirectorySeparatorChar);
                    var directoryName = System.IO.Path.GetDirectoryName(path);
                    return new RepositoryPath() { Path = directoryName + System.IO.Path.DirectorySeparatorChar };
                }
                else
                {
                    var directoryName = System.IO.Path.GetDirectoryName(this.Path);
                    return new RepositoryPath() { Path = directoryName + System.IO.Path.DirectorySeparatorChar };
                }
            }
        }

        public static bool operator ==(RepositoryPath t1, RepositoryPath t2)
        {
            return t1.Path == t2.Path;
        }

        public static bool operator !=(RepositoryPath t1, RepositoryPath t2)
        {
            return t1.Path != t2.Path;
        }

        public static readonly RepositoryPath Empty = new RepositoryPath() { Path = string.Empty };

        private static string GenerateCategoryPath(string basePath, string categoryPath)
        {
            NameValidator.ValidateCategoryPath(categoryPath);
            var baseUri = new Uri(basePath);
            var uri = new Uri(baseUri + categoryPath);
            return uri.LocalPath;
        }

        private static string GenerateUserPath(string basePath, string categoryPath, string userID)
        {
            return System.IO.Path.Combine(GenerateCategoryPath(basePath, categoryPath), userID);
        }

        private static string GeneratePath(string basePath, string path)
        {
            if (NameValidator.VerifyCategoryPath(path) == true)
                return GenerateCategoryPath(basePath, path);
            var itemName = new ItemName(path);
            return GenerateUserPath(basePath, itemName.CategoryPath, itemName.Name);
        }
    }
}
