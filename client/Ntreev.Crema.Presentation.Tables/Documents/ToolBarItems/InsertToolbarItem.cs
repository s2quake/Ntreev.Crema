using Ntreev.Crema.Presentation.Tables.Documents.ViewModels;
using Ntreev.ModernUI.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ntreev.Crema.Presentation.Tables.Documents.ToolBarItems
{
    //[Export]
    //[Export(typeof(IToolBarItem))]
    //[ParentType(typeof(TableItemViewModel))]
    //class InsertToolbarItem : ToolBarItemBase
    //{
    //    public InsertToolbarItem()
    //    {
    //        this.DisplayName = "Insert...";
    //        this.Icon = "/Ntreev.Crema.Presentation.Tables;component/Images/new.png";
    //    }

    //    protected override async void OnExecute(object parameter)
    //    {
    //        if (parameter is TableItemViewModel viewModel)
    //        {
    //            await viewModel.NewRowAsync();
    //        }
    //    }

    //    protected override bool OnCanExecute(object parameter)
    //    {
    //        if (parameter is TableItemViewModel viewModel)
    //        {
    //            return true;
    //        }
    //        return false;
    //    }
    //}
}
