using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.CustomControls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FormView : ContentView
    {
        public static readonly BindableProperty ControlsProperty = BindableProperty.Create("Controls", typeof(List<EbMobileControl>), typeof(ContentView));

        public static readonly BindableProperty NetWorkTypeProperty = BindableProperty.Create("NetWorkType", typeof(NetworkMode), typeof(ContentView));

        public static readonly BindableProperty FormModeProperty = BindableProperty.Create("FormMode", typeof(FormMode), typeof(ContentView));

        public static readonly BindableProperty SpacingProperty = BindableProperty.Create(nameof(Spacing), typeof(int), typeof(ContentView), defaultValue: 0);

        public List<EbMobileControl> Controls
        {
            get { return (List<EbMobileControl>)GetValue(ControlsProperty); }
            set { SetValue(ControlsProperty, value); }
        }

        public NetworkMode NetWorkType
        {
            get { return (NetworkMode)GetValue(NetWorkTypeProperty); }
            set { SetValue(NetWorkTypeProperty, value); }
        }

        public FormMode FormMode
        {
            get { return (FormMode)GetValue(FormModeProperty); }
            set { SetValue(FormModeProperty, value); }
        }

        public int Spacing
        {
            get { return (int)GetValue(SpacingProperty); }
            set { SetValue(SpacingProperty, value); }
        }

        public FormView()
        {
            InitializeComponent();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            this.Render();

            FormViewContainer.Spacing = Spacing;
        }

        private void Render()
        {
            try
            {
                FormViewContainer.Children.Clear();

                foreach (EbMobileControl ctrl in Controls)
                {
                    if (ctrl is EbMobileTableLayout)
                    {
                        foreach (EbMobileTableCell Tc in (ctrl as EbMobileTableLayout).CellCollection)
                        {
                            foreach (EbMobileControl tbctrl in Tc.ControlCollection)
                            {
                                View controlView = tbctrl.Draw(this.FormMode, this.NetWorkType);
                                FormViewContainer.Children.Add(controlView);
                            }
                        }
                    }
                    else
                    {
                        View controlView = ctrl.Draw(this.FormMode, this.NetWorkType);
                        FormViewContainer.Children.Add(controlView);
                    }
                }
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
        }
    }
}