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
using System.IO;

namespace Ntreev.Crema.Repository.Svn
{
    class SvnPath
    {
        private readonly string path;

        /// <summary>
        /// "C:\test\" 와 같은 문자열을 Process의 인수로 넘겨질때 마지막의 \"가 escape가 되어서 "C:\test\ 처럼 넘겨진다.
        /// 결과적으로 잘못된 문자열로 인해 에러가 발생함.
        /// </summary>
        public SvnPath(string path)
        {
            this.path = path;
            if (this.path.EndsWith($"{Path.DirectorySeparatorChar}") == true)
                this.path = this.path.TrimEnd(Path.DirectorySeparatorChar);
            if (this.path.EndsWith($"{Path.AltDirectorySeparatorChar}") == true)
                this.path = this.path.TrimEnd(Path.AltDirectorySeparatorChar);
        }

        public SvnPath(Uri uri)
            : this(uri.ToString())
        {

        }

        public override string ToString()
        {
            return $"\"{this.path}\"";
        }

        public static explicit operator SvnPath(string path)
        {
            return new SvnPath(path);
        }

        public static explicit operator SvnPath(Uri uri)
        {
            return new SvnPath(uri);
        }
    }
}
