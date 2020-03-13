using ExpressBase.Mobile.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views.Shared
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DataGridView : ContentPage
    {
        public EbMobileDataGrid DataGrid { set; get; }

        public DataGridView()
        {
            InitializeComponent();
        }

        public DataGridView(EbMobileDataGrid dataGrid)
        {
            DataGrid = dataGrid;
            InitializeComponent();
            CreateForm();
        }

        private void CreateForm()
        {
            foreach(var ctrl in this.DataGrid.ChildControls)
            {
                ctrl.InitXControl(this.DataGrid.Mode);
                ControlContainer.Children.Add(ctrl.XView);
            }
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PopModalAsync();
            DataGrid.RowAddCallBack();
        }
    }
}