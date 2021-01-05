using ExpressBase.Mobile.Views.Base;
using System;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.CustomControls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConfirmBox : ContentView
    {
        public static readonly BindableProperty MessageProperty =
            BindableProperty.Create(propertyName: "Message", typeof(string), typeof(string), default(string));

        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(propertyName: "Title", typeof(string), typeof(string), default(string));

        public static readonly BindableProperty PositionProperty =
            BindableProperty.Create(propertyName: "Position", typeof(LayoutOptions), typeof(string), LayoutOptions.CenterAndExpand);

        public static readonly BindableProperty CancelCommandProperty =
            BindableProperty.Create(propertyName: "CancelClicked", typeof(ICommand), typeof(ConfirmBox));

        public static readonly BindableProperty ConfirmCommandProperty =
            BindableProperty.Create(propertyName: "ConfirmClicked", typeof(ICommand), typeof(ConfirmBox));

        public event EbEventHandler ConfirmClicked;

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

        public ICommand CancelCommand
        {
            get { return (ICommand)GetValue(CancelCommandProperty); }
            set { SetValue(CancelCommandProperty, value); }
        }

        public ICommand ConfirmCommand
        {
            get { return (ICommand)GetValue(ConfirmCommandProperty); }
            set { SetValue(ConfirmCommandProperty, value); }
        }

        public ConfirmBox()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName == MessageProperty.PropertyName)
            {
                ConfirmMessage.Text = Message;
            }
            else if (propertyName == PositionProperty.PropertyName)
            {
                ConfirmBoxFrame.VerticalOptions = Position;
            }
            else if (propertyName == TitleProperty.PropertyName)
            {
                ConfirmTitle.Text = Title;
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

        private void CancelButton_Clicked(object sender, EventArgs e)
        {
            if (CancelCommand == null)
                this.Hide();
            else
            {
                if (CancelCommand.CanExecute(null))
                    CancelCommand.Execute(null);
            }
        }

        private void ConfirmButton_Clicked(object sender, EventArgs e)
        {
            this.Hide();

            if (ConfirmCommand != null)
            {
                if (ConfirmCommand.CanExecute(null))
                {
                    ConfirmCommand.Execute(null);
                }
            }
            else if (ConfirmClicked != null)
            {
                ConfirmClicked.Invoke(null, null);
            }
        }
    }
}