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

using JSSoft.Crema.Designer.Properties;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using JSSoft.Library.ObjectModel;
using JSSoft.Crema.Data;
using System.Windows;
using JSSoft.ModernUI.Framework.Dialogs.ViewModels;

namespace JSSoft.Crema.Designer.Types.ViewModels
{
    public class RenameTypeViewModel : RenameViewModel
    {
        private readonly CremaDataType dataType;

        public RenameTypeViewModel(CremaDataType dataType)
            : base(dataType.TypeName)
        {
            this.dataType = dataType;
            this.DisplayName = Resources.Title_RenameType;
        }

        protected override bool VerifyRename(string newName)
        {
            if (CremaDataSet.VerifyName(newName) == false)
                return false;

            if (this.dataType.Name == newName)
                return false;

            var dataSet = this.dataType.DataSet;
            if (dataSet.Types.Contains(newName, this.dataType.CategoryPath) == true)
                return false;

            return true;
        }
    }
}
