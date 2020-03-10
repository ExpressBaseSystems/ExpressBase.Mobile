using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DoAction : ContentPage
    {
        public DoAction()
        {
            InitializeComponent();
        }
        public DoAction(EbMyAction action)
        {
            InitializeComponent();
            DoActionViewModel vm = new DoActionViewModel(action);
            BindingContext = vm;

            //foreach (Param p in vm.ActionData)
            //{
            //    Grid g = new Grid
            //    {
            //        ColumnDefinitions =
            //        {
            //            new ColumnDefinition{ Width=GridLength.Auto },
            //            new ColumnDefinition{ Width=GridLength.Star },
            //        }
            //    };

            //    g.Children.Add(new Label
            //    {
            //        Style = (Style)HelperFunctions.GetResourceValue("ActionDataColName"),
            //        Text = p.Name
            //    }, 0, 0);

            //    g.Children.Add(new Label
            //    {
            //        Style = (Style)HelperFunctions.GetResourceValue("ActionDataColValue"),
            //        Text = p.Value
            //    }, 1, 0);

            //    ActionDataContainer.Children.Add(g);
            //}
        }
    }
}