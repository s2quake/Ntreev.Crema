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

namespace JSSoft.Crema.Services
{
    /// <summary>
    /// 경로로 표현되는 객체를 읽고 쓰도록 하는 인터페이스입니다.
    /// 경로는 itemPath로 나타내며 확장자가 포함되지 않는 순수 경로를 나타냅니다. 
    /// 폴더를 일 경우에는 경우 마지막에 경로 문자(window는 \, linux는 /)가 있게 됩니다.
    /// 또한 실제 접근 가능한 경로로 표현됩니다.
    /// C:\\repo-svn\database\tables\
    /// C:\\repo-svn\database\tables\table1
    /// </summary>
    public interface IObjectSerializer
    {
        string[] Serialize(string itemPath, object obj, ObjectSerializerSettings settings);

        object Deserialize(string itemPath, Type type, ObjectSerializerSettings settings);

        string[] GetPath(string itemPath, Type type, ObjectSerializerSettings settings);

        string[] GetReferencedPath(string itemPath, Type type, ObjectSerializerSettings settings);

        string[] GetItemPaths(string path, Type type, ObjectSerializerSettings settings);

        void Validate(string itemPath, Type type, ObjectSerializerSettings settings);

        bool Exists(string itemPath, Type type, ObjectSerializerSettings settings);

        string Name { get; }
    }

    static class IObjectSerializerExtensions
    {
        public static string[] Serialize(this IObjectSerializer serializer, RepositoryPath repositoryPath, object obj, ObjectSerializerSettings settings)
        {
            return serializer.Serialize(repositoryPath.Path, obj, settings);
        }

        public static T Deserialize<T>(this IObjectSerializer serializer, RepositoryPath repositoryPath, ObjectSerializerSettings settings)
        {
            return (T)serializer.Deserialize(repositoryPath.Path, typeof(T), settings);
        }
    }
}
