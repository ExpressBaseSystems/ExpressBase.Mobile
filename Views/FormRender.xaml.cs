using ExpressBase.Mobile.Common.Objects;
using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Objects;
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

            Grid OuterGrid = new Grid();
            OuterGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
            OuterGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            ScrollView InnerScroll = new ScrollView { Orientation = ScrollOrientation.Vertical };
            StackLayout ScrollStack = new StackLayout { Spacing = 0 };

            foreach (var ctrl in this.WebForm.Controls)
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
            else if (ctrl is EbDate)
            {
                var tempstack = new StackLayout { Margin = 10 };
                tempstack.Children.Add(new Label { Text = ctrl.Label });
                tempstack.Children.Add(new CustomDatePicker
                {
                    Date = DateTime.Now
                });

                ContentStackTop.Children.Add(tempstack);
            }
            else if (ctrl is EbSimpleSelect)
            {
                var tempstack = new StackLayout { Margin = 10 };
                tempstack.Children.Add(new Label { Text = ctrl.Label });
                var picker = new CustomSelect { Title = "-select-", TitleColor = Color.Red };
                picker.ItemsSource = (ctrl as EbSimpleSelect).Options;
                picker.ItemDisplayBinding = new Binding("Name");
                tempstack.Children.Add(picker);
                ContentStackTop.Children.Add(tempstack);
            }

            else if (ctrl is EbFileUploader)
            {
                var tempstack = new StackLayout { Margin = 10 };
                tempstack.Children.Add(new Label { Text = ctrl.Label });

                Grid _grid = new Grid { BackgroundColor = Color.FromHex("#f2f2f2") };

                _grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                _grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                _grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5, GridUnitType.Star) });
                _grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5, GridUnitType.Star) });

                this.ImplementFUP(_grid);
                tempstack.Children.Add(_grid);
                ContentStackTop.Children.Add(tempstack);
            }
        }

        void ImplementFUP(Grid grid)
        {
            var FilesBtn = new Button() { Text = "Choose File" };
            grid.Children.Add(FilesBtn, 0, 0);

            var CameraBtn = new Button() { Text = "Take pic" };
            grid.Children.Add(CameraBtn, 1, 0);

            CameraBtn.Clicked += async (sender, e) =>
            {
                await CrossMedia.Current.Initialize();

                if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
                {
                    await DisplayAlert("No Camera", ":( No camera available.", "OK");
                    return;
                }

                var photo = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions()
                {
                    AllowCropping = true
                });

                if (photo != null)
                {
                    Image _img = new Image
                    {
                        Source = ImageSource.FromStream(() => { return photo.GetStream(); })
                    };
                    grid.Children.Add(_img);
                    grid.Children.Add(_img, 0, 1);
                    Grid.SetColumnSpan(_img, 2);
                }
            };

            //file btn event
            FilesBtn.Clicked += async (sender, e) =>
            {
                var photo = await CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions() { });

                if (photo != null)
                {
                    Image _img = new Image
                    {
                        Source = ImageSource.FromStream(() => { return photo.GetStream(); })
                    };
                    grid.Children.Add(_img);
                    grid.Children.Add(_img, 0, 1);
                    Grid.SetColumnSpan(_img, 2);
                }
            };
        }

        protected override bool OnBackButtonPressed()
        {
            
            return true;
        }
    }
}