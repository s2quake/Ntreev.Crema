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

using System;
using System.Text.RegularExpressions;

namespace Ntreev.Crema.Repository.Git
{
    class GitAuthor
    {
        private const string authorPattern = "(?<name>.+)\\s<(?<email>.*)>";
        private readonly string email;

        public GitAuthor(string author)
        {
            if (author == null)
                throw new ArgumentNullException(nameof(author));
            var match = Regex.Match(author, authorPattern, RegexOptions.ExplicitCapture);
            if (match.Success == true)
            {
                this.Name = match.Groups["name"].Value;
                this.email = match.Groups["email"].Value;
            }
            else
            {
                this.Name = author;
            }
        }

        public string Name { get; }

        public string Email => this.email ?? string.Empty;

        public override string ToString()
        {
            return $"{this.Name} <{this.Email}>";
        }

        public static explicit operator GitAuthor(string author)
        {
            return new GitAuthor(author);
        }
    }
}
