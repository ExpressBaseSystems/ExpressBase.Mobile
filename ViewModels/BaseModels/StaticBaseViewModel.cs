using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace ExpressBase.Mobile.ViewModels.BaseModels
{
    public class StaticBaseViewModel : BaseViewModel
    {
        public StaticBaseViewModel() { }

        public StaticBaseViewModel(string title)
        {
            this.PageTitle = title;
        }
    }
}
