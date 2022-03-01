using ExpressBase.Mobile.Views.Base;
using System;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.CustomControls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MessageBox : ContentView
    {
        public static readonly BindableProperty MessageProperty =
            BindableProperty.Create(propertyName: "Message", typeof(string), typeof(string), default(string));

        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(propertyName: "Title", typeof(string), typeof(string), default(string));

        public static readonly BindableProperty PositionProperty =
            BindableProperty.Create(propertyName: "Position", typeof(LayoutOptions), typeof(string), LayoutOptions.CenterAndExpand);

        public static readonly BindableProperty OkCommandProperty =
            BindableProperty.Create(propertyName: "OkClicked", typeof(ICommand), typeof(MessageBox));

        public event EbEventHandler OkClicked;

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public LayoutOptions Position
        {
            get { return (LayoutOptions)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        public ICommand OkCommand
        {
            get { return (ICommand)GetValue(OkCommandProperty); }
            set { SetValue(OkCommandProperty, value); }
        }

        public MessageBox()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName == MessageProperty.PropertyName)
            {
                MessageContent.Text = Message;
            }
            else if (propertyName == PositionProperty.PropertyName)
            {
                MessageBoxFrame.VerticalOptions = Position;
            }
            else if (propertyName == TitleProperty.PropertyName)
            {
                MessageTitle.Text = Title;
            }
        }

        public void Show(string title, string message)
        {
            Title = title;
            Message = message;
            CancelButton.IsVisible = false;
            Grid.SetColumn(OkButton, 0);
            Grid.SetColumnSpan(OkButton, 2);
            OkButton.Text = "Ok";
            this.IsVisible = true;
        }

        public void ShowUpdateMessage(string message)
        {
            Title = "Update available";
            Message = message;
            CancelButton.IsVisible = true;
            Grid.SetColumn(OkButton, 1);
            Grid.SetColumnSpan(OkButton, 1);
            OkButton.Text = "Update Now";
            this.IsVisible = true;
        }

        public void Hide()
        {
            this.IsVisible = false;
        }

        private void OkButton_Clicked(object sender, EventArgs e)
        {
            this.Hide();

            if (sender is Button btn && btn.Text == "Update Now")
            {
                Launcher.OpenAsync(new Uri("https://play.google.com/store/apps/details?id=com.expressbase.R_Mad"));
            }
        }

        private void CancelButton_Clicked(object sender, EventArgs e)
        {
            this.Hide();
        }

    }
}