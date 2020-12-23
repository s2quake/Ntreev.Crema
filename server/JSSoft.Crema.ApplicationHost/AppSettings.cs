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

using JSSoft.Library.Commands;
using System.ComponentModel;

namespace JSSoft.Crema.ApplicationHost
{
    class AppSettings
    {
        [CommandProperty]
        public string[] PluginsPath
        {
            get; set;
        }

        /// <summary>
        /// </summary>
        [CommandProperty(InitValue = "")]
        public string BasePath
        {
            get; set;
        }

        [CommandProperty]
        public int Port
        {
            get; set;
        }

        [CommandProperty]
        public bool Run
        {
            get; set;
        }

        /// <summary>
        /// light or dark
        /// </summary>
        [CommandProperty(InitValue = "")]
        public string Theme
        {
            get; set;
        }

        /// <summary>
        /// color as #ffffff
        /// </summary>
        [CommandProperty("color", InitValue = "")]
        public string ThemeColor
        {
            get; set;
        }


#if DEBUG
        [CommandProperty(InitValue = "en-US")]
#else
        [CommandProperty(InitValue = "")]
#endif
        public string Culture
        {
            get; set;
        }

        [CommandPropertyArray]
        public string[] DataBases
        {
            get; set;
        }
    }
}