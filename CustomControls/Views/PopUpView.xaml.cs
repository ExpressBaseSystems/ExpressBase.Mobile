using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.CustomControls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PopUpView : Grid
    {
        public static readonly BindableProperty ContentProperty =
            BindableProperty.Create(nameof(Content), typeof(View), typeof(PopUpView));

        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(PopUpView), defaultValue: "Title");

        public View Content
        {
            get { return (View)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public PopUpView()
        {
            InitializeComponent();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            Container.Content = Content;
            PopupTitle.Text = Title;
        }

        public void Hide()
        {
            this.IsVisible = false;
            PopupWindowFrame.Scale = 0;
        }

        public void Show()
        {
            this.IsVisible = true;
            PopupWindowFrame.ScaleTo(1, 200);
        }

        private void PopupCloseButtonClicked(object sender, System.EventArgs e)
        {
            this.Hide();
        }
    }
}