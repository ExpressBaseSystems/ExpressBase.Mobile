using ExpressBase.Mobile.CustomControls.Layout;
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

        private void SetTapGestureEvent(View view)
        {
            tapGesture = new TapGestureRecognizer { NumberOfTapsRequired = 1 };
            tapGesture.Tapped += ItemTappedEvent;
            view.GestureRecognizers.Add(tapGesture);
        }

        private void SetItemColoring(View view)
        {
            if (Visualization.EnableAlternateRowColoring)
            {
                SetBackGroundColor(ItemIndex.Value, view);
                ItemIndex.Increment();
            }
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            if (BindingContext != null)
            {
                DynamicFrame li = null;

                if (this.BindingContext is EbDataRow row)
                {
                    li = new DynamicFrame(row, Visualization, false);
                    if (Visualization.HasLink())
                    {
                        SetTapGestureEvent(li);
                    }
                }
                else if (this.BindingContext is EbMobileStaticListItem item)
                {
                    li = new StaticLSFrame(item, Visualization, false);
                    if (item.HasLink())
                    {
                        SetTapGestureEvent(li);
                    }
                }

                if (li != null)
                {
                    SetItemColoring(li);
                    this.View = li;
                }
            }
        }

        private void ItemTappedEvent(object sender, EventArgs e)
        {
            if (ItemSelected != null && ItemSelected.CanExecute(sender))
            {
                ItemSelected.Execute(sender);
            }
        }

        public void SetBackGroundColor(int index, View frame)
        {
            if (index % 2 == 0)
                frame.BackgroundColor = Color.Default;
            else
                frame.BackgroundColor = Color.FromHex("F2F2F2");
        }
    }
}
