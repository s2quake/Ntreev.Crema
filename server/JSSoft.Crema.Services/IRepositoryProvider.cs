﻿// Released under the MIT License.
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

namespace JSSoft.Crema.Services
{
    public interface IRepositoryProvider
    {
        void InitializeRepository(string basePath, string initPath, params LogPropertyInfo[] properties);

        void CreateRepository(string author, string basePath, string initPath, string comment, params LogPropertyInfo[] properties);

        void CloneRepository(string author, string basePath, string repositoryName, string newRepositoryName, string comment, string revision, params LogPropertyInfo[] properties);

        void RenameRepository(string author, string basePath, string repositoryName, string newRepositoryName, string comment, params LogPropertyInfo[] properties);

        void DeleteRepository(string author, string basePath, string repositoryName, string comment, params LogPropertyInfo[] properties);

        void RevertRepository(string author, string basePath, string repositoryName, string revision, string comment);

        void ExportRepository(string basePath, string repositoryName, string revision, string exportPath);

        IRepository CreateInstance(RepositorySettings settings);

        string[] GetRepositories(string basePath);

        string GetRevision(string basePath, string repositoryName);

        RepositoryInfo GetRepositoryInfo(string basePath, string repositoryName);

        string[] GetRepositoryItemList(string basePath, string repositoryName);

        LogInfo[] GetLog(string basePath, string repositoryName, string revision);

        string Name { get; }
    }
}