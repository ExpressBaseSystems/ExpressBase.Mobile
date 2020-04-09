using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views.Shared
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConfirmBox : ContentView
    {
        public static readonly BindableProperty MessageProperty = BindableProperty.Create(propertyName: "Message", typeof(string), typeof(string), default(string));

        public static readonly BindableProperty TitleProperty = BindableProperty.Create(propertyName: "Title", typeof(string), typeof(string), default(string));

        public static readonly BindableProperty PositionProperty = BindableProperty.Create(propertyName: "Position", typeof(LayoutOptions), typeof(string), LayoutOptions.CenterAndExpand);

        public static readonly BindableProperty CancelClickedProperty = BindableProperty.Create(propertyName: "CancelClicked", typeof(ICommand), typeof(ConfirmBox));

        public static readonly BindableProperty ConfirmClickedProperty = BindableProperty.Create(propertyName: "ConfirmClicked", typeof(ICommand), typeof(ConfirmBox));

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

        public ICommand CancelClicked
        {
            get { return (ICommand)GetValue(CancelClickedProperty); }
            set { SetValue(CancelClickedProperty, value); }
        }

        public ICommand ConfirmClicked
        {
            get { return (ICommand)GetValue(ConfirmClickedProperty); }
            set { SetValue(ConfirmClickedProperty, value); }
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
            if (CancelClicked == null)
                this.Hide();
            else
            {
                if (CancelClicked.CanExecute(null))
                    CancelClicked.Execute(null);
            }
        }

        private void ConfirmButton_Clicked(object sender, EventArgs e)
        {
            if (ConfirmClicked == null)
                this.Hide();
            else
            {
                if (ConfirmClicked.CanExecute(null))
                {
                    this.IsVisible = false;
                    ConfirmClicked.Execute(null);
                }
            }
        }
    }
}