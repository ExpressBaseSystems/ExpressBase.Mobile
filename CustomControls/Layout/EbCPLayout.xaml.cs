using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.ViewModels;
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
        private static EbCPLayout instance;

        public static readonly BindableProperty ContentProperty =
            BindableProperty.Create(nameof(Content), typeof(View), typeof(EbCPLayout), propertyChanged: OnContentPropertyChanged);

        public static readonly BindableProperty ToolBarItemsProperty =
            BindableProperty.Create(nameof(ToolBarItems), typeof(View), typeof(EbCPLayout), propertyChanged: OnItemsPropertyChanged);

        public static readonly BindableProperty ToolBarItemsSecondaryProperty =
            BindableProperty.Create(nameof(ToolBarItemsSecondary), typeof(View), typeof(EbCPLayout), propertyChanged: OnItemsSecondaryPropertyChanged);

        public static readonly BindableProperty ToolBarLayoverProperty =
            BindableProperty.Create(nameof(ToolBarLayover), typeof(View), typeof(EbCPLayout), propertyChanged: OnLayoverPropertyChanged);

        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(EbCPLayout), propertyChanged: OnTitlePropertyChanged);

        public static readonly BindableProperty LoaderVisibiltyProperty =
            BindableProperty.Create(nameof(LoaderVisibilty), typeof(bool), typeof(EbCPLayout), defaultValue: false, propertyChanged: OnLoaderVisiblePropertyChanged);

        public static readonly BindableProperty HasBackButtonProperty =
            BindableProperty.Create(nameof(HasBackButton), typeof(bool), typeof(EbCPLayout), defaultValue: true, propertyChanged: OnBackButtonVisiblePropertyChanged);

        public static readonly BindableProperty HasToolBarProperty =
            BindableProperty.Create(nameof(HasToolBar), typeof(bool), typeof(EbCPLayout), defaultValue: true, propertyChanged: OnToolBarVisiblePropertyChanged);

        public static readonly BindableProperty IsMasterPageProperty =
            BindableProperty.Create(nameof(IsMasterPage), typeof(bool), typeof(EbCPLayout), propertyChanged: OnMasterPagePropertyChanged);

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

        public View ToolBarItemsSecondary
        {
            get { return (View)GetValue(ToolBarItemsSecondaryProperty); }
            set { SetValue(ToolBarItemsSecondaryProperty, value); }
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

        public bool IsMasterPage
        {
            get { return (bool)GetValue(IsMasterPageProperty); }
            set { SetValue(IsMasterPageProperty, value); }
        }

        public EbCPLayout()
        {
            InitializeComponent();
            instance = this;
        }

        private static void OnContentPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            EbCPLayout binding = bindable as EbCPLayout;
            binding.Container.Content = (View)newValue;
        }

        private static void OnItemsPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            EbCPLayout binding = bindable as EbCPLayout;
            binding.ToolBarItemsContainer.Content = (View)newValue;
        }

        private static void OnItemsSecondaryPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            EbCPLayout binding = bindable as EbCPLayout;
            binding.SecondaryToolbarContainer.Content = (View)newValue;
            binding.SecondaryToggle.IsVisible = true;
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
            Grid.SetColumnSpan(newView, 3);
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

        private static void OnMasterPagePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            EbCPLayout binding = bindable as EbCPLayout;
            binding.SideBarToggle.IsVisible = Convert.ToBoolean(newValue);
        }

        private async void OnBackButtonClicked(object sender, EventArgs e)
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
            await App.Navigation.PopByRenderer(true);
        }

        public void ShowLoader()
        {
            Loader.IsVisible = true;
        }

        public void HideLoader()
        {
            Loader.IsVisible = false;
        }

        public Loader GetMessageLoader()
        {
            return MessageLoader;
        }

        private void SecondaryToggleClicked(object sender, EventArgs e)
        {
            SecondaryToolbar.IsVisible = true;
            SecondaryToolbar.FadeTo(1, 150);
            SecondaryToolbar.TranslateTo(SecondaryToolbar.TranslationY, 0);
            SecondaryToolBarFade.IsVisible = true;
        }

        private void SecondaryToolbarTapped(object sender, EventArgs e)
        {
            SecondaryToolBarFade.IsVisible = false;
            var fade = new Animation(v => SecondaryToolbar.Opacity = v, 1, 0);
            var translation = new Animation(v => SecondaryToolbar.TranslationX = v, 0, SecondaryToolbar.Width, null, () =>
            {
                SecondaryToolbar.IsVisible = false;
            });
            var parent = new Animation { { 0.5, 1, fade }, { 0, 1, translation } };
            parent.Commit(this, "SecondaryToolbarHide");
        }

        public static void SecondaryItemClicked(string name)
        {
            instance?.SecondaryToolbarTapped(null, null);
            EbLog.Info($"secondary toolbar ite clicked '{name}'");
        }

        public static void Loading(bool show)
        {
            if (instance != null)
            {
                instance.Loader.IsVisible = show;
            }
        }

        private void SideBarToggleClicked(object sender, EventArgs e)
        {
            App.RootMaster.IsPresented = true;
        }

        private async void LogoutConfirmClicked(object sender, EventArgs e)
        {
            await SideBarViewModel.Instance?.Logout();
        }

        public static void ConfirmLogoutAction()
        {
            Page currentPage = App.Navigation.GetCurrentPage();

            if (currentPage is IMasterPage master)
            {
                master.GetCurrentLayout().LogoutDialog.Show();
            }
        }
    }
}