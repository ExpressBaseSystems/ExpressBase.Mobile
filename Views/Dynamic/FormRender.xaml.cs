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

                Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        //edit mode
        public FormRender(EbMobilePage Page, int RowId)
        {
            InitializeComponent();
            try
            {
                SaveButton.Text = "Save Changes";
                SaveButton.IsVisible = false;
                EditButton.IsVisible = true;

                Renderer = new FormRenderViewModel(Page, RowId);
                FormScrollView.Content = Renderer.XView;
                BindingContext = Renderer;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        //reference mode
        public FormRender(EbMobilePage CurrentForm, EbMobilePage ParentForm, int ParentId)
        {
            InitializeComponent();
            try
            {
                Renderer = new FormRenderViewModel(CurrentForm, ParentForm, ParentId);
                FormScrollView.Content = Renderer.XView;
                BindingContext = Renderer;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        //prefill new mode
        public FormRender(EbMobilePage Page, EbDataRow dataRow, ColumnColletion dataColumns)
        {
            InitializeComponent();
            try
            {
                Renderer = new FormRenderViewModel(Page, dataRow, dataColumns);
                FormScrollView.Content = Renderer.XView;
                BindingContext = Renderer;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
            if (e.NetworkAccess == NetworkAccess.Internet)
                HideMessage("Back to online", Color.FromHex("41d041"));
            else
                ShowMessage("You are not connected to internet!", Color.FromHex("fd6b6b"));
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