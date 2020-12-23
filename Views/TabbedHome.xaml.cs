using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TabbedHome : Xamarin.Forms.TabbedPage
    {
        public TabbedHome()
        {
            InitializeComponent();
            Initialize();
        }

        public TabbedHome(List<Page> pageCollection)
        {
            InitializeComponent();
            Initialize();

            foreach (var page in pageCollection)
            {
                Children.Add(page);
            }
        }

        void Initialize()
        {
            TitleLabel.Text = App.Settings.CurrentApplication?.AppName;
            On<Android>().SetToolbarPlacement(ToolbarPlacement.Bottom);
        }
    }
}