using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.ViewModels.Dynamic;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views.Dynamic
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FormRender : ContentPage
    {
        public FormRenderViewModel Renderer { set; get; }

        //new mode
        public FormRender(EbMobilePage page)
        {
            InitializeComponent();
            try
            {
                Renderer = new FormRenderViewModel(page);
                FormScrollView.Content = Renderer.XView;
                BindingContext = Renderer;

                if (page.NetworkMode == NetworkMode.Online && !Settings.HasInternet)
                    ShowMessage("You are not connected to internet!", Color.FromHex("fd6b6b"));
                else
                    HideMessage("Back to online", Color.FromHex("41d041"));

                Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
            }
            catch (Exception ex)
            {
                Log.Write("FormRender new mode" + ex.Message);
            }
        }

        //edit mode
        public FormRender(EbMobilePage page, int rowId)
        {
            InitializeComponent();
            try
            {
                SaveButton.Text = "Save Changes";
                SaveButton.IsVisible = false;
                EditButton.IsVisible = true;

                Renderer = new FormRenderViewModel(page, rowId);
                FormScrollView.Content = Renderer.XView;
                BindingContext = Renderer;

                if (page.NetworkMode == NetworkMode.Online && !Settings.HasInternet)
                    ShowMessage("You are not connected to internet!", Color.FromHex("fd6b6b"));
                else
                    HideMessage("Back to online", Color.FromHex("41d041"));

                Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
            }
            catch (Exception ex)
            {
                Log.Write("FormRender edit mode" + ex.Message);
            }
        }

        //reference mode
        public FormRender(EbMobilePage currentForm, EbMobilePage parentForm, int parentId)
        {
            InitializeComponent();
            try
            {
                Renderer = new FormRenderViewModel(currentForm, parentForm, parentId);
                FormScrollView.Content = Renderer.XView;
                BindingContext = Renderer;
            }
            catch (Exception ex)
            {
                Log.Write("FormRender reference mode" + ex.Message);
            }
        }

        //prefill new mode
        public FormRender(EbMobilePage page, EbDataRow dataRow)
        {
            InitializeComponent();
            try
            {
                Renderer = new FormRenderViewModel(page, dataRow);
                FormScrollView.Content = Renderer.XView;
                BindingContext = Renderer;
            }
            catch (Exception ex)
            {
                Log.Write("FormRender prefill mode" + ex.Message);
            }
        }

        private void EditButton_Clicked(object sender, EventArgs e)
        {
            if (Renderer != null)
            {
                EditButton.IsVisible = false;
                SaveButton.IsVisible = true;

                foreach (var pair in Renderer.Form.ControlDictionary)
                    pair.Value.SetAsReadOnly(false);
            }
        }

        private void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            if(Renderer != null && Renderer.NetworkType == NetworkMode.Online)
            {
                if (e.NetworkAccess == NetworkAccess.Internet)
                    HideMessage("Back to online", Color.FromHex("41d041"));
                else
                    ShowMessage("You are not connected to internet!", Color.FromHex("fd6b6b"));
            }
        }

        private void ShowMessage(string message, Color background)
        {
            MessageBoxFrame.BackgroundColor = background;
            MessageBoxLabel.Text = message;
            MessageBox.IsVisible = true;
            SaveButton.IsVisible = false;
        }

        private void HideMessage(string message_beforehide, Color background)
        {
            MessageBoxFrame.BackgroundColor = background;
            MessageBoxLabel.Text = message_beforehide;
            MessageBox.IsVisible = false;
            SaveButton.IsVisible = true;
        }
    }
}