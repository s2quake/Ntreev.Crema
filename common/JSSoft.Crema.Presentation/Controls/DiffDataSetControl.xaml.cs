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

using JSSoft.Crema.Data.Diff;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace JSSoft.Crema.Presentation.Controls
{
    /// <summary>
    /// DiffDataSetControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DiffDataSetControl : UserControl
    {
        public readonly static DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(DiffDataSet), typeof(DiffDataSetControl),
            new PropertyMetadata(null, SourcePropertyChangedCallback));

        private readonly static DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(DiffDataSetControl));

        private readonly static DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(DiffDataSetControl),
            new PropertyMetadata(null, SelectedItemPropertyChangedCallback));

        public DiffDataSetControl()
        {
            InitializeComponent();
        }

        public DiffDataSet Source
        {
            get => (DiffDataSet)this.GetValue(SourceProperty);
            set => this.SetValue(SourceProperty, value);
        }

        private static void SelectedItemPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is DependencyObject oldDependency)
            {
                oldDependency.SetValue(ItemViewModel.IsVisibleProperty, false);
            }

            if (e.NewValue is DependencyObject newDependency)
            {
                newDependency.SetValue(ItemViewModel.IsVisibleProperty, true);
            }
        }

        private static void SourcePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is DiffDataSet dataSet)
            {
                var query = from item in dataSet.Tables
                                //where DiffDataSet.GetDiffState(item.DiffTable1) != DiffState.Unchanged
                            select new ItemViewModel(item, d);
                var itemsSource = query.ToArray();
                d.SetValue(ItemsSourceProperty, itemsSource);
                d.SetValue(SelectedItemProperty, itemsSource.FirstOrDefault());
            }
            else
            {
                d.SetValue(ItemsSourceProperty, null);
                d.SetValue(SelectedItemProperty, null);
            }
        }

        protected IEnumerable ItemsSource
        {
            get => (IEnumerable)this.GetValue(ItemsSourceProperty);
            set => this.SetValue(ItemsSourceProperty, value);
        }

        protected object SelectedItem
        {
            get => (object)this.GetValue(SelectedItemProperty);
            set => this.SetValue(SelectedItemProperty, value);
        }

        #region classes

        class ItemViewModel : DependencyObject
        {
            public readonly static DependencyProperty ReadOnlyProperty =
                DependencyProperty.Register("ReadOnly", typeof(bool), typeof(ItemViewModel));

            public readonly static DependencyProperty IsVisibleProperty =
                DependencyProperty.Register("IsVisible", typeof(bool), typeof(ItemViewModel));

            public ItemViewModel(DiffDataTable diffTable, DependencyObject parent)
            {
                this.Source = diffTable;
                this.Parent = parent;
                this.DiffState = DiffUtility.GetDiffState(this.Source.SourceItem1) == DiffState.Imaginary ? DiffState.Inserted : DiffUtility.GetDiffState(this.Source.SourceItem1);
                BindingOperations.SetBinding(this, ReadOnlyProperty, new Binding("ReadOnly") { Source = parent, });
            }

            public override string ToString()
            {
                return this.Source.ToString();
            }

            public DiffState DiffState { get; private set; }

            public DiffDataTable Source { get; private set; }

            public bool ReadOnly
            {
                get => (bool)this.GetValue(ReadOnlyProperty);
                set => this.SetValue(ReadOnlyProperty, value);
            }

            public bool IsVisible
            {
                get => (bool)this.GetValue(IsVisibleProperty);
                set => this.SetValue(IsVisibleProperty, value);
            }

            protected DependencyObject Parent { get; }
        }

        #endregion
    }
}
