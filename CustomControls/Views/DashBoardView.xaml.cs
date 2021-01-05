using ExpressBase.Mobile.Data;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.CustomControls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DashBoardView : ContentView
    {
        public static readonly BindableProperty DashBoardProperty =
            BindableProperty.Create(nameof(DashBoard), typeof(EbMobileDashBoard), typeof(DashBoardView), propertyChanged: OnDashBoardPropertyChanged);

        public static readonly BindableProperty DataProperty =
            BindableProperty.Create(nameof(Data), typeof(EbDataSet), typeof(DashBoardView), propertyChanged: OnDataPropertyChanged);

        public EbMobileDashBoard DashBoard
        {
            get { return (EbMobileDashBoard)GetValue(DashBoardProperty); }
            set { SetValue(DashBoardProperty, value); }
        }

        public EbDataSet Data
        {
            get { return (EbDataSet)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        List<EbMobileDashBoardControl> controls;

        public DashBoardView()
        {
            InitializeComponent();
        }

        private static void OnDataPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            DashBoardView binding = bindable as DashBoardView;

            EbDataSet ds = (EbDataSet)newValue;

            if (ds != null)
            {
                binding.BindValues(ds);
            }
        }

        private static void OnDashBoardPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            DashBoardView binding = bindable as DashBoardView;

            EbMobileDashBoard dash = (EbMobileDashBoard)newValue;

            if (dash != null)
            {
                binding.Container.Spacing = dash.Spacing;

                if (binding.DashBoard.ChildControls != null)
                {
                    binding.controls = dash.ChildControls;
                    binding.DrawTemplate();
                }
            }
        }

        private void DrawTemplate()
        {
            foreach (EbMobileDashBoardControl ctrl in controls)
            {
                View view = ctrl.Draw();

                if (view != null)
                {
                    Container.Children.Add(view);
                }
            }
        }

        private void BindValues(EbDataSet dataSet)
        {
            if (controls == null) return;

            foreach (EbMobileDashBoardControl ctrl in controls)
            {
                ctrl.SetBindingValue(dataSet);
            }
        }
    }
}