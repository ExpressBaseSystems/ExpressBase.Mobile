using ExpressBase.Mobile.Models;
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
    public partial class FilterView : ContentView
    {
        public static readonly BindableProperty FilterControlsProperty = BindableProperty.Create("FilterControls", typeof(IEnumerable<EbMobileControl>), typeof(FilterView));

        public static readonly BindableProperty SortColumnsProperty = BindableProperty.Create("SortColumns", typeof(IEnumerable<SortColumn>), typeof(FilterView));

        public IEnumerable<EbMobileControl> FilterControls
        {
            get { return (IEnumerable<EbMobileControl>)GetValue(FilterControlsProperty); }
            set { SetValue(FilterControlsProperty, value); }
        }

        public IEnumerable<SortColumn> SortColumns
        {
            get { return (IEnumerable<SortColumn>)GetValue(FilterControlsProperty); }
            set { SetValue(FilterControlsProperty, value); }
        }

        public FilterView()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
        }

        public void Show()
        {
            this.IsVisible = true;
        }

        public void Hide()
        {
            this.IsVisible = false;
        }
    }
}