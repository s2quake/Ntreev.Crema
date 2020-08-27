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

namespace JSSoft.Crema.Repository.Git
{
    static class GitConfig
    {
        public static void SetValue(string repositoryPath, string name, string value)
        {
            if (repositoryPath == null)
                throw new ArgumentNullException(nameof(repositoryPath));
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var configCommand = new GitCommand(repositoryPath, "config")
            {
                name,
                (GitString)value
            };
            configCommand.Run();
        }

        public static void UnsetValue(string repositoryPath, string name)
        {
            if (repositoryPath == null)
                throw new ArgumentNullException(nameof(repositoryPath));
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (HasValue(repositoryPath, name) == false)
                return;
            var configCommand = new GitCommand(repositoryPath, "config")
            {
                new GitCommandItem("unset"),
                name,
            };
            configCommand.Run();
        }

        public static bool HasValue(string repositoryPath, string name)
        {
            if (repositoryPath == null)
                throw new ArgumentNullException(nameof(repositoryPath));
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            var configCommand = new GitCommand(repositoryPath, "config")
            {
                new GitCommandItem("get"),
                name,
            };
            configCommand.ThrowOnError = false;
            try
            {
                return configCommand.ReadLine() != null;
            }
            catch
            {
                return false;
            }
        }

        public static string GetValue(string repositoryPath, string name)
        {
            if (repositoryPath == null)
                throw new ArgumentNullException(nameof(repositoryPath));
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            var configCommand = new GitCommand(repositoryPath, "config")
            {
                new GitCommandItem("get"),
                name,
            };
            return configCommand.Run();
        }

        public static void SetValue(string name, string value)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var configCommand = new GitCommand(null, "config")
            {
                GitCommandItem.Global,
                name,
                (GitString)value
            };
            configCommand.Run();
        }

        public static void UnsetValue(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (HasValue(name) == false)
                return;
            var configCommand = new GitCommand(null, "config")
            {
                GitCommandItem.Global,
                new GitCommandItem("unset"),
                name,
            };
            configCommand.Run();
        }

        public static bool HasValue(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            var configCommand = new GitCommand(null, "config")
            {
                GitCommandItem.Global,
                new GitCommandItem("get"),
                name,
            };
            configCommand.ThrowOnError = false;
            return configCommand.ReadLine() != null;
        }

        public static string GetValue(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            var configCommand = new GitCommand(null, "config")
            {
                GitCommandItem.Global,
                new GitCommandItem("get"),
                name,
            };
            return configCommand.Run();
        }

        public static Guid GetValueAsGuid(string name)
        {
            return Guid.Parse(GetValue(name));
        }
    }
}
