using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Services;
using System;
using System.Collections.Generic;
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
            recognizer.Tapped += OnItemSelected;

            SelectSearchBox.Placeholder = $"Search {powerSelect.Label}...";
            SelectSearchBox.Text = powerSelect.SearchBox.Text;
            searchLength = this.powerSelect.MinSearchLength < 3 ? 3 : this.powerSelect.MinSearchLength;
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();

            SelectSearchBox.Focus();

            if (powerSelect.EnablePreload)
            {
                Loader.IsVisible = true;
                EbDataTable data = await GetData(null, true);
                this.Render(data);
                Loader.IsVisible = false;
            }
        }

        private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
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
            Device.BeginInvokeOnMainThread(() => Loader.IsVisible = true);

            EbDataTable Data = await this.GetData(SelectSearchBox.Text);
            this.Render(Data);

            Device.BeginInvokeOnMainThread(() => Loader.IsVisible = false);
        }

        private void Render(EbDataTable Data)
        {
            int c = 1;
            ResultList.Children.Clear();
            try
            {
                foreach (EbDataRow row in Data.Rows)
                {
                    ComboBoxLabel lbl = new ComboBoxLabel(c)
                    {
                        Padding = new Thickness(10),
                        Text = row[this.powerSelect.DisplayMember.ColumnName]?.ToString(),
                        Value = row[this.powerSelect.ValueMember.ColumnName],
                        Row = row
                    };
                    lbl.GestureRecognizers.Add(recognizer);
                    ResultList.Children.Add(lbl);
                    c++;
                }
            }
            catch (Exception ex)
            {
                EbLog.Error("Failed to Render select ::" + ex.Message);
            }
            EmptyMessage.IsVisible = ResultList.Children.Count <= 0;
        }

        private void OnItemSelected(object sender, EventArgs e)
        {
            powerSelect.SelectionCallback((ComboBoxLabel)sender);
        }

        private async Task<EbDataTable> GetData(string text, bool preload = false)
        {
            EbDataTable dt;
            try
            {
                if (powerSelect.DisplayMember == null)
                    throw new Exception();

                if (powerSelect.NetworkType == NetworkMode.Online)
                    dt = await this.GetLiveData(text, preload);
                else if (powerSelect.NetworkType == NetworkMode.Mixed)
                {
                    dt = await this.GetLiveData(text, preload);
                    if (dt.Rows.Count <= 0)
                        dt = this.GetLocalData(text, preload);
                }
                else
                    dt = this.GetLocalData(text, preload);
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
                dt = new EbDataTable();
            }
            return dt;
        }

        private EbDataTable GetLocalData(string search, bool preload)
        {
            EbDataTable dt;
            string sql = HelperFunctions.B64ToString(powerSelect.OfflineQuery.Code).TrimEnd(';');
            string WrpdQuery;

            if (preload)
                WrpdQuery = $"SELECT * FROM ({sql}) AS WR LIMIT 100;";
            else
                WrpdQuery = $"SELECT * FROM ({sql}) AS WR WHERE WR.{powerSelect.DisplayMember.ColumnName} LIKE '%{search}%' LIMIT 100;";

            try
            {
                dt = App.DataDB.DoQuery(WrpdQuery);
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
                dt = new EbDataTable();
            }

            return dt;
        }

        private async Task<EbDataTable> GetLiveData(string search, bool preload)
        {
            EbDataTable dt;
            try
            {
                if (powerSelect.DisplayMember == null)
                {
                    throw new Exception("Display member cannot be null");
                }

                List<Param> parameters = null;

                if (!preload)
                {
                    parameters = new List<Param>();

                    Param p = new Param
                    {
                        Name = powerSelect.DisplayMember.ColumnName,
                        Type = ((int)powerSelect.DisplayMember.Type).ToString(),
                        Value = search
                    };
                    parameters.Add(p);
                }

                var response = await DataService.Instance.GetDataAsync(powerSelect.DataSourceRefId, 100, 0, parameters, null, null, true);

                if (response.Data != null && response.Data.Tables.HasLength(2))
                    dt = response.Data.Tables[1];
                else
                    throw new Exception();
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
                dt = new EbDataTable();
            }
            return dt;
        }

        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            await App.Navigation.PopModalByRenderer(true);
        }

        private void OnResetButtonClicked(object sender, EventArgs e)
        {
            SelectSearchBox.Text = string.Empty;
            ResultList.Children.Clear();
            SelectSearchBox.Focus();
        }
    }
}