using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using Plugin.Media;
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
        public EbMobilePage Page { set; get; }

        public EbMobileForm Form { set; get; }

        public IList<Element> Elements { set; get; }

        public FormRender(EbMobilePage page)
        {
            InitializeComponent();
            this.Elements = new List<Element>();
            try
            {
                this.Page = page;
                this.Form = (page.Container as EbMobileForm);
                this.BuildUi();

                CommonServices MyService = new CommonServices();
                MyService.CreateLocalTable4Form(this.Form);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        void BuildUi()
        {
            this.Title = this.Page.DisplayName;

            Grid OuterGrid = new Grid();
            OuterGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
            OuterGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            ScrollView InnerScroll = new ScrollView { Orientation = ScrollOrientation.Vertical };
            StackLayout ScrollStack = new StackLayout { Spacing = 0 };

            foreach (var ctrl in this.Form.ChiledControls)
            {
                this.EbCtrlToXamCtrl(ctrl, ScrollStack);
            }

            InnerScroll.Content = ScrollStack;
            OuterGrid.Children.Add(InnerScroll);

            Button btn = new Button
            {
                Text = "Save",
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.EndAndExpand,
                BackgroundColor = Color.FromHex("#508bf9"),
                TextColor = Color.White,
            };
            btn.Clicked += OnSaveClicked;
            OuterGrid.Children.Add(btn);
            Grid.SetRow(btn, 1);

            this.Content = OuterGrid;
        }

        private void PushFromTableLayout(EbMobileTableLayout TL, StackLayout ContentStackTop)
        {
            foreach (EbMobileTableCell Tc in TL.CellCollection)
            {
                foreach (var ctrl in Tc.ControlCollection)
                {
                    this.EbCtrlToXamCtrl(ctrl, ContentStackTop);
                }
            }
        }

        private void EbCtrlToXamCtrl(EbMobileControl ctrl, StackLayout ContentStackTop, int Margin = 10)
        {
            var tempstack = new StackLayout { Margin = Margin };
            tempstack.Children.Add(new Label { Text = ctrl.Label });

            if (ctrl is EbMobileTableLayout)
            {
                this.PushFromTableLayout((ctrl as EbMobileTableLayout), ContentStackTop);
            }
            else if (ctrl is EbMobileFileUpload)
            {
                FileInput uploader = new FileInput((ctrl as EbMobileFileUpload), this);
                tempstack.Children.Add(uploader.Html);
                this.Elements.Add(uploader);
            }
            else
            {
                var el = (View)Activator.CreateInstance(ctrl.XControlType,ctrl);
                tempstack.Children.Add(el);
                this.Elements.Add(el);
            }

            ContentStackTop.Children.Add(tempstack);
        }

        protected override bool OnBackButtonPressed()
        {

            return false;
        }

        public void OnSaveClicked(object sender, EventArgs e)
        {
            FormService Form = new FormService(this.Elements, this.Form);
            bool status = Form.Save();
            if (status)
            {
                DependencyService.Get<IToast>().Show("Data pushed successfully :)");
                (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PopAsync(true);
            }
        }
    }
}