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

using JSSoft.Crema.Data.Diff;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace JSSoft.Crema.Presentation.Controls.Actions
{
    class CopyItemAction : ActionBase
    {
        private readonly ItemInfo item;

        public CopyItemAction(object dataItem, object destItem)
            : this(dataItem, destItem, false)
        {

        }

        public CopyItemAction(object dataItem, object destItem, bool recursive)
        {
            this.item = new ItemInfo(dataItem, destItem);
        }

        protected override void OnRedo()
        {
            base.OnRedo();
            this.item.Redo();
        }

        protected override void OnUndo()
        {
            base.OnUndo();
            this.item.Undo();
        }

        #region classes

        class ItemInfo
        {
            private readonly object destItem;
            private readonly List<ItemInfo> childList = new();

            public ItemInfo(object dataItem, object destItem)
            {
                this.Item = dataItem;
                this.destItem = destItem;
                this.Fields = DiffUtility.GetFields(destItem);

                var dataChildItems = new List<object>();
                var destChildItems = new List<object>();
                foreach (var item in DiffUtility.GetChilds(dataItem))
                {
                    dataChildItems.Add(item);
                }
                foreach (var item in DiffUtility.GetChilds(destItem))
                {
                    destChildItems.Add(item);
                }

                var query = from dataChild in dataChildItems
                            join destChild in destChildItems
                            on $"{DiffUtility.GetListName(dataChild)}{DiffUtility.GetItemKey(dataChild)}"
                            equals $"{DiffUtility.GetListName(destChild)}{DiffUtility.GetItemKey(destChild)}"
                            select new { DataItem = dataChild, DestItem = destChild };

                foreach (var item in query)
                {
                    this.childList.Add(new ItemInfo(item.DataItem, item.DestItem));
                }
            }

            public object Item { get; private set; }

            public object[] Fields { get; private set; }

            public void Redo()
            {
                DiffUtility.Copy(this.Item, this.destItem);
                foreach (var item in this.childList)
                {
                    item.Redo();
                }
            }

            public void Undo()
            {
                DiffUtility.Copy(this.destItem, this.Fields);
                foreach (var item in this.childList)
                {
                    item.Undo();
                }
            }

            private void Obj_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == string.Empty)
                {
                    this.Fields = DiffUtility.GetFields(this.Item);
                }
            }
        }

        #endregion
    }
}
