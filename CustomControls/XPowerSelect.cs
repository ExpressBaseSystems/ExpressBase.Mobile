using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Structures;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ExpressBase.Mobile.CustomControls
{
    public class XPowerSelect : XCustomControl
    {
        public CustomSearchBar SearchBox { set; get; }

        public StackLayout ResultStack { set; get; }

        public Frame ResultFrame { set; get; }

        public int Mode { set; get; }

        public ComboBoxLabel Selected { set; get; }

        public XPowerSelect() { }

        public XPowerSelect(EbMobileControl Ctrl)
        {
            this.Name = Ctrl.Name;
            this.DbType = Ctrl.EbDbType;
            this.EbControl = Ctrl;

            var _Select = Ctrl as EbMobileSimpleSelect;

            if (string.IsNullOrEmpty(_Select.DataSourceRefId))
            {
                Mode = 0;//static
                XControl = this.GetPicker(_Select);
            }
            else
            {
                Mode = 1;//dynamic
                XControl = this.GetComboBox();
            }
        }

        public override object GetValue()
        {
            if (Mode == 0)
            {
                if((this.XControl as CustomPicker).SelectedItem != null)
                {
                    return ((this.XControl as CustomPicker).SelectedItem as EbMobileSSOption).Value;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return (Selected == null) ? null : Selected.Value;
            }
        }

        public override bool SetValue(object value)
        {
            if (value == null)
                return false;

            if (Mode == 0)
            {
                (this.XControl as CustomPicker).SelectedItem = (this.EbControl as EbMobileSimpleSelect).Options.Find(i => i.Value == value.ToString());
            }
            else
            {

            }

            return true;
        }

        public StackLayout GetComboBox()
        {
            StackLayout Stack = new StackLayout();

            ResultStack = new StackLayout();

            ScrollView Scroll = new ScrollView
            {
                Content = ResultStack
            };

            SearchBox = new CustomSearchBar
            {
                Placeholder = "Seach " + EbControl.Label + "...",
                FontSize = 14
            };
            SearchBox.TextChanged += SearchBox_TextChanged;

            ResultFrame = new Frame()
            {
                BorderColor = Color.FromHex("315eff"),
                IsVisible = false,
                HasShadow = true,
                CornerRadius = 10.0f,
                Padding = 5,
                Content = Scroll,
            };

            Stack.Children.Add(SearchBox);
            Stack.Children.Add(ResultFrame);

            return Stack;
        }

        public CustomPicker GetPicker(EbMobileSimpleSelect EbSelect)
        {
            CustomPicker Picker = new CustomPicker
            {
                Title = "Select",
                TitleColor = Color.DarkBlue,
                ItemsSource = EbSelect.Options,
                ItemDisplayBinding = new Binding("DisplayName")
            };

            return Picker;
        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            ResultStack.Children.Clear();
            if (SearchBox.Text.Length > 3)
            {
                EbDataTable Data = GetData(SearchBox.Text);
                int c = 1;

                if (Data.Rows.Count <= 0)
                {
                    ResultStack.Children.Add(new Label { Text = "Not found :(" });
                }

                foreach (EbDataRow row in Data.Rows)
                {
                    ComboBoxLabel lbl = new ComboBoxLabel(c)
                    {
                        Text = row[(EbControl as EbMobileSimpleSelect).DisplayMember.ColumnName].ToString(),
                        Value = row[(EbControl as EbMobileSimpleSelect).ValueMember.ColumnName],
                    };

                    var labelTaped = new TapGestureRecognizer();
                    labelTaped.Tapped += LabelTaped_Tapped;

                    lbl.GestureRecognizers.Add(labelTaped);
                    ResultStack.Children.Add(lbl);
                    c++;
                }
                this.ResultFrame.IsVisible = true;
            }
            else
            {
                this.Selected = null;
                this.ResultFrame.IsVisible = false;
            }
        }

        private void LabelTaped_Tapped(object sender, EventArgs e)
        {
            this.Selected = sender as ComboBoxLabel;
            this.SearchBox.Text = this.Selected.Text;
            this.ResultFrame.IsVisible = false;
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
    }

    public class ComboBoxLabel : Label
    {
        public object Value { set; get; }

        public ComboBoxLabel() { }

        public ComboBoxLabel(int index)
        {
            this.Padding = new Thickness(5);

            if (index % 2 == 0)
            {
                this.BackgroundColor = Color.FromHex("ecf0f1");
            }
        }
    }
}
