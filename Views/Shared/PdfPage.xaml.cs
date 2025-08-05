using ExpressBase.Mobile.CustomControls;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views.Shared
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PdfPage : ContentPage
    {
        public bool isRendered;
        private string base64;
        private string title;

        public PdfPage(string Base64, string Title)
        {
            base64 = Base64;
            title = Title ?? "PDF Viewer";
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (!isRendered)
            {
                isRendered = true;
            }

            EbLayout.Title = title;

            var htmlViewerUrl = "file:///android_asset/pdfviewer/index.html";

            PdfWebView.Source = htmlViewerUrl;

            PdfWebView.Navigated += (s, e) =>
            {
                string js = $"loadPdf('data:application/pdf;base64,{base64}');";
                PdfWebView.Eval(js);
                PdfWebView.HorizontalOptions = LayoutOptions.FillAndExpand;
                PdfWebView.VerticalOptions = LayoutOptions.FillAndExpand;
            };

        }

        private bool OnBackButtonPressed(object sender, EventArgs e)
        {
            return true;
        }
    }
}