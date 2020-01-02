using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.DynamicRenders;
using ExpressBase.Mobile.Helpers;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VisRender : ContentPage
    {
        public VisRender(EbMobilePage Page)
        {
            InitializeComponent();
            try
            {
                var Renderer = new VisRenderViewModel(Page);
                this.Content = Renderer.View;
                BindingContext = Renderer;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public VisRender(EbMobilePage LinkPage, EbMobileVisualization SourceVis, CustomFrame CustFrame)
        {
            InitializeComponent();
            try
            {
                var Renderer = new VisRenderViewModel(LinkPage, SourceVis, CustFrame);
                BindingContext = Renderer;

                AbsoluteLayout layout = new AbsoluteLayout
                {
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    HorizontalOptions = LayoutOptions.FillAndExpand
                };

                Button btn = new Button
                {
                    BackgroundColor = (Color)HelperFunctions.GetResourceValue("Eb-Blue"),
                    TextColor = Color.White,
                    FontSize = 16,
                    Text = "\uf067",
                    HeightRequest = 60,
                    WidthRequest = 60,
                    CornerRadius = 30,
                    FontFamily = (OnPlatform<string>)HelperFunctions.GetResourceValue("FontAwesome"),
                };

                btn.SetBinding(Button.CommandProperty, new Binding("AddCommand", 0));
                AbsoluteLayout.SetLayoutBounds(btn, new Rectangle(.95, .95, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
                AbsoluteLayout.SetLayoutFlags(btn, AbsoluteLayoutFlags.PositionProportional);

                layout.Children.Add(Renderer.View);
                layout.Children.Add(btn);
                this.Content = layout;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}