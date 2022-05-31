using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.CustomControls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TwoFAView : ContentView
    {
        public static readonly BindableProperty SubmitClickedProperty = BindableProperty.Create(propertyName: "SubmitClicked", typeof(Command), typeof(TwoFAView));

        public static readonly BindableProperty ResendClickedProperty = BindableProperty.Create(propertyName: "ResendClicked", typeof(Command), typeof(TwoFAView));

        public Command SubmitClicked
        {
            get { return (Command)GetValue(SubmitClickedProperty); }
            set { SetValue(SubmitClickedProperty, value); }
        }

        public Command ResendClicked
        {
            get { return (Command)GetValue(ResendClickedProperty); }
            set { SetValue(ResendClickedProperty, value); }
        }

        private readonly List<Label> labels;

        public bool DisableBackBtn { get; set; }

        public TwoFAView()
        {
            InitializeComponent();

            labels = new List<Label>
            {
                L1,L2, L3,L4,L5,L6
            };
        }

        public void SetAddress(string address)
        {
            ToAddressLabel.Text = address;
        }

        private void DigitBV_SizeChanged(object sender, EventArgs e)
        {
            BoxView bx = (BoxView)sender;
            bx.HeightRequest = bx.Width;
        }

        private void OtpTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            EbXHiddenEntry entry = (EbXHiddenEntry)sender;
            string otp = entry.Text;

            if (otp.Length > 6)
            {
                entry.Text = otp.Substring(0, 6);
            }

            if (otp.Length >= 6) entry.Unfocus();

            for (int i = 0; i < labels.Count; i++)
            {
                Label lb = labels[i];

                if (i < otp.Length)
                {
                    lb.Text = otp.Substring(i, 1);
                }
                else
                    lb.Text = "";
            }
        }

        private void OtpSubmit_Clicked(object sender, EventArgs e)
        {
            string otp = OtpTextBox.Text.Trim();

            if (string.IsNullOrEmpty(otp) && otp.Length < 6)
                return;

            if (SubmitClicked.CanExecute(otp))
            {
                SubmitClicked.Execute(otp);
            }
        }

        private void ResendButoon_Clicked(object sender, EventArgs e)
        {
            if (ResendClicked.CanExecute(null))
            {
                ResendClicked.Execute(null);
            }
        }

        private void BackButton_Clicked(object sender, EventArgs e)
        {
            if (!this.DisableBackBtn)
                this.Hide();
        }

        public void Show()
        {
            this.IsVisible = true;
        }

        public void Hide()
        {
            this.IsVisible = false;
        }
    }
}