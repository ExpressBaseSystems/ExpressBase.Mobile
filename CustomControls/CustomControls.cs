using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Structures;
using Plugin.Media;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ExpressBase.Mobile.CustomControls
{
    public interface ICustomElement
    {
        string Name { set; get; }

        EbDbTypes DbType { set; get; }

        object GetValue();

        bool SetValue(object value);

        void SetAsReadOnly(bool Enable);
    }

    public class TextBox : Entry, ICustomElement
    {
        public TextBox() { }

        public TextBox(EbMobileTextBox EbTextBox)
        {
            this.Name = EbTextBox.Name;

            this.DbType = EbTextBox.EbDbType;
        }

        public EbDbTypes DbType { set; get; }

        public string Name { set; get; }

        public object GetValue()
        {
            return this.Text;
        }

        public bool SetValue(object value)
        {
            if (value == null)
                return false;
            this.Text = value.ToString();
            return true;
        }

        public void SetAsReadOnly(bool Enable)
        {
            if (Enable == true)
                this.IsEnabled = false;
            else
                this.IsEnabled = true;
        }
    }

    public class NumericTextBox : Entry, ICustomElement
    {
        public NumericTextBox() { }

        public NumericTextBox(EbMobileNumericBox EbTextBox)
        {

            this.Name = EbTextBox.Name;
            this.DbType = EbTextBox.EbDbType;
            Keyboard = Keyboard.Numeric;
        }

        public EbDbTypes DbType { set; get; }

        public string Name { set; get; }

        public object GetValue()
        {
            return this.Text;
        }

        public bool SetValue(object value)
        {
            if (value == null)
                return false;
            this.Text = value.ToString();
            return true;
        }

        public void SetAsReadOnly(bool Enable)
        {
            if (Enable == true)
                this.IsEnabled = false;
            else
                this.IsEnabled = true;
        }
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

    public class CustomDatePicker : DatePicker, ICustomElement
    {
        public EbDbTypes DbType { set; get; }

        public string Name { set; get; }

        public CustomDatePicker() { }

        public CustomDatePicker(EbMobileDateTime EbDate)
        {
            Date = DateTime.Now;
            Name = EbDate.Name;
            DbType = EbDate.EbDbType;
            Format = "yyyy-MM-dd";
        }

        public object GetValue()
        {
            return this.Date.ToString("yyyy-MM-dd");
        }

        public bool SetValue(object value)
        {
            if (value == null)
                return false;
            this.Date = Convert.ToDateTime(value);
            return true;
        }

        public void SetAsReadOnly(bool Enable)
        {
            if (Enable == true)
                this.IsEnabled = false;
            else
                this.IsEnabled = true;
        }
    }

    public class CustomSelect : Picker, ICustomElement
    {
        public EbDbTypes DbType { set; get; }

        public string Name { set; get; }

        public EbMobileSimpleSelect EbSimpleSelect { set; get; }

        public CustomSelect() { }

        public CustomSelect(EbMobileSimpleSelect EbSelect)
        {
            EbSimpleSelect = EbSelect;

            Title = "Select";
            TitleColor = Color.DarkBlue;
            Name = EbSelect.Name;
            DbType = EbSelect.EbDbType;
            this.ItemsSource = EbSelect.Options;
            this.ItemDisplayBinding = new Binding("DisplayName");
        }

        public object GetValue()
        {
            return (!(this.SelectedItem is EbMobileSSOption opt)) ? null : opt.Value;
        }

        public bool SetValue(object value)
        {
            if (value == null)
                return false;
            this.SelectedItem = this.EbSimpleSelect.Options.Find(i => i.Value == value.ToString());
            return true;
        }

        public void SetAsReadOnly(bool Enable)
        {
            if (Enable == true)
                this.IsEnabled = false;
            else
                this.IsEnabled = true;
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

        private EbMobileFileUpload Control { set; get; }

        private Page Page { set; get; }

        private Grid _Grid { set; get; }

        public FileInput(EbMobileFileUpload Control, Page CurrentPage)
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

        public async void OnCameraClick(object o, object e)
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

    public class CustomCheckBox : CheckBox, ICustomElement
    {
        public EbDbTypes DbType { set; get; }

        public string Name { set; get; }

        public CustomCheckBox() { }

        public CustomCheckBox(EbMobileBoolean EbBool)
        {
            Name = EbBool.Name;
        }

        public object GetValue()
        {
            return this.IsChecked;
        }

        public bool SetValue(object value)
        {
            if (value == null)
                return false;
            int val = Convert.ToInt32(value);
            this.IsChecked = (val == 0) ? false : true;
            return true;
        }

        public void SetAsReadOnly(bool Enable)
        {
            if (Enable == true)
                this.IsEnabled = false;
            else
                this.IsEnabled = true;
        }
    }

    public class CustomFrame : Frame
    {
        public EbDataRow DataRow { set; get; }

        private StackLayout OuterLayout { set; get; }

        public CustomFrame()
        {

        }

        public CustomFrame(EbDataRow _row, Color BgColor)
        {
            this.DataRow = _row;
            this.BackgroundColor = BgColor;
            //this.OuterLayout = new StackLayout { Spacing = 0 };
            ///SetSyncFlag();
        }

        public void SetSyncFlag()
        {
            var _label = new Label { Text = "&#xf013;",
                TextColor = Color.FromHex("cccccc"),
            };

            _label.SetDynamicResource(Label.StyleProperty, "SyncFlag");

            this.OuterLayout.Children.Add(_label);
        }

        public void SetContent(View _View)
        {
            //this.OuterLayout.Children.Add(_View);
            this.Content = _View;
        }
    }
}
