using ExpressBase.Mobile.Views.Base;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.CustomControls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EbCPLayout : Grid
    {
        public static readonly BindableProperty ContentProperty =
            BindableProperty.Create(nameof(Content), typeof(View), typeof(EbCPLayout), propertyChanged: OnContentPropertyChanged);

        public static readonly BindableProperty ToolBarItemsProperty =
            BindableProperty.Create(nameof(ToolBarItems), typeof(View), typeof(EbCPLayout), propertyChanged: OnItemsPropertyChanged);

        public static readonly BindableProperty ToolBarLayoverProperty =
            BindableProperty.Create(nameof(ToolBarItems), typeof(View), typeof(EbCPLayout), propertyChanged: OnLayoverPropertyChanged);

        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(EbCPLayout), propertyChanged: OnTitlePropertyChanged);

        public static readonly BindableProperty LoaderVisibiltyProperty =
            BindableProperty.Create(nameof(LoaderVisibilty), typeof(bool), typeof(EbCPLayout), defaultValue: false, propertyChanged: OnLoaderVisiblePropertyChanged);

        public static readonly BindableProperty HasBackButtonProperty =
            BindableProperty.Create(nameof(HasBackButton), typeof(bool), typeof(EbCPLayout), defaultValue: true, propertyChanged: OnBackButtonVisiblePropertyChanged);

        public static readonly BindableProperty HasToolBarProperty =
            BindableProperty.Create(nameof(HasToolBar), typeof(bool), typeof(EbCPLayout), defaultValue: true, propertyChanged: OnToolBarVisiblePropertyChanged);

        public event OnBackButtonPressed BackButtonPressed;

        public View Content
        {
            get { return (View)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        public View ToolBarItems
        {
            get { return (View)GetValue(ToolBarItemsProperty); }
            set { SetValue(ToolBarItemsProperty, value); }
        }

        public View ToolBarLayover
        {
            get { return (View)GetValue(ToolBarLayoverProperty); }
            set { SetValue(ToolBarLayoverProperty, value); }
        }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public bool LoaderVisibilty
        {
            get { return (bool)GetValue(LoaderVisibiltyProperty); }
            set { SetValue(LoaderVisibiltyProperty, value); }
        }

        public bool HasBackButton
        {
            get { return (bool)GetValue(HasBackButtonProperty); }
            set { SetValue(HasBackButtonProperty, value); }
        }

        public bool HasToolBar
        {
            get { return (bool)GetValue(HasToolBarProperty); }
            set { SetValue(HasToolBarProperty, value); }
        }

        public EbCPLayout()
        {
            InitializeComponent();
        }

        private static void OnContentPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            EbCPLayout binding = bindable as EbCPLayout;
            View newView = (View)newValue;
            binding.Container.Children.Add(newView);
        }

        private static void OnItemsPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            EbCPLayout binding = bindable as EbCPLayout;
            View newView = (View)newValue;

            binding.Children.Add(newView, 2, 0);
        }

        private static void OnTitlePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            EbCPLayout binding = bindable as EbCPLayout;
            string title = newValue?.ToString();
            binding.TitleLabel.Text = title;
        }

        private static void OnLayoverPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            EbCPLayout binding = bindable as EbCPLayout;
            View newView = (View)newValue;

            binding.Children.Add(newView);
            Grid.SetRow(newView, 0);
            Grid.SetColumn(newView, 1);
            Grid.SetColumnSpan(newView, 2);
        }

        private static void OnLoaderVisiblePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            EbCPLayout binding = bindable as EbCPLayout;
            binding.Loader.IsVisible = Convert.ToBoolean(newValue);
        }

        private static void OnBackButtonVisiblePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            EbCPLayout binding = bindable as EbCPLayout;
            binding.BackButton.IsVisible = Convert.ToBoolean(newValue);
        }

        private static void OnToolBarVisiblePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            EbCPLayout binding = bindable as EbCPLayout;
            bool flag = Convert.ToBoolean(newValue);
            if (!flag)
                binding.HeaderRow.Height = 0;
            else
                binding.HeaderRow.Height = GridLength.Auto;
        }

        private async void BackButton_Clicked(object sender, EventArgs e)
        {
            if (BackButtonPressed != null)
            {
                bool popout = BackButtonPressed.Invoke(sender, e);
                if (popout) await PopNavigationAsync();
            }
            else
                await PopNavigationAsync();
        }

        private async Task PopNavigationAsync()
        {
            if (App.RootMaster != null)
                await App.Navigation.PopMasterAsync(true);
            else
                await App.Navigation.PopAsync(true);
        }

        public void ShowLoader()
        {
            Loader.IsVisible = true;
        }

        public void HideLoader()
        {
            Loader.IsVisible = false;
        }
    }
}