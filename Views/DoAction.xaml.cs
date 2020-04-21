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
        public DoActionViewModel ViewModel { set; get; }

        public DoAction()
        {
            InitializeComponent();
        }
        public DoAction(EbMyAction action)
        {
            InitializeComponent();
            BindingContext = ViewModel = new DoActionViewModel(action);

            if (action != null && action.StageInfo != null)
                CurrentStage.Text = action.StageInfo.StageName;
        }
    }
}