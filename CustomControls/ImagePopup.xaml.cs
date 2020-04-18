using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.CustomControls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ImagePopup : ContentView
    {
        public static readonly BindableProperty SourceProperty = BindableProperty.Create("Source", typeof(ImageSource), typeof(ImagePopup));

        public static readonly BindableProperty CloseClickedProperty = BindableProperty.Create("CloseClicked", typeof(ICommand), typeof(ImagePopup));

        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public ICommand CloseClicked
        {
            get { return (ICommand)GetValue(CloseClickedProperty); }
            set { SetValue(CloseClickedProperty, value); }
        }

        public ImagePopup()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName == SourceProperty.PropertyName)
            {
                FullScreenImage.Source = Source;
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

        private void CloseButton_Clicked(object sender, EventArgs e)
        {
            if (CloseClicked == null)
                this.Hide();
            else
            {
                if (CloseClicked.CanExecute(null))
                {
                    this.IsVisible = false;
                    CloseClicked.Execute(null);
                }
            }
        }
    }
}