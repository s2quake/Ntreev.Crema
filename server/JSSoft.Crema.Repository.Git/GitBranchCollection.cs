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

using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Ntreev.Crema.Repository.Git
{
    class GitBranchCollection : IReadOnlyList<string>
    {
        private readonly List<string> branchList;

        private GitBranchCollection(IEnumerable<string> branchList, string currentBranch)
        {
            this.branchList = new List<string>(branchList);
            this.CurrentBranch = currentBranch;
        }

        public static GitBranchCollection Run(string repositoryPath)
        {
            var listCommand = new GitCommand(repositoryPath, "branch")
            {
                new GitCommandItem("list")
            };
            var lines = listCommand.ReadLines(true);
            var itemList = new List<string>(lines.Length);
            var currentBranch = string.Empty;
            foreach (var line in lines)
            {
                var match = Regex.Match(line, "^(?<current>[*])*\\s*(?<branch>\\S+)");
                if (match.Success == true)
                {
                    var isCurrent = match.Groups["current"].Value == "*";
                    var branchName = match.Groups["branch"].Value;

                    if (isCurrent == true)
                        currentBranch = branchName;
                    itemList.Add(branchName);
                }
            }

            return new GitBranchCollection(itemList, currentBranch);
        }

        public static GitBranchCollection GetRemoteBranches(string repositoryPath)
        {
            var listCommand = new GitCommand(repositoryPath, "branch")
            {
                new GitCommandItem('a')
            };
            var lines = listCommand.ReadLines(true);
            var itemList = new List<string>(lines.Length);
            var currentBranch = string.Empty;
            foreach (var line in lines)
            {
                var match = Regex.Match(line, "remotes/origin/(?<branch>[^/]+)$");
                if (match.Success == true)
                {
                    var branchName = match.Groups["branch"].Value.Trim();
                    itemList.Add(branchName);
                }
            }

            return new GitBranchCollection(itemList, currentBranch);
        }

        public string this[int index] => this.branchList[index];

        public string CurrentBranch { get; }

        public int Count => this.branchList.Count;

        #region IEnumerable

        IEnumerator<string> IEnumerable<string>.GetEnumerator()
        {
            foreach (var item in this.branchList)
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var item in this.branchList)
            {
                yield return item;
            }
        }

        #endregion
    }
}
