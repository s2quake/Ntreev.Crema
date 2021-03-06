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

using JSSoft.Crema.Data;
using JSSoft.Library;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace JSSoft.Crema.Tools.Framework.Controls
{
    public class TagSelector : ButtonBase
    {
        private static readonly Dictionary<string, Color> tagToColor = new();

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(TagInfo?), typeof(TagSelector),
            new UIPropertyMetadata(TagInfo.All, ValuePropertyChangedCallback, ValuePropertyCoerceValueCallback));

        public static readonly DependencyProperty FilterProperty = DependencyProperty.Register("Filter", typeof(TagInfo), typeof(TagSelector),
            new UIPropertyMetadata(TagInfo.All, FilterPropertyChangedCallback));

        public static readonly DependencyProperty IsPopupOpenProperty = DependencyProperty.Register("IsPopupOpen", typeof(bool), typeof(TagSelector),
            new UIPropertyMetadata(false, IsPopupOpenPropertyChangedCallback));

        public static readonly RoutedEvent ValueChangedEvent =
            EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TagSelector));

        private System.Windows.Controls.ContextMenu popup;

        static TagSelector()
        {
            tagToColor.Add("Server", Color.FromArgb(255, 251, 190, 190));
            tagToColor.Add("Client", Color.FromArgb(255, 153, 183, 251));
            tagToColor.Add("Unused", Color.FromArgb(255, 220, 240, 220));
        }

        public TagSelector()
        {

        }

        public TagInfo? Value
        {

/* 'JSSoft.Crema.Tools.Framework (net452)' 프로젝트에서 병합되지 않은 변경 내용
이전:
            get { return (TagInfo?)GetValue(ValueProperty); }
이후:
            get => (TagInfo?)GetValue(ValueProperty); }
*/
            get => (TagInfo?)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public TagInfo Filter
        {

/* 'JSSoft.Crema.Tools.Framework (net452)' 프로젝트에서 병합되지 않은 변경 내용
이전:
            get { return (TagInfo)GetValue(FilterProperty); }
이후:
            get => (TagInfo)GetValue(FilterProperty); }
*/
            get => (TagInfo)GetValue(FilterProperty);
            set => SetValue(FilterProperty, value);
        }

        public bool IsPopupOpen
        {
            get => (bool)GetValue(IsPopupOpenProperty);
            set => SetValue(IsPopupOpenProperty, value);
        }

        public event RoutedEventHandler ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }

        public event EventHandler PopupOpened;

        public event EventHandler PopupClosed;

        protected virtual void OnPopupOpened(EventArgs e)
        {
            this.PopupOpened?.Invoke(this, e);
        }

        protected virtual void OnPopupClosed(EventArgs e)
        {
            this.PopupClosed?.Invoke(this, e);
        }

        protected override void OnClick()
        {
            base.OnClick();

            if (this.popup != null)
            {
                this.popup.IsOpen = false;
            }
            else
            {
                var popup = new System.Windows.Controls.ContextMenu();

                foreach (var item in TagInfoUtility.Names)
                {
                    var menuItem = new System.Windows.Controls.MenuItem() { Header = item, };
                    popup.Items.Add(menuItem);

                    var value = (TagInfo)item;

                    if (tagToColor.ContainsKey(item) == true)
                        menuItem.Background = new SolidColorBrush(tagToColor[item]);

                    menuItem.IsEnabled = (this.Filter & value) == value;
                    menuItem.IsChecked = this.Value == value;
                    menuItem.Click += MenuItem_Click;
                }

                popup.Closed += Popup_Closed;
                popup.PlacementTarget = this;
                popup.Placement = System.Windows.Controls.Primitives.PlacementMode.Right;
                popup.IsOpen = true;
                this.popup = popup;
            }
        }

        private static object ValuePropertyCoerceValueCallback(DependencyObject d, object value)
        {
            if (value == null)
                return value;
            TagInfo tag = (TagInfo)value;
            TagInfo filter = (TagInfo)d.GetValue(TagSelector.FilterProperty);
            return tag & filter;
        }

        private static void ValuePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = d as UIElement;
            element.RaiseEvent(new RoutedEventArgs(ValueChangedEvent));
        }

        private static void FilterPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.SetValue(ValueProperty, d.GetValue(ValueProperty));
        }

        private static void IsPopupOpenPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tagSelector = d as TagSelector;
            if ((bool)e.NewValue == true)
            {
                tagSelector.OnPopupOpened(EventArgs.Empty);
            }
            else
            {
                if (tagSelector.popup != null)
                {
                    tagSelector.popup.IsOpen = false;
                }
            }
        }

        private void Selector_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void Popup_Closed(object sender, RoutedEventArgs e)
        {
            this.popup.Closed -= Popup_Closed;
            this.popup = null;
            this.OnPopupClosed(EventArgs.Empty);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as System.Windows.Controls.MenuItem;
            if (item.IsChecked == true)
                return;

            this.Value = (TagInfo)(item.Header as string);
        }
    }
}
