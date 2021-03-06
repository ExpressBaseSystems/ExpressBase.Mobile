﻿using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Structures;
using ExpressBase.Mobile.Views.Shared;
using System;
using Xamarin.Forms;

namespace ExpressBase.Mobile
{
    public class EbMobileQrReader : EbMobileControl
    {
        public override EbDbTypes EbDbType { get { return EbDbTypes.String; } set { } }

        private EbXTextBox dataHolder;

        public override View Draw(FormMode mode, NetworkMode network)
        {
            dataHolder = new EbXTextBox
            {
                IsReadOnly = true,
                XBackgroundColor = Color.FromHex("#fafafa")
            };
            Button link = new Button
            {
                Style = (Style)HelperFunctions.GetResourceValue("QRReaderButton")
            };
            link.Clicked += OpenQrScanner;

            Button clear = new Button
            {
                Style = (Style)HelperFunctions.GetResourceValue("QRReaderClearButton")
            };
            clear.Clicked += Clear_Clicked; ;

            var grid = new Grid
            {
                Style = (Style)HelperFunctions.GetResourceValue("QRReaderContainerGrid"),
                RowDefinitions =
                {
                    new RowDefinition{Height = GridLength.Auto },
                    new RowDefinition{Height = 40 },
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition{Width = GridLength.Star},
                    new ColumnDefinition{Width = GridLength.Auto},
                }
            };

            grid.Children.Add(dataHolder, 0, 0);
            Grid.SetColumnSpan(dataHolder, 2);

            grid.Children.Add(link, 0, 1);
            grid.Children.Add(clear, 1, 1);

            XControl = grid;

            return base.Draw(mode, network);
        }

        private void Clear_Clicked(object sender, EventArgs e)
        {
            dataHolder?.ClearValue(EbXTextBox.TextProperty);
        }

        public override object GetValue()
        {
            return dataHolder.Text;
        }

        public override void SetValue(object value)
        {
            dataHolder.Text = value?.ToString();
        }

        public override void Reset()
        {
            dataHolder.ClearValue(EbXTextBox.TextProperty);
        }

        public override bool Validate()
        {
            string value = GetValue() as string;

            if (Required && string.IsNullOrEmpty(value))
                return false;

            return true;
        }

        private async void OpenQrScanner(object sender, EventArgs e)
        {
            bool hasCameraAccess = await AppPermission.Camera();

            if (hasCameraAccess)
            {
                QrScanner scannerPage = new QrScanner();
                scannerPage.BindMethod(OnScannedResult);

                await App.Navigation.NavigateByRenderer(scannerPage);
            }
            else
                Utils.Toast("Allow permission to access camera");
        }

        public void OnScannedResult(string payload)
        {
            Device.BeginInvokeOnMainThread(() => dataHolder.Text = payload);
        }
    }
}
