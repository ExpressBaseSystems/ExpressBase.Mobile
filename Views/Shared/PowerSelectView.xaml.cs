using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
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
    public partial class PowerSelectView : ContentPage
    {
        private EbMobileSimpleSelect PowerSelect { set; get; }

        private int SearchLength { set; get; }

        public PowerSelectView()
        {
            InitializeComponent();
        }

        public PowerSelectView(EbMobileSimpleSelect powerSelect)
        {
            InitializeComponent();
            PowerSelect = powerSelect;

            SelectSearchBox.Placeholder = $"Search {powerSelect.Label}...";
            SelectSearchBox.Text = powerSelect.SearchBox.Text;
            SearchLength = PowerSelect.MinSearchLength < 3 ? 3 : PowerSelect.MinSearchLength;
        }

        protected override void OnAppearing()
        {
            SelectSearchBox.Focus();
            base.OnAppearing();
        }

        private void SelectSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SelectSearchBox.Text.Length >= this.SearchLength)
                this.SetData();

        }

        private async void SetData()
        {
            try
            {
                Device.BeginInvokeOnMainThread(() => SearchLoader.IsVisible = true);
                ResultList.Children.Clear();
                EbDataTable Data = await GetData(SelectSearchBox.Text);
                int c = 1;

                foreach (EbDataRow row in Data.Rows)
                {
                    ComboBoxLabel lbl = new ComboBoxLabel(c)
                    {
                        Padding = new Thickness(10),
                        Text = row[this.PowerSelect.DisplayMember.ColumnName].ToString(),
                        Value = row[this.PowerSelect.ValueMember.ColumnName],
                    };

                    var labelTaped = new TapGestureRecognizer();
                    labelTaped.Tapped += LabelTaped_Tapped;

                    lbl.GestureRecognizers.Add(labelTaped);
                    ResultList.Children.Add(lbl);
                    c++;
                }
                Device.BeginInvokeOnMainThread(() => SearchLoader.IsVisible = false);

                if (ResultList.Children.Count <= 0)
                    ResultList.Children.Add(new Label { Text = "No result found.", VerticalOptions = LayoutOptions.CenterAndExpand, HorizontalTextAlignment = TextAlignment.Center });
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }

        private void LabelTaped_Tapped(object sender, EventArgs e)
        {
            PowerSelect.SelectionCallback(sender as ComboBoxLabel);
            //callback
        }

        private async Task<EbDataTable> GetData(string text)
        {
            EbDataTable dt;
            try
            {
                if (PowerSelect.DisplayMember == null)
                    throw new Exception();

                if (PowerSelect.NetworkType == NetworkMode.Online)
                    dt = await this.GetLiveData(text);
                else if (PowerSelect.NetworkType == NetworkMode.Mixed)
                {
                    dt = await this.GetLiveData(text);
                    if (dt.Rows.Count <= 0)
                        dt = this.GetLocalData(text);
                }
                else
                    dt = this.GetLocalData(text);
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
                dt = new EbDataTable();
            }
            return dt;
        }

        private EbDataTable GetLocalData(string search)
        {
            EbDataTable dt;
            try
            {
                string sql = HelperFunctions.B64ToString(PowerSelect.OfflineQuery.Code).TrimEnd(';');

                string WrpdQuery = $"SELECT * FROM ({sql}) AS WR WHERE WR.{PowerSelect.DisplayMember.ColumnName} LIKE '%{search}%';";

                dt = App.DataDB.DoQuery(WrpdQuery);
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
                dt = new EbDataTable();
            }
            return dt;
        }

        private async Task<EbDataTable> GetLiveData(string search)
        {
            EbDataTable dt;
            try
            {
                if (PowerSelect.DisplayMember == null)
                    throw new Exception("Display member cannot be null");

                Param p = new Param
                {
                    Name = PowerSelect.DisplayMember.ColumnName,
                    Type = ((int)PowerSelect.DisplayMember.Type).ToString(),
                    Value = search
                };

                var response = await RestServices.Instance.PullReaderDataAsync(PowerSelect.DataSourceRefId, new List<Param> { p }, 50, 0, true);

                if (response.Data != null && response.Data.Tables.Count >= 2)
                    dt = response.Data.Tables[1];
                else
                    throw new Exception();
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
                dt = new EbDataTable();
            }
            return dt;
        }

        private void BackButton_Clicked(object sender, EventArgs e)
        {
            (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PopModalAsync();
        }
    }
}