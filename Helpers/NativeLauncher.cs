using ExpressBase.Mobile.Constants;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Helpers
{
    public class NativeLauncher
    {
        public static async Task OpenMapAsync(string cordinates)
        {
            IToast toast = DependencyService.Get<IToast>();

            if (string.IsNullOrEmpty(cordinates))
            {
                toast.Show("Empty Coordinates");
                return;
            }

            string[] parts = cordinates?.Split(CharConstants.COMMA);

            if (parts != null && parts.Length == 2)
            {
                try
                {
                    double latitude = Convert.ToDouble(parts[0]);
                    double longitude = Convert.ToDouble(parts[1]);

                    Location loc = new Location(latitude, longitude);

                    await Map.OpenAsync(loc);
                }
                catch (Exception ex)
                {
                    EbLog.Error($"Coordinates value format error 'Value:{cordinates}'");
                    EbLog.Error(ex.Message);

                    toast.Show("Unable to open map");
                }
            }
        }

        public static void OpenDialerAsync(string number)
        {
            IToast toast = DependencyService.Get<IToast>();
            try
            {
                PhoneDialer.Open(number);
            }
            catch (ArgumentNullException)
            {
                toast.Show("No number");
            }
            catch (FeatureNotSupportedException)
            {
                toast.Show("Unsupported feature");
            }
            catch (Exception)
            {
                toast.Show("Something went wrong");
            }
        }

        public static async Task OpenEmailAsync(string email)
        {
            IToast toast = DependencyService.Get<IToast>();

            if (string.IsNullOrEmpty(email))
            {
                toast.Show("Empty email");
                return;
            }

            try
            {
                var message = new EmailMessage
                {
                    To = new List<string> { email },
                };
                await Email.ComposeAsync(message);
            }
            catch (FeatureNotSupportedException fbsEx)
            {
                toast.Show("Email is not supported on this device");
                WriteLogEntry("Email is not supported on this device");
                WriteLogEntry(fbsEx.Message);
            }
            catch (Exception ex)
            {
                WriteLogEntry(ex.Message);
            }
        }

        private static void WriteLogEntry(string message)
        {
            EbLog.Error(message);
        }
    }
}
