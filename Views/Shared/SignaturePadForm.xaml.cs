using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Views.Base;
using SignaturePad.Forms;
using System;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views.Shared
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SignaturePadForm : ContentPage
    {
        private readonly EbSignaturePad signPad;

        public event SignPadDoneHandler OnSigningDone;

        public SignaturePadForm(EbSignaturePad signPad)
        {
            this.signPad = signPad;
            InitializeComponent();
        }

        private async void SaveBtn_Clicked(object sender, EventArgs e)
        {
            if (signPad.IsSignFormVisible)
            {
                signPad.IsSignFormVisible = false;
                Stream imgStream = await MainSignaturePad.GetImageStreamAsync(SignatureImageFormat.Png);
                if (imgStream == null)
                {
                    signPad.IsSignFormVisible = true;
                    Utils.Toast("Signature required");
                    return;
                }
                OnSigningDone?.Invoke(imgStream);
                await App.Navigation.PopMasterModalAsync(true);
            }
        }
        private async void CancelBtn_Clicked(object sender, EventArgs e)
        {
            if (signPad.IsSignFormVisible)
            {
                signPad.IsSignFormVisible = false;
                await App.Navigation.PopMasterModalAsync(true);
            }
        }

        protected override bool OnBackButtonPressed()
        {
            if (signPad.IsSignFormVisible)
            {
                signPad.IsSignFormVisible = false;
                App.Navigation.PopMasterModalAsync(true);
            }
            return true;
        }
    }
}