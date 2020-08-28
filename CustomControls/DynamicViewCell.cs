using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Models;
using System;
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

        public static readonly BindableProperty ItemIndexProperty =
            BindableProperty.Create(propertyName: "ItemIndex", typeof(IntRef), typeof(DynamicViewCell));

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

        public IntRef ItemIndex
        {
            get { return (IntRef)GetValue(ItemIndexProperty); }
            set { SetValue(ItemIndexProperty, value); }
        }

        private TapGestureRecognizer tapGesture;

        public DynamicViewCell() { }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            EbDataRow row = (EbDataRow)this.BindingContext;

            if (BindingContext != null)
            {
                DynamicFrame li = new DynamicFrame(row, Visualization, false);

                if (Visualization.HasLink())
                {
                    tapGesture = new TapGestureRecognizer { NumberOfTapsRequired = 1 };
                    tapGesture.Tapped += TapGesture_Tapped;

                    li.GestureRecognizers.Add(tapGesture);
                }

                if (Visualization.EnableAlternateRowColoring)
                {
                    this.SetBackGroundColor(ItemIndex.Value, li);
                    ItemIndex.Increment();
                }
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

        public void SetBackGroundColor(int index, DynamicFrame frame)
        {
            if (index % 2 == 0)
                frame.BackgroundColor = Color.Default;
            else
                frame.BackgroundColor = Color.FromHex("F2F2F2");
        }
    }
}
