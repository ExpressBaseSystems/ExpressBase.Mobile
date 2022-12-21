using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ExpressBase.Mobile.Views.Base
{
    public delegate void ViewOnDisAppearing();

    public delegate bool OnBackButtonPressed(object sender, EventArgs e);

    public delegate void EbEventHandler(object sender, EventArgs e);

    public delegate void DataGridInsertHandler(string name);

    public delegate void SignPadDoneHandler(Stream imageStream);
}
