using ExpressBase.Mobile.CustomControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Behavior
{
    public class NumericBoxBehavior : Behavior<NumericTextBox>
    {
        protected override void OnAttachedTo(NumericTextBox numeric)
        {
            numeric.TextChanged += OnEntryTextChanged;
            base.OnAttachedTo(numeric);
        }

        protected override void OnDetachingFrom(NumericTextBox numeric)
        {
            numeric.TextChanged -= OnEntryTextChanged;
            base.OnDetachingFrom(numeric);
        }

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

                ((NumericTextBox)sender).Text = isValid ? args.NewTextValue : args.NewTextValue.Remove(args.NewTextValue.Length - 1);
            }
        }
    }
}
