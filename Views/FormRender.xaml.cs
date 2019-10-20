using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Objects;
using ExpressBase.Mobile.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FormRender : ContentPage
    {
        public EbWebForm WebForm { set; get; }

        public FormRender(EbObjectWrapper o_wraper)
        {
            InitializeComponent();
            try
            {
                string json_rgexed = EbSerializers.JsonToNETSTD(o_wraper.Json);
                this.WebForm = EbSerializers.Json_Deserialize<EbWebForm>(json_rgexed);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            this.BuildUi();
        }

        void BuildUi()
        {
            this.Title = this.WebForm.DisplayName;
            
            StackLayout OuterStack = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                VerticalOptions = LayoutOptions.FillAndExpand
            };

            StackLayout ContentStackTop = new StackLayout
            {
                VerticalOptions = LayoutOptions.StartAndExpand
            };

            foreach (var ctrl in this.WebForm.Controls)
            {
                if (ctrl is EbTextBox)
                {
                    var tempstack = new StackLayout { Margin = 10 };
                    tempstack.Children.Add(new Label { Text = ctrl.Label });
                    tempstack.Children.Add(new TextBox());
                    ContentStackTop.Children.Add(tempstack);
                }
            }

            StackLayout BottomStack = new StackLayout
            {
                VerticalOptions = LayoutOptions.End
            };

            Button btn = new Button
            {
                Text = "Save",
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.EndAndExpand,
                BackgroundColor = Color.FromHex("#508bf9"),
                TextColor = Color.White
            };

            btn.Clicked += OnSaveClicked;
            BottomStack.Children.Add(btn);

            OuterStack.Children.Add(ContentStackTop);
            OuterStack.Children.Add(BottomStack);

            this.Content = OuterStack;
        }

        public void OnSaveClicked(object sender, EventArgs e)
        {

        }
    }
}