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

using Ntreev.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ntreev.Crema.Repository.Svn
{
    class SvnCommandItem : CommandOption
    {
        public SvnCommandItem(string name)
            : base(name)
        {
            
        }

        public SvnCommandItem(char name)
            : base(name)
        {
            
        }

        public SvnCommandItem(string name, object value)
            : base(name, value)
        {
            
        }

        public SvnCommandItem(char name, object value)
            : base(name, value)
        {
            
        }

        public static SvnCommandItem FromMessage(string message)
        {
            return new SvnCommandItem('m', (SvnString)message);
        }

        public static SvnCommandItem FromFile(string path)
        {
            return new SvnCommandItem("file", (SvnPath)path);
        }

        public static SvnCommandItem FromUsername(string username)
        {
            return new SvnCommandItem("username", username);
        }

        public static SvnCommandItem FromEncoding(Encoding encoding)
        {
            return new SvnCommandItem("encoding", encoding.HeaderName);
        }

        public static SvnCommandItem FromRevision(string revision)
        {
            return new SvnCommandItem('r', revision);
        }

        public static SvnCommandItem FromMaxCount(int maxCount)
        {
            return new SvnCommandItem('l', maxCount);
        }

        public readonly static SvnCommandItem Force = new SvnCommandItem("force");

        public readonly static SvnCommandItem Recursive = new SvnCommandItem("recursive");

        public readonly static SvnCommandItem Quiet = new SvnCommandItem("quiet");

        public readonly static SvnCommandItem Xml = new SvnCommandItem("xml");

        public readonly static SvnCommandItem Verbose = new SvnCommandItem("verbose");

        public readonly static SvnCommandItem WithAllRevprops = new SvnCommandItem("with-all-revprops");
    }
}
