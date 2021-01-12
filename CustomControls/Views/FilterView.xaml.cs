using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Views.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.CustomControls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FilterView : ContentView
    {
        public static readonly BindableProperty FilterControlsProperty
            = BindableProperty.Create("FilterControls", typeof(IEnumerable<EbMobileControl>), typeof(FilterView));

        public static readonly BindableProperty SortColumnsProperty
            = BindableProperty.Create("SortColumns", typeof(IEnumerable<SortColumn>), typeof(FilterView));

        public static readonly BindableProperty NetWorkTypeProperty
            = BindableProperty.Create("NetWorkType", typeof(NetworkMode), typeof(FilterView));

        public static readonly BindableProperty ConfirmClickedProperty
            = BindableProperty.Create(propertyName: "ConfirmClicked", typeof(ICommand), typeof(FilterView));

        public event ViewOnDisAppearing OnDisAppearing;

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

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            Task.Run(() =>
            {
                this.AppendFilterControls();
            });
        }

        private void AppendFilterControls()
        {
            try
            {
                if (FilterControls.Any())
                    this.FilterContainer.Children.Clear();

                foreach (EbMobileControl ctrl in this.FilterControls)
                {
                    View view = ctrl.Draw(FormMode.NEW, this.NetWorkType);
                    this.FilterContainer.Children.Add(view);
                }
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
        }

        public void Show()
        {
            this.IsVisible = true;
        }

        public void Hide()
        {
            this.IsVisible = false;
            OnDisAppearing?.Invoke();
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
                    List<DbParameter> filters = this.GetFilterValues();
                    this.Hide();
                    ConfirmClicked.Execute(filters);
                }
            }
        }

        private void ClearFilter_Clicked(object sender, EventArgs e)
        {
            FilterControls.ForEach(item => item.Reset());
        }

        public void ClearFilter()
        {
            ClearFilter_Clicked(null, null);
        }
    }
}