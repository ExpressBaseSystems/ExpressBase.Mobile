using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Structures;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace ExpressBase.Mobile
{
    public class EbMobileRating : EbMobileControl
    {
        public override EbDbTypes EbDbType { get { return EbDbTypes.Decimal; } set { } }

        public int MaxValue { get; set; }

        public string SelectionColor { get; set; }

        public int Spacing { get; set; }

        private readonly List<Button> buttonCollection;

        private int rating;

        private readonly Color selectionColor = App.Settings.Vendor.GetPrimaryColor();

        private readonly Color defaultColor = Color.FromHex("eeeeee");

        public EbMobileRating()
        {
            buttonCollection = new List<Button>();
        }

        public override View Draw(FormMode Mode, NetworkMode Network)
        {
            Grid grid = new Grid { ColumnSpacing = this.Spacing };

            for (int i = 0; i < this.MaxValue; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());

                Button btn = new Button
                {
                    Style = (Style)HelperFunctions.GetResourceValue("RatingControlButton"),
                    ClassId = (i + 1).ToString(),
                };
                btn.Clicked += StarClicked;

                grid.Children.Add(btn, i, 0);
                buttonCollection.Add(btn);
            }
            XControl = grid;

            return base.Draw(Mode, Network); ;
        }

        private void StarClicked(object sender, EventArgs e)
        {
            Button btn = (Button)sender;

            this.rating = Convert.ToInt32(btn.ClassId);

            this.UpdateSelection();
        }

        private void UpdateSelection()
        {
            if (this.rating == 0) return;

            for (int i = 0; i < this.buttonCollection.Count; i++)
            {
                if (i < this.rating)
                    buttonCollection[i].TextColor = selectionColor;
                else
                    buttonCollection[i].TextColor = defaultColor;
            }
        }

        public override object GetValue()
        {
            return this.rating;
        }

        public override void SetValue(object value)
        {
            this.rating = Convert.ToInt32(value);

            this.UpdateSelection();
        }

        public override void Reset()
        {
            this.rating = 0;
            this.buttonCollection.ForEach(item => item.TextColor = defaultColor);
        }

        public override bool Validate()
        {
            if (this.rating == 0 && this.Required)
                return false;

            return true;
        }
    }
}
