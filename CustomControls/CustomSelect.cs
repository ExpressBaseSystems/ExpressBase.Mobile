using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Structures;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ExpressBase.Mobile.CustomControls
{

    public class PowerSelect : StackLayout
    {
        public EbMobileControl EbControl { set; get; }

        public View XControl { set; get; }

        public PowerSelect(EbMobileControl EbSelect)
        {
            var _Select = EbSelect as EbMobileSimpleSelect;

            if (string.IsNullOrEmpty(_Select.DataSourceRefId))
            {
                XControl = new CustomPicker(_Select);
            }
            else
            {
                XControl = new ComboBox(_Select);
            }

            this.Children.Add(XControl);
        }
    }

    public class ComboBox : StackLayout, ICustomElement
    {
        public EbMobileControl EbControl { get; set; }

        public string Name { get; set; }

        public EbDbTypes DbType { get; set; }

        public TextBox SearchBox { set; get; }

        public Frame ResultList { set; get; }

        private StackLayout ResultListStack { set; get; }

        public ComboBox(EbMobileSimpleSelect EbSelect)
        {
            EbControl = EbSelect;

            SearchBox = new TextBox();
            SearchBox.Unfocused += SearchBox_Unfocused;

            ResultListStack = new StackLayout { Spacing = 0 };
            ResultList = new Frame
            {
                IsVisible = false,
                HasShadow = true,
                CornerRadius = 0,
                Padding = 5,
                Content = ResultListStack
            };

            this.Children.Add(SearchBox);
            this.Children.Add(ResultList);
        }

        private void SearchBox_Unfocused(object sender, EventArgs e)
        {
            if (SearchBox.Text.Length > 3)
            {
                EbDataTable Data = GetData(SearchBox.Text);

                foreach (EbDataRow row in Data.Rows)
                {
                    string displayVal = row[(EbControl as EbMobileSimpleSelect).DisplayMember.ColumnName].ToString();

                    if (displayVal.Contains(SearchBox.Text))
                    {
                        ComboBoxLabel lbl = new ComboBoxLabel
                        {
                            Text = displayVal,
                            Value = row[(EbControl as EbMobileSimpleSelect).ValueMember.ColumnName],
                        };

                        var labelTaped = new TapGestureRecognizer();
                        labelTaped.Tapped += LabelTaped_Tapped;

                        lbl.GestureRecognizers.Add(labelTaped);
                        ResultListStack.Children.Add(lbl);
                    }
                }
                ResultList.IsVisible = true;
            }
        }

        private void LabelTaped_Tapped(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private EbDataTable GetData(string text)
        {
            try
            {
                EbMobileDataColumn DisplayMember = (EbControl as EbMobileSimpleSelect).DisplayMember;
                if (DisplayMember == null)
                {
                    throw new Exception();
                }

                byte[] b = Convert.FromBase64String((this.EbControl as EbMobileSimpleSelect).OfflineQuery.Code);
                string sql = System.Text.Encoding.UTF8.GetString(b).TrimEnd(';');

                string WrpdQuery = $"SELECT * FROM ({sql}) AS WR WHERE WR.{DisplayMember.ColumnName} LIKE '%{text}%';";

                return App.DataDB.DoQuery(WrpdQuery);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new EbDataTable();
            }
        }

        public object GetValue()
        {
            throw new NotImplementedException();
        }

        public void SetAsReadOnly(bool Enable)
        {
            throw new NotImplementedException();
        }

        public bool SetValue(object value)
        {
            throw new NotImplementedException();
        }
    }

    public class ComboBoxLabel : Label
    {
        public object Value { set; get; }
    }

    public class CustomPicker : Picker, ICustomElement
    {
        public EbMobileControl EbControl { set; get; }

        public EbDbTypes DbType { set; get; }

        public string Name { set; get; }

        public CustomPicker() { }

        public CustomPicker(EbMobileSimpleSelect EbSelect)
        {
            EbControl = EbSelect;

            Title = "Select";
            TitleColor = Color.DarkBlue;
            ItemsSource = EbSelect.Options;
            ItemDisplayBinding = new Binding("DisplayName");
            IsVisible = !(this.EbControl.Hidden);
            Name = EbSelect.Name;
            DbType = EbSelect.EbDbType;
        }

        public object GetValue()
        {
            return (this.SelectedItem as EbMobileSSOption).Value;
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
}
