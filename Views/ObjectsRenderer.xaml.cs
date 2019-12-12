using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ExpressBase.Mobile.Constants;
using Xamarin.Essentials;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Views.Shared;

namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ObjectsRenderer : ContentPage
    {
        public ObjectsRenderer()
        {
            InitializeComponent();
        }

        protected override bool OnBackButtonPressed()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                bool status = await this.DisplayAlert("Alert!", "Do you really want to exit?", "Yes", "No");
                if (status)
                {
                    INativeHelper nativeHelper = null;
                    nativeHelper = DependencyService.Get<INativeHelper>();
                    nativeHelper.CloseApp();
                }
            });
            return true;
        }
    }
}