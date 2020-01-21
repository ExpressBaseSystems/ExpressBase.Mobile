using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Structures;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ExpressBase.Mobile
{
    public class EbMobileSimpleSelect : EbMobileControl
    {
        public override EbDbTypes EbDbType
        {
            get
            {
                if (!string.IsNullOrEmpty(DataSourceRefId))
                {
                    if (this.ValueMember != null)
                    {
                        return this.ValueMember.EbDbType;
                    }
                    else
                    {
                        return EbDbTypes.String;
                    }
                }
                else
                {
                    return EbDbTypes.String;
                }
            }
            set { }
        }

        public List<EbMobileSSOption> Options { set; get; }

        public bool IsMultiSelect { get; set; }

        public string DataSourceRefId { get; set; }

        public List<EbMobileDataColumn> Columns { set; get; }

        public EbMobileDataColumn DisplayMember { set; get; }

        public EbMobileDataColumn ValueMember { set; get; }

        public EbScript OfflineQuery { set; get; }

        public List<Param> Parameters { set; get; }

        //mobile props
        public CustomSearchBar SearchBox { set; get; }

        public StackLayout ResultStack { set; get; }

        public Frame ResultFrame { set; get; }

        public ComboBoxLabel Selected { set; get; }

        public override void InitXControl(FormMode Mode)
        {
            if (string.IsNullOrEmpty(this.DataSourceRefId))
            {
                XControl = this.GetPicker();
            }
            else
            {
                XControl = this.GetComboBox();
            }
        }

        public override object GetValue()
        {
            if (string.IsNullOrEmpty(this.DataSourceRefId))
            {
                if ((this.XControl as CustomPicker).SelectedItem != null)
                {
                    return ((this.XControl as CustomPicker).SelectedItem as EbMobileSSOption).Value;
                }
                else
                    return null;
            }
            else
                return (this.Selected == null) ? null : this.Selected.Value;
        }

        public override bool SetValue(object value)
        {
            if (value == null)
                return false;

            if (string.IsNullOrEmpty(this.DataSourceRefId))
            {
                (this.XControl as CustomPicker).SelectedItem = this.Options.Find(i => i.Value == value.ToString());
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
                Placeholder = "Seach " + this.Label + "...",
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

        public CustomPicker GetPicker()
        {
            CustomPicker Picker = new CustomPicker
            {
                Title = "Select",
                TitleColor = Color.DarkBlue,
                ItemsSource = this.Options,
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
                        Text = row[this.DisplayMember.ColumnName].ToString(),
                        Value = row[this.ValueMember.ColumnName],
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
                EbMobileDataColumn DisplayMember = this.DisplayMember;
                if (DisplayMember == null)
                {
                    throw new Exception();
                }

                byte[] b = Convert.FromBase64String(this.OfflineQuery.Code);
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

    public class EbMobileSSOption : EbMobilePageBase
    {
        public string EbSid { get; set; }

        public override string DisplayName { get; set; }

        public string Value { get; set; }
    }
}
