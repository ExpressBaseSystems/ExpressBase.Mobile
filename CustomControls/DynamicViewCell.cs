using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace ExpressBase.Mobile.CustomControls
{
    public class DynamicViewCell : ViewCell
    {
        public static readonly BindableProperty VisualizationProperty =
            BindableProperty.Create("Visualization", typeof(EbMobileVisualization), typeof(DynamicViewCell));

        public static readonly BindableProperty ItemSelectedProperty =
            BindableProperty.Create(propertyName: "ItemSelected", typeof(ICommand), typeof(DynamicViewCell));

        public EbMobileVisualization Visualization
        {
            get { return (EbMobileVisualization)GetValue(VisualizationProperty); }
            set { SetValue(VisualizationProperty, value); }
        }

        public ICommand ItemSelected
        {
            get { return (ICommand)GetValue(ItemSelectedProperty); }
            set { SetValue(ItemSelectedProperty, value); }
        }

        private TapGestureRecognizer tapGesture;

        public DynamicViewCell() { }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            EbDataRow row = (EbDataRow)this.BindingContext;

            if (BindingContext != null)
            {
                if (Visualization.HasLink())
                {
                    tapGesture = new TapGestureRecognizer { NumberOfTapsRequired = 1 };
                    tapGesture.Tapped += TapGesture_Tapped;
                }

                DynamicFrame li = new DynamicFrame(row, Visualization, false);
                li.GestureRecognizers.Add(tapGesture);
                View = li;
            }
        }

        private void TapGesture_Tapped(object sender, EventArgs e)
        {
            if (ItemSelected != null && ItemSelected.CanExecute(sender))
            {
                ItemSelected.Execute(sender);
            }
        }
    }
}
