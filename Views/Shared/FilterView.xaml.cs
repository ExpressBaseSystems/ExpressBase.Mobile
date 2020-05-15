using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views.Shared
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FilterView : ContentView
    {
        public static readonly BindableProperty FilterControlsProperty = BindableProperty.Create("FilterControls", typeof(IEnumerable<EbMobileControl>), typeof(FilterView));

        public static readonly BindableProperty SortColumnsProperty = BindableProperty.Create("SortColumns", typeof(IEnumerable<SortColumn>), typeof(FilterView));

        public static readonly BindableProperty NetWorkTypeProperty = BindableProperty.Create("NetWorkType", typeof(NetworkMode), typeof(FilterView));

        public static readonly BindableProperty ConfirmClickedProperty = BindableProperty.Create(propertyName: "ConfirmClicked", typeof(ICommand), typeof(FilterView));

        public IEnumerable<EbMobileControl> FilterControls
        {
            get { return (IEnumerable<EbMobileControl>)GetValue(FilterControlsProperty); }
            set { SetValue(FilterControlsProperty, value); }
        }

        public IEnumerable<SortColumn> SortColumns
        {
            get { return (IEnumerable<SortColumn>)GetValue(SortColumnsProperty); }
            set { SetValue(SortColumnsProperty, value); }
        }

        public NetworkMode NetWorkType
        {
            get { return (NetworkMode)GetValue(NetWorkTypeProperty); }
            set { SetValue(NetWorkTypeProperty, value); }
        }

        public ICommand ConfirmClicked
        {
            get { return (ICommand)GetValue(ConfirmClickedProperty); }
            set { SetValue(ConfirmClickedProperty, value); }
        }

        public FilterView()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if(propertyName == FilterControlsProperty.PropertyName)
            {
                Task.Run(() =>
                {
                    this.AppendFilterControls();
                });
            }
        }

        private void AppendFilterControls()
        {
            try
            {
                foreach (EbMobileControl ctrl in this.FilterControls)
                {
                    Label lbl = new Label { Text = ctrl.Label };

                    ctrl.NetworkType = this.NetWorkType;
                    ctrl.InitXControl(FormMode.NEW);
                    ctrl.XControl.Margin = new Thickness(0, 0, 0, 10);
                    this.FilterContainer.Children.Add(lbl);
                    this.FilterContainer.Children.Add(ctrl.XControl);
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message, LogTypes.EXCEPTION);
            }
        }

        public void Show()
        {
            this.IsVisible = true;
        }

        public void Hide()
        {
            this.IsVisible = false;
        }

        private void FilterButton_Tapped(object sender, EventArgs e)
        {
            FilterTab.IsVisible = true;
            FilterSelection.IsVisible = true;
            SortTab.IsVisible = false;
            SortSelection.IsVisible = false;
        }

        private void SortButton_Tapped(object sender, EventArgs e)
        {
            SortTab.IsVisible = true;
            FilterSelection.IsVisible = false;
            SortSelection.IsVisible = true;
            FilterTab.IsVisible = false;
        }

        private void CancelButton_Clicked(object sender, EventArgs e)
        {
            this.Hide();
        }

        public List<DbParameter> GetFilterValues()
        {
            List<DbParameter> p = new List<DbParameter>();

            foreach (EbMobileControl ctrl in FilterControls)
            {
                object value = ctrl.GetValue();

                if (value != null)
                {
                    p.Add(new DbParameter
                    {
                        DbType = (int)ctrl.EbDbType,
                        ParameterName = ctrl.Name,
                        Value = value
                    });
                }
            }
            return p.Any() ? p : null;
        }

        private void ConfirmButton_Clicked(object sender, EventArgs e)
        {
            if (ConfirmClicked == null)
                this.Hide();
            else
            {
                if (ConfirmClicked.CanExecute(null))
                {
                    this.Hide();
                    ConfirmClicked.Execute(null);
                }
            }
        }
    }
}