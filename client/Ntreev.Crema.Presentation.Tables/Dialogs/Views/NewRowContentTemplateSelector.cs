using Ntreev.Crema.Data;
using Ntreev.Crema.Presentation.Tables.Dialogs.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Ntreev.Crema.Presentation.Tables.Dialogs.Views
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
                    return (DataTemplate)fe.FindResource("Content_Template");
                }
            }
            return base.SelectTemplate(item, container);
        }
    }
}
