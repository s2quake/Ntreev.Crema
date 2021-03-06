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

using JSSoft.Library;
using System;
using System.Runtime.Serialization;

namespace JSSoft.Crema.ServiceModel.Extensions
{
    public static class DataBaseFlagsExtensions
    {
        public static void Validate(this DataBaseFlags dataBaseFlags)
        {
            if (dataBaseFlags.HasFlag(DataBaseFlags.Loaded) == true && dataBaseFlags.HasFlag(DataBaseFlags.NotLoaded) == true)
                throw new ArgumentException("'DataBaseFlags.Loaded' and 'DataBaseFlags.NotLoaded' cannot be used together.", nameof(dataBaseFlags));
            if (dataBaseFlags.HasFlag(DataBaseFlags.Public) == true && dataBaseFlags.HasFlag(DataBaseFlags.Private) == true)
                throw new ArgumentException("'DataBaseFlags.Public' and 'DataBaseFlags.Private' cannot be used together.", nameof(dataBaseFlags));
            if (dataBaseFlags.HasFlag(DataBaseFlags.Locked) == true && dataBaseFlags.HasFlag(DataBaseFlags.NotLocked) == true)
                throw new ArgumentException("'DataBaseFlags.Locked' and 'DataBaseFlags.NotLocked' cannot be used together.", nameof(dataBaseFlags));
        }

        public static bool Verify(this DataBaseFlags dataBaseFlags)
        {
            if (dataBaseFlags.HasFlag(DataBaseFlags.Loaded) == true && dataBaseFlags.HasFlag(DataBaseFlags.NotLoaded) == true)
                return false;
            if (dataBaseFlags.HasFlag(DataBaseFlags.Public) == true && dataBaseFlags.HasFlag(DataBaseFlags.Private) == true)
                return false;
            if (dataBaseFlags.HasFlag(DataBaseFlags.Locked) == true && dataBaseFlags.HasFlag(DataBaseFlags.NotLocked) == true)
                return false;
            return true;
        }
    }
}
