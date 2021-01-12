using ExpressBase.Mobile.Views.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.CustomControls.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SideBarLink : ContentView
    {
        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(SideBarLink));

        public static readonly BindableProperty IconProperty =
            BindableProperty.Create(nameof(Icon), typeof(string), typeof(SideBarLink));

        public static readonly BindableProperty TextProperty =
            BindableProperty.Create(nameof(Text), typeof(string), typeof(SideBarLink));

        public event EbEventHandler Clicked;

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public string Icon
        {
            get { return (string)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public SideBarLink()
        {
            InitializeComponent();
        }

        protected override void OnBindingContextChanged()
        {
            LinkIcon.Text = Icon;
            LinkText.Text = Text;
        }

        private void OnViewTapped(object sender, EventArgs e)
        {
            App.RootMaster.IsPresented = false;

            if (Command != null)
            {
                if (Command.CanExecute(null))
                {
                    Command.Execute(null);
                }
            }
            else if (Clicked != null)
            {
                Clicked.Invoke(sender, e);
            }
        }
    }
}