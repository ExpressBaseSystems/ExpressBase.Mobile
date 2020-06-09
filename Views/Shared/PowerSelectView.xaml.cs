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
        private readonly EbMobileSimpleSelect powerSelect;

        private readonly int searchLength;

        private readonly TapGestureRecognizer recognizer;

        public PowerSelectView()
        {
            InitializeComponent();
        }

        public PowerSelectView(EbMobileSimpleSelect powerSelect)
        {
            InitializeComponent();
            this.powerSelect = powerSelect;

            recognizer = new TapGestureRecognizer() { NumberOfTapsRequired = 1 };
            recognizer.Tapped += LabelTaped_Tapped;

            SelectSearchBox.Placeholder = $"Search {powerSelect.Label}...";
            SelectSearchBox.Text = powerSelect.SearchBox.Text;
            searchLength = this.powerSelect.MinSearchLength < 3 ? 3 : this.powerSelect.MinSearchLength;
        }

        protected override void OnAppearing()
        {
            SelectSearchBox.Focus();
            base.OnAppearing();
        }

        private async void SelectSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SelectSearchBox.Text.Length > 0)
                ResetSearch.IsVisible = true;
            else
                ResetSearch.IsVisible = false;

            if (SelectSearchBox.Text.Length >= this.searchLength)
                await this.SetData();
        }

        private async Task SetData()
        {
            try
            {
                Device.BeginInvokeOnMainThread(() => SearchLoader.IsVisible = true);
                ResultList.Children.Clear();
                EbDataTable Data = await this.GetData(SelectSearchBox.Text);
                int c = 1;

                foreach (EbDataRow row in Data.Rows)
                {
                    ComboBoxLabel lbl = new ComboBoxLabel(c)
                    {
                        Padding = new Thickness(10),
                        Text = row[this.powerSelect.DisplayMember.ColumnName].ToString(),
                        Value = row[this.powerSelect.ValueMember.ColumnName],
                    };
                    lbl.GestureRecognizers.Add(recognizer);
                    ResultList.Children.Add(lbl);
                    c++;
                }
                Device.BeginInvokeOnMainThread(() => SearchLoader.IsVisible = false);

                if (ResultList.Children.Count <= 0)
                {
                    ResultList.Children.Add(new Label
                    {
                        Text = "No result found.",
                        VerticalOptions = LayoutOptions.CenterAndExpand,
                        HorizontalTextAlignment = TextAlignment.Center
                    });
                }
            }
            catch (Exception ex)
            {
                EbLog.Write(ex.Message);
            }
        }

        private void LabelTaped_Tapped(object sender, EventArgs e)
        {
            powerSelect.SelectionCallback((ComboBoxLabel)sender);
            //callback
        }

        private async Task<EbDataTable> GetData(string text)
        {
            EbDataTable dt;
            try
            {
                if (powerSelect.DisplayMember == null)
                    throw new Exception();

                if (powerSelect.NetworkType == NetworkMode.Online)
                    dt = await this.GetLiveData(text);
                else if (powerSelect.NetworkType == NetworkMode.Mixed)
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
                EbLog.Write(ex.Message);
                dt = new EbDataTable();
            }
            return dt;
        }

        private EbDataTable GetLocalData(string search)
        {
            EbDataTable dt;
            try
            {
                string sql = HelperFunctions.B64ToString(powerSelect.OfflineQuery.Code).TrimEnd(';');

                string WrpdQuery = $"SELECT * FROM ({sql}) AS WR WHERE WR.{powerSelect.DisplayMember.ColumnName} LIKE '%{search}%';";

                dt = App.DataDB.DoQuery(WrpdQuery);
            }
            catch (Exception ex)
            {
                EbLog.Write(ex.Message);
                dt = new EbDataTable();
            }
            return dt;
        }

        private async Task<EbDataTable> GetLiveData(string search)
        {
            EbDataTable dt;
            try
            {
                if (powerSelect.DisplayMember == null)
                    throw new Exception("Display member cannot be null");

                Param p = new Param
                {
                    Name = powerSelect.DisplayMember.ColumnName,
                    Type = ((int)powerSelect.DisplayMember.Type).ToString(),
                    Value = search
                };

                var response = await DataService.Instance.GetDataAsync(powerSelect.DataSourceRefId, new List<Param> { p }, null, 50, 0, true);

                if (response.Data != null && response.Data.Tables.Count >= 2)
                    dt = response.Data.Tables[1];
                else
                    throw new Exception();
            }
            catch (Exception ex)
            {
                EbLog.Write(ex.Message);
                dt = new EbDataTable();
            }
            return dt;
        }

        private void BackButton_Clicked(object sender, EventArgs e)
        {
            (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PopModalAsync();
        }

        private void ResetSearch_Clicked(object sender, EventArgs e)
        {
            SelectSearchBox.Text = string.Empty;
            ResultList.Children.Clear();
        }
    }
}