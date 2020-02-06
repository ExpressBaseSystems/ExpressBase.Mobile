using ExpressBase.Mobile.ViewModels.Dynamic;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Collections.Generic;
using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using System.Linq;

namespace ExpressBase.Mobile.Views.Dynamic
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ListViewRender : ContentPage
    {
        public ListViewRenderViewModel Renderer { set; get; }

        public ListViewRender(EbMobilePage Page)
        {
            InitializeComponent();
            try
            {
                Renderer = new ListViewRenderViewModel(Page);

                listContainer.Content = Renderer.View;

                if (Renderer.FilterDialog != null)
                    FilterContainer.Content = Renderer.FilterDialog;
                BindingContext = Renderer;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void FilterButton_Clicked(object sender, EventArgs e)
        {
            if(Renderer != null)
            {
                FilterDialogView.IsVisible = Renderer.FilterDialog == null ? false : true;
            }
        }

        private void FDCancel_Clicked(object sender, EventArgs e)
        {
            FilterDialogView.IsVisible = false;
        }

        private void FDApply_Clicked(object sender, EventArgs e)
        {
            try
            {
                var paramDict = new Dictionary<string, object>();

                foreach (KeyValuePair<string, View> pair in Renderer.FilterControls)
                {
                    if (paramDict.ContainsKey(pair.Key))
                        continue;
                    var ctrlValue = GetControlValue(pair.Value);
                    if (ctrlValue != null)
                        paramDict.Add(pair.Key, ctrlValue);
                }

                FilterDialogView.IsVisible = false;

                if (paramDict.Any())
                {
                    List<DbParameter> parameters = paramDict.Select(item => new DbParameter { ParameterName = item.Key, Value = item.Value }).ToList();
                    Renderer.Refresh(parameters);
                    listContainer.Content = Renderer.View;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private object GetControlValue(View ctrl)
        {
            if (ctrl is TextBox)
                return (ctrl as TextBox).Text;
            else if (ctrl is CustomDatePicker)
                return (ctrl as CustomDatePicker).Date;
            else if (ctrl is CustomCheckBox)
                return (ctrl as CustomCheckBox).IsChecked;
            else
                return null;
        }
    }
}