using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SolutionSelect : ContentPage
    {
        public bool Running { set; get; }

        public SolutionSelect()
        {
            InitializeComponent();

            NavigationPage.SetHasNavigationBar(this, false);
        }

        void StoreSidVal(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.Sid.Text))
            {
                Store.SetValue(AppConst.SID, this.Sid.Text.Trim());
                Application.Current.MainPage.Navigation.PushAsync(new Login());
                this.CreateDB(this.Sid.Text.Trim());
            }
        }

        public void CreateDB(string dbName)
        {
            string dpPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), string.Format("{0}.db3",dbName));
            var db = new SQLiteConnection(dpPath);
        }
    }
}