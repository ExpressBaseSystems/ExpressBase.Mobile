using ExpressBase.Mobile.CustomControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Behavior
{
    /// <summary>
    /// MobileForm Numeric control validation
    /// Allow numbers only
    /// </summary>
    public class NumericBoxBehavior : Behavior<EbXNumericTextBox>
    {
        protected override void OnAttachedTo(EbXNumericTextBox numeric)
        {
            numeric.TextChanged += OnEntryTextChanged;
            base.OnAttachedTo(numeric);
        }

        protected override void OnDetachingFrom(EbXNumericTextBox numeric)
        {
            numeric.TextChanged -= OnEntryTextChanged;
            base.OnDetachingFrom(numeric);
        }

        /// <summary>
        /// Inherited method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void OnEntryTextChanged(object sender, TextChangedEventArgs args)
        {
            if (!string.IsNullOrWhiteSpace(args.NewTextValue))
            {
                char[] charArray = args.NewTextValue.ToCharArray();
                bool isValid;
                if (charArray.All(x => char.IsDigit(x) || char.IsPunctuation('.')))
                    isValid = true;
                else if (charArray.Contains('.'))
                    isValid = false;
                else
                    isValid = false;

                ((EbXNumericTextBox)sender).Text = isValid ? args.NewTextValue : args.NewTextValue.Remove(args.NewTextValue.Length - 1);
            }
        }
    }
}
