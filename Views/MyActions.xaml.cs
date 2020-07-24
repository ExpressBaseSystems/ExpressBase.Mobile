﻿using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MyActions : ContentPage
    {
        public bool isRendered;

        private readonly MyActionsViewModel viewModel;

        public MyActions()
        {
            InitializeComponent();
            BindingContext = viewModel = new MyActionsViewModel();
            Loader.IsVisible = true;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (!isRendered)
            {
                await viewModel.InitializeAsync();
                isRendered = true;
            }
            Loader.IsVisible = false;
        }

        private async void MyActionsRefresh_Refreshing(object sender, EventArgs e)
        {
            try
            {
                if (!Utils.HasInternet)
                {
                    DependencyService.Get<IToast>().Show("Not connected to internet!");
                    return;
                }

                MyActionsRefresh.IsRefreshing = true;
                await viewModel.RefreshMyActions();
                MyActionsRefresh.IsRefreshing = false;
            }
            catch (Exception ex)
            {
                MyActionsRefresh.IsRefreshing = false;
                EbLog.Write(ex.Message);
            }
        }
    }
}