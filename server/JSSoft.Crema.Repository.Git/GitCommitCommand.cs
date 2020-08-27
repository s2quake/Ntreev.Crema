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

namespace JSSoft.Crema.Repository.Git
{
    class GitCommitCommand : GitCommand
    {
        private readonly GitAuthor author;
        private readonly string basePath;

        public GitCommitCommand(string basePath, string author, string message)
            : base(basePath, "commit")
        {
            this.author = new GitAuthor(author);
            this.basePath = basePath;
            this.Add(new GitCommandItem('a'));
            this.Add(GitCommandItem.FromMessage(message));
        }

        protected override void OnRun()
        {
            GitConfig.SetValue(this.basePath, "user.email", this.author.Email == string.Empty ? "<>" : this.author.Email);
            GitConfig.SetValue(this.basePath, "user.name", this.author.Name);
            base.OnRun();
        }
    }
}
