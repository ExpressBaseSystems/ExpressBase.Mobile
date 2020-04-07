using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views.Shared
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Loader : ContentView
    {
        public static readonly BindableProperty MessageProperty = BindableProperty.Create(propertyName: "Message",
            returnType: typeof(string),
            declaringType: typeof(string),
            defaultValue: default(string));

        public static readonly BindableProperty PositionProperty = BindableProperty.Create(propertyName: "Position",
            returnType: typeof(LayoutOptions),
            declaringType: typeof(string),
            defaultValue: LayoutOptions.CenterAndExpand);

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public LayoutOptions Position
        {
            get { return (LayoutOptions)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        public Loader()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName == MessageProperty.PropertyName)
            {
                LoaderMessage.Text = Message;
            }
            else if(propertyName == PositionProperty.PropertyName)
            {
                MessageBox.VerticalOptions = Position;
            }
        }
    }
}