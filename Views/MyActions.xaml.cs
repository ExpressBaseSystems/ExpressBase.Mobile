using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MyActions : ContentPage
    {
        public MyActionsViewModel ViewModel { set; get; }

        public MyActions(MyActionsResponse actionResp)
        {
            InitializeComponent();
            BindingContext = ViewModel = new MyActionsViewModel(actionResp);
            if (!ViewModel.Actions.Any())
            {

            }
        }
    }
}