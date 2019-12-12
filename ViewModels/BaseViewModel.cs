using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace ExpressBase.Mobile.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged, IDisposable
    {
        public string PageTitle { set; get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
