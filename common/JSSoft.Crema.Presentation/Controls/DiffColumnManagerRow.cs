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

using JSSoft.Crema.Data.Xml.Schema;
using JSSoft.ModernUI.Framework.DataGrid.Controls;
using System.ComponentModel;
using System.Windows.Data;
using Xceed.Wpf.DataGrid;

namespace JSSoft.Crema.Presentation.Controls
{
    class DiffColumnManagerRow : ModernColumnManagerRow
    {
        public DiffColumnManagerRow()
        {

        }

        protected override Cell CreateCell(ColumnBase column)
        {
            return new DiffColumnManagerCell();
        }

        protected override void PrepareContainer(DataGridContext dataGridContext, object item)
        {
            base.PrepareContainer(dataGridContext, item);

            if (dataGridContext.Items.SourceCollection is not ITypedList typedList)
            {
                var source = (dataGridContext.Items.SourceCollection as CollectionView).SourceCollection;
                typedList = source as ITypedList;
            }

            if (typedList is IBindingListView listView)
            {
                if (listView.SortProperty == null)
                {
                    var indexProp = typedList.GetItemProperties(null)[CremaSchema.Index];

                    if (indexProp != null)
                    {
                        listView.ApplySort(indexProp, ListSortDirection.Ascending);
                    }
                }
            }
        }
    }
}
