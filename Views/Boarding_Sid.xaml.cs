using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
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
    public partial class Boarding_Sid : ContentPage
    {
        public bool Running { set; get; }

        public Boarding_Sid()
        {
            InitializeComponent();
        }

        void StoreSidVal(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.Sid.Text))
            {
                Store.SetValue(Constants.SID, this.Sid.Text.Trim());
                Application.Current.MainPage = new NavigationPage(new Login());
            }
        }
    }
}