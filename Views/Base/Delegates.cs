using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressBase.Mobile.Views.Base
{
    public delegate void ViewOnDisAppearing();

    public delegate bool OnBackButtonPressed(object sender, EventArgs e);

    public delegate void EbEventHandler(object sender, EventArgs e);

    public delegate void DataGridInsertHandler(string name);
}
