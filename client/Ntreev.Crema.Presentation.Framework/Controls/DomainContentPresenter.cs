﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using Xceed.Wpf.DataGrid;

namespace Ntreev.Crema.Presentation.Framework.Controls
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

            m_sContentBinding = new Binding();
            m_sContentBinding.RelativeSource = RelativeSource.TemplatedParent;
            m_sContentBinding.Mode = BindingMode.OneWay;
            m_sContentBinding.Path = new PropertyPath(DomainDataCell.DisplayContentProperty);

            m_sContentTemplateBinding = new Binding();
            m_sContentTemplateBinding.RelativeSource = RelativeSource.TemplatedParent;
            m_sContentTemplateBinding.Mode = BindingMode.OneWay;
            m_sContentTemplateBinding.Path = new PropertyPath(Cell.CoercedContentTemplateProperty);

            Binding trimmingBinding = new Binding();
            trimmingBinding.Path = new PropertyPath("(0).(1).(2)",
              Cell.ParentCellProperty,
              Cell.ParentColumnProperty,
              ColumnBase.TextTrimmingProperty);
            trimmingBinding.Mode = BindingMode.OneWay;
            trimmingBinding.RelativeSource = new RelativeSource(RelativeSourceMode.Self);

            Binding wrappingBinding = new Binding();
            wrappingBinding.Path = new PropertyPath("(0).(1).(2)",
              Cell.ParentCellProperty,
              Cell.ParentColumnProperty,
              ColumnBase.TextWrappingProperty);
            wrappingBinding.Mode = BindingMode.OneWay;
            wrappingBinding.RelativeSource = new RelativeSource(RelativeSourceMode.Self);

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

        private static Binding m_sContentTemplateBinding;
        private static Binding m_sContentBinding;
        private static Style m_sTextBlockStyle;
    }
}