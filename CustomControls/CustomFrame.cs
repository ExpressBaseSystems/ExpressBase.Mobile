using ExpressBase.Mobile.Data;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ExpressBase.Mobile.CustomControls
{
    public class CustomFrame : Frame
    {
        public ColumnColletion Columns { set; get; }

        public EbDataRow DataRow { set; get; }

        private StackLayout OuterLayout { set; get; }

        public CustomFrame() { }

        public CustomFrame(EbDataRow Row, ColumnColletion Columns, Color BgColor)
        {
            this.DataRow = Row;
            this.Columns = Columns;
            this.BackgroundColor = BgColor;
        }

        public void SetSyncFlag()
        {
            var _label = new Label
            {
                Text = "&#xf013;",
                TextColor = Color.FromHex("cccccc"),
            };
            _label.SetDynamicResource(Label.StyleProperty, "SyncFlag");
            this.OuterLayout.Children.Add(_label);
        }

        public void SetContent(View _View)
        {
            this.Content = _View;
        }
    }
}
