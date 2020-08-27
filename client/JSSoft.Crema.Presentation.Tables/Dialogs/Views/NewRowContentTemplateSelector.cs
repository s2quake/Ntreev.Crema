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

using JSSoft.Crema.Data;
using JSSoft.Crema.Presentation.Tables.Dialogs.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace JSSoft.Crema.Presentation.Tables.Dialogs.Views
{
    class NewRowContentTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (container is FrameworkElement fe)
            {
                if (item is NewRowItemViewModel viewModel)
                {
                    if (CremaDataTypeUtility.IsBaseType(viewModel.DataType) == true)
                    {
                        var dataType = CremaDataTypeUtility.GetType(viewModel.DataType);
                        return (DataTemplate)fe.FindResource(dataType.FullName);
                    }
                    else
                    {
                        if (viewModel.IsFlag == true)
                            return (DataTemplate)fe.FindResource("CremaFlagTypeSelector");
                        return (DataTemplate)fe.FindResource("CremaTypeSelector");
                    }
                }
                else if (item is NewRowParentItemViewModel parentViewModel)
                {
                    return (DataTemplate)fe.FindResource("ParentSelector");
                }
            }
            return base.SelectTemplate(item, container);
        }
    }
}
