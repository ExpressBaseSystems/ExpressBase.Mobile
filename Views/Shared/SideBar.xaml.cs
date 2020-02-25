using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.ViewModels.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views.Shared
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SideBar : ContentPage
    {
        public SideBar()
        {
            InitializeComponent();
            this.BindingContext = new SideBarViewModel();
            SetDp();
        }

        private void SetDp()
        {
            INativeHelper helper = DependencyService.Get<INativeHelper>();
            string sid = Settings.SolutionId;
            try
            {
                var bytes = helper.GetPhoto($"ExpressBase/{sid}/user.png");
                if (bytes != null)
                    UserDp.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
            }
            catch (Exception ex)
            {
                Log.Write("SideBar.SetDp---" + ex.Message);
            }
        }

        private void About_Tapped(object sender, EventArgs e)
        {
            App.RootMaster.IsPresented = false;
            App.RootMaster.Detail.Navigation.PushAsync(new About());
        }
    }
}