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
        EbMobileControl EbControl { set; get; }

        string Name { set; get; }

        EbDbTypes DbType { set; get; }

        object GetValue();

        bool SetValue(object value);

        void SetAsReadOnly(bool Enable);
    }

    public class TextBox : Entry, ICustomElement
    {
        public EbMobileControl EbControl { set; get; }

        public TextBox() { }

        public TextBox(EbMobileTextBox EbTextBox)
        {
            this.EbControl = EbTextBox;
            this.Name = EbTextBox.Name;
            this.DbType = EbTextBox.EbDbType;
            this.IsVisible = !(this.EbControl.Hidden);
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
        public EbMobileControl EbControl { set; get; }

        public NumericTextBox() { }

        public NumericTextBox(EbMobileNumericBox EbTextBox)
        {
            this.EbControl = EbTextBox;
            this.Name = EbTextBox.Name;
            this.DbType = EbTextBox.EbDbType;
            Keyboard = Keyboard.Numeric;
            this.IsVisible = !(this.EbControl.Hidden);
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
        public EbMobileControl EbControl { set; get; }

        public EbDbTypes DbType { set; get; }

        public string Name { set; get; }

        public CustomDatePicker() { }

        public CustomDatePicker(EbMobileDateTime EbDate)
        {
            EbControl = EbDate;
            Date = DateTime.Now;
            Name = EbDate.Name;
            DbType = EbDate.EbDbType;
            Format = "yyyy-MM-dd";
            this.IsVisible = !(this.EbControl.Hidden);
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
        public EbMobileControl EbControl { set; get; }

        public EbDbTypes DbType { set; get; }

        public string Name { set; get; }

        public CustomSelect() { }

        public CustomSelect(EbMobileSimpleSelect EbSelect)
        {
            EbControl = EbSelect;

            Title = "Select";
            TitleColor = Color.DarkBlue;
            Name = EbSelect.Name;
            DbType = EbSelect.EbDbType;
            this.ItemsSource = EbSelect.Options;
            this.ItemDisplayBinding = new Binding("DisplayName");
            this.IsVisible = !(this.EbControl.Hidden);
        }

        public object GetValue()
        {
            return (!(this.SelectedItem is EbMobileSSOption opt)) ? null : opt.Value;
        }

        public bool SetValue(object value)
        {
            if (value == null)
                return false;
            this.SelectedItem = (this.EbControl as EbMobileSimpleSelect).Options.Find(i => i.Value == value.ToString());
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

        public EbMobileControl EbControl { set; get; }

        private Page Page { set; get; }

        private Grid _Grid { set; get; }

        public FileInput(EbMobileFileUpload Control, Page CurrentPage)
        {
            this.EbControl = Control;
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
        public EbMobileControl EbControl { set; get; }

        public EbDbTypes DbType { set; get; }

        public string Name { set; get; }

        public CustomCheckBox() { }

        public CustomCheckBox(EbMobileBoolean EbBool)
        {
            EbControl = EbBool;
            Name = EbBool.Name;
            this.IsVisible = !(this.EbControl.Hidden);
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
}
