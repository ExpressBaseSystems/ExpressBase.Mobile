using ExpressBase.Mobile.CustomControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Behavior
{
    public class SolutioUrlBehavior : Behavior<TextBox>
    {
        protected override void OnAttachedTo(TextBox numeric)
        {
            numeric.TextChanged += OnEntryTextChanged;
            base.OnAttachedTo(numeric);
        }

        protected override void OnDetachingFrom(TextBox numeric)
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

                if (charArray.Any(char.IsWhiteSpace))
                    isValid = false;
                else
                    isValid = true;

                ((TextBox)sender).Text = isValid ? args.NewTextValue : args.NewTextValue.Remove(args.NewTextValue.Length - 1);
            }
        }
    }
}
