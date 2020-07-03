using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.CustomControls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TwoFAView : ContentView
    {
        private readonly List<Label> labels;

        public TwoFAView()
        {
            InitializeComponent();

            labels = new List<Label>
            {
                L1,L2, L3,L4,L5,L6
            };
        }

        private void DigitBV_SizeChanged(object sender, EventArgs e)
        {
            BoxView bx = (BoxView)sender;
            bx.HeightRequest = bx.Width;
        }

        private void OtpTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HiddenEntry entry = (HiddenEntry)sender;

            string editorStr = entry.Text;
            //if string.length lager than max length
            if (editorStr.Length > 6)
            {
                entry.Text = editorStr.Substring(0, 6);
            }

            //dismiss keyboard
            if (editorStr.Length >= 6)
            {
                entry.Unfocus();
            }

            for (int i = 0; i < labels.Count; i++)
            {
                Label lb = labels[i];

                if (i < editorStr.Length)
                {
                    lb.Text = editorStr.Substring(i, 1);
                }
                else
                {
                    lb.Text = "";
                }
            }
        }
    }
}