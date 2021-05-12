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

using Caliburn.Micro;
using JSSoft.Crema.Reader;
using System.Collections;
using System.Data;
using System.Threading.Tasks;

namespace JSSoft.Crema.Tools.View.ViewModels
{
    class ItemViewModel : PropertyChangedBase
    {
        private readonly ITable table;
        private IEnumerable itemsSource;

        public ItemViewModel(ITable table)
        {
            this.table = table;
        }

        public override string ToString()
        {
            return this.Name;
        }


/* 'JSSoft.Crema.Tools.View (net452)' 프로젝트에서 병합되지 않은 변경 내용
이전:
        public string Name
        {
            get { return this.table.Name; }
이후:
        public string Name => this.table.Name; }
*/
        public string Name => this.table.Name;

        public IEnumerable ItemsSource
        {
            get
            {
                if (this.itemsSource == null)
                {
                    Task.Run(() => this.Initialize());
                }

                return this.itemsSource;
            }
        }

        private void Initialize()
        {
            var table = new DataTable();

            foreach (var item in this.table.Columns)
            {
                table.Columns.Add(item.Name);
            }

            foreach (var item in this.table.Rows)
            {
                var row = table.NewRow();
                foreach (var c in this.table.Columns)
                {
                    var value = item[c];
                    if (value != null)
                        row[c.Name] = value.ToString();
                }
                table.Rows.Add(row);
            }

            this.itemsSource = table.DefaultView;
            this.NotifyOfPropertyChange(() => this.ItemsSource);
        }
    }
}
