using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
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
    public partial class VisRender : ContentPage
    {
        public EbMobilePage Page { set; get; }

        public EbMobileVisualization Vis { set; get; }

        public EbDataTable DataTable { set; get; }

        public VisRender(EbMobilePage page)
        {
            InitializeComponent();
            this.Page = page;
            this.Vis = this.Page.Container as EbMobileVisualization;

            this.Title = this.Page.DisplayName;

            this.GetData();
            this.BuildView();
        }

        void GetData()
        {
            byte[] b = Convert.FromBase64String(Vis.OfflineQuery.Code);
            string sql = System.Text.Encoding.UTF8.GetString(b);
            try
            {
                DataTable = App.DataDB.DoQuery(sql);
            }
            catch (Exception e)
            {
                DataTable = new EbDataTable();
                Console.WriteLine(e.Message);
            }
        }

        void BuildView()
        {
            CustomListView lv = new CustomListView(this.DataTable, this.Vis.DataLayout);
            ScrollView scroll = new ScrollView
            {
                Content = new StackLayout
                {
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    Children = {
                        lv
                    }
                }
            };

            Content = scroll;
        }
    }
}