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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xceed.Wpf.DataGrid;

namespace JSSoft.Crema.Presentation.Framework.Controls
{
    public class DomainContentPresenter : ContentPresenter
    {
        static DomainContentPresenter()
        {
            //DomainContentPresenter.MinHeightProperty.OverrideMetadata(typeof(DomainContentPresenter), new FrameworkPropertyMetadata(null, new CoerceValueCallback(DomainContentPresenter.CoerceMinHeight)));
            //TextElement.FontFamilyProperty.OverrideMetadata(typeof(DomainContentPresenter), new FrameworkPropertyMetadata(new PropertyChangedCallback(DomainContentPresenter.InvalidateMinHeight)));
            //TextElement.FontSizeProperty.OverrideMetadata(typeof(DomainContentPresenter), new FrameworkPropertyMetadata(new PropertyChangedCallback(DomainContentPresenter.InvalidateMinHeight)));
            //TextElement.FontStretchProperty.OverrideMetadata(typeof(DomainContentPresenter), new FrameworkPropertyMetadata(new PropertyChangedCallback(DomainContentPresenter.InvalidateMinHeight)));
            //TextElement.FontStyleProperty.OverrideMetadata(typeof(DomainContentPresenter), new FrameworkPropertyMetadata(new PropertyChangedCallback(DomainContentPresenter.InvalidateMinHeight)));
            //TextElement.FontWeightProperty.OverrideMetadata(typeof(DomainContentPresenter), new FrameworkPropertyMetadata(new PropertyChangedCallback(DomainContentPresenter.InvalidateMinHeight)));

            m_sContentBinding = new Binding
            {
                RelativeSource = RelativeSource.TemplatedParent,
                Mode = BindingMode.OneWay,
                Path = new PropertyPath(DomainDataCell.DisplayContentProperty)
            };

            m_sContentTemplateBinding = new Binding
            {
                RelativeSource = RelativeSource.TemplatedParent,
                Mode = BindingMode.OneWay,
                Path = new PropertyPath(Cell.CoercedContentTemplateProperty)
            };

            var trimmingBinding = new Binding
            {
                Path = new PropertyPath("(0).(1).(2)",
                Cell.ParentCellProperty,
                Cell.ParentColumnProperty,
                ColumnBase.TextTrimmingProperty),
                Mode = BindingMode.OneWay,
                RelativeSource = new RelativeSource(RelativeSourceMode.Self)
            };

            var wrappingBinding = new Binding
            {
                Path = new PropertyPath("(0).(1).(2)",
                Cell.ParentCellProperty,
                Cell.ParentColumnProperty,
                ColumnBase.TextWrappingProperty),
                Mode = BindingMode.OneWay,
                RelativeSource = new RelativeSource(RelativeSourceMode.Self)
            };

            m_sTextBlockStyle = new Style(typeof(TextBlock));
            m_sTextBlockStyle.Setters.Add(new Setter(TextBlock.TextTrimmingProperty, trimmingBinding));
            m_sTextBlockStyle.Setters.Add(new Setter(TextBlock.TextWrappingProperty, wrappingBinding));
            m_sTextBlockStyle.Seal();
        }

        public DomainContentPresenter()
        {
            this.Resources.Add(typeof(TextBlock), m_sTextBlockStyle);
            this.DataContext = null;

            this.SetCurrentValue(DomainContentPresenter.MinHeightProperty, 0d);
        }

        public override void EndInit()
        {
            base.EndInit();

            BindingOperations.SetBinding(this, DomainContentPresenter.ContentProperty, m_sContentBinding);
            BindingOperations.SetBinding(this, DomainContentPresenter.ContentTemplateProperty, m_sContentTemplateBinding);
        }

        //private static object CoerceMinHeight(DependencyObject sender, object value)
        //{
        //    var self = sender as DomainContentPresenter;
        //    if (self == null)
        //        return value;

        //    return self.CoerceMinHeight(new Thickness(), value);
        //}

        private static void InvalidateMinHeight(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var self = sender as DomainContentPresenter;
            if (self == null)
                return;

            self.CoerceValue(DomainContentPresenter.MinHeightProperty);
        }

        private static readonly Binding m_sContentTemplateBinding;
        private static readonly Binding m_sContentBinding;
        private static readonly Style m_sTextBlockStyle;
    }
}
