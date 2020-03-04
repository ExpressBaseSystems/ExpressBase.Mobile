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
        public MyActions(MyActionsResponse actionResp)
        {
            InitializeComponent();
            BindingContext = new MyActionsViewModel(actionResp);
        }

        private async void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            EbMyAction action = e.SelectedItem as EbMyAction;
            await (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(new DoAction(action));
        }
    }
}