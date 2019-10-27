using ExpressBase.Mobile.Common.Objects;
using ExpressBase.Mobile.Common.Structures;
using Plugin.Media;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ExpressBase.Mobile.CustomControls
{
    public interface CustomElement
    {
        EbDbTypes DbType { set; get; }
    }

    public class TextBox : Entry, CustomElement
    {
        public TextBox() { }

        public EbDbTypes DbType { set; get; }
    }

    public class XButton : Button
    {
        public XButton() { }
    }

    public class CustomToolBar
    {
        public IList<ToolbarItem> ToolBar { set; get; }

        public CustomToolBar()
        {
            this.ToolBar = new List<ToolbarItem>();
            this.ToolBar.Add(new ToolbarItem
            {
                Text = "item1"
            });
        }
    }

    public class CustomDatePicker : DatePicker, CustomElement
    {
        public EbDbTypes DbType { set; get; }

        public CustomDatePicker()
        {

        }
    }

    public class CustomSelect : Picker, CustomElement
    {
        public EbDbTypes DbType { set; get; }

        public CustomSelect()
        {

        }
    }

    public class FileInput : Element
    {
        public Grid Html
        {
            set { }
            get
            {
                return this._Grid;
            }
        }

        public List<int> Source { set; get; }

        private EbControl Control { set; get; }

        private Page Page { set; get; }

        private Grid _Grid { set; get; }

        public FileInput(EbControl Control, Page CurrentPage)
        {
            this.Control = Control;
            this.Page = CurrentPage;
            this.Source = new List<int>();

            this.BuildHtml();
            this.AppendButtons();
        }

        public void BuildHtml()
        {
            this._Grid = new Grid { BackgroundColor = Color.FromHex("#f2f2f2") };

            this._Grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            this.Html.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            this._Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5, GridUnitType.Star) });
            this._Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5, GridUnitType.Star) });
        }

        public void AppendButtons()
        {
            var FilesBtn = new Button() { Text = "Choose File" };
            this._Grid.Children.Add(FilesBtn, 0, 0);

            var CameraBtn = new Button() { Text = "Take pic" };
            this._Grid.Children.Add(CameraBtn, 1, 0);

            //bind events
            CameraBtn.Clicked += OnCameraClick;
            FilesBtn.Clicked += OnFileClick;
        }

        public async void OnCameraClick(object o,object e)
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await this.Page.DisplayAlert("No Camera", ":( No camera available.", "OK");
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
                this._Grid.Children.Add(_img);
                this._Grid.Children.Add(_img, 0, 1);
                Grid.SetColumnSpan(_img, 2);
            }
        }

        public async void OnFileClick(object o, object e)
        {
            var photo = await CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions() { });

            if (photo != null)
            {
                Image _img = new Image
                {
                    Source = ImageSource.FromStream(() => { return photo.GetStream(); })
                };
                this._Grid.Children.Add(_img);
                this._Grid.Children.Add(_img, 0, 1);
                Grid.SetColumnSpan(_img, 2);
            }
        }
    }
}
