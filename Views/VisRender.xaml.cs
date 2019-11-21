using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VisRender : ContentPage
    {
        public EbMobilePage Page { set; get; }

        public VisRender(EbMobilePage page)
        {
            InitializeComponent();
            this.Page = page;
        }
    }
}