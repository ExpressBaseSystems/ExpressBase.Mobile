using ExpressBase.Mobile.CustomControls;
using System.Linq;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Behavior
{
    /// <summary>
    /// NewSolution page textbox behavior
    /// Contains solutionurl validation
    /// </summary>
    public class SolutioUrlBehavior : Behavior<EbXTextBox>
    {
        protected override void OnAttachedTo(EbXTextBox numeric)
        {
            numeric.TextChanged += OnEntryTextChanged;
            base.OnAttachedTo(numeric);
        }

        protected override void OnDetachingFrom(EbXTextBox numeric)
        {
            numeric.TextChanged -= OnEntryTextChanged;
            base.OnDetachingFrom(numeric);
        }

        /// <summary>
        /// Inherited method
        /// Removing whitespace on textbox change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
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

                ((EbXTextBox)sender).Text = isValid ? args.NewTextValue.ToLower() : args.NewTextValue.Remove(args.NewTextValue.Length - 1);
            }
        }
    }
}
