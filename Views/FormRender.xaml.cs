using ExpressBase.Mobile.Common.Objects;
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
                this.EbCtrlToXamCtrl(ctrl, ContentStackTop);
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

        private void PushFromTableLayout(EbTableLayout TL, StackLayout ContentStackTop)
        {
            foreach (EbTableTd Td in TL.Controls)
            {
                foreach (var ctrl in Td.Controls)
                {
                    this.EbCtrlToXamCtrl(ctrl, ContentStackTop);
                }
            }
        }

        private void EbCtrlToXamCtrl(EbControl ctrl, StackLayout ContentStackTop)
        {
            if (ctrl is EbTextBox)
            {
                var tempstack = new StackLayout { Margin = 10 };
                tempstack.Children.Add(new Label { Text = ctrl.Label });
                tempstack.Children.Add(new TextBox());
                ContentStackTop.Children.Add(tempstack);
            }
            else if (ctrl is EbNumeric)
            {
                var tempstack = new StackLayout { Margin = 10 };
                tempstack.Children.Add(new Label { Text = ctrl.Label });
                tempstack.Children.Add(new TextBox());
                ContentStackTop.Children.Add(tempstack);
            }
            else if (ctrl is EbTableLayout)
            {
                this.PushFromTableLayout((ctrl as EbTableLayout), ContentStackTop);
            }
            else if(ctrl is EbDate)
            {
                var tempstack = new StackLayout { Margin = 10 };
                tempstack.Children.Add(new Label { Text = ctrl.Label });
                tempstack.Children.Add(new CustomDatePicker
                {
                    Date = DateTime.Now
                });

                ContentStackTop.Children.Add(tempstack);
            }
            else if(ctrl is EbSimpleSelect)
            {
                var tempstack = new StackLayout { Margin = 10 };
                tempstack.Children.Add(new Label { Text = ctrl.Label });
                var picker = new CustomSelect { Title = "-select-", TitleColor = Color.Red };
                picker.ItemsSource = (ctrl as EbSimpleSelect).Options;
                picker.SetBinding(Picker.ItemsSourceProperty, "EbSimpleSelectOption");
                picker.ItemDisplayBinding = new Binding("Name");
                tempstack.Children.Add(picker);

                ContentStackTop.Children.Add(tempstack);
            }
        }
    }
}