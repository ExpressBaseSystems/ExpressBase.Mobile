using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.CustomControls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ImagePopUp : Grid
    {
        public ImagePopUp()
        {
            InitializeComponent();
        }

        private void CloseButtonClicked(object sender, EventArgs e)
        {
            Hide();
        }

        public void Hide()
        {
            this.IsVisible = false;
            FSWindowFrame.Scale = 0;
        }

        public void Show()
        {
            this.IsVisible = true;
            FSWindowFrame.ScaleTo(1, 200);
        }

        public ImagePopUp SetSource(ImageSource source)
        {
            FullScreenImage.Source = source;
            Show();
            return this;
        }
    }
}