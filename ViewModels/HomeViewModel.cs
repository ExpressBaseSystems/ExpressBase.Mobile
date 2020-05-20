﻿using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Structures;
using ExpressBase.Mobile.ViewModels.BaseModels;
using ExpressBase.Mobile.Views.Dynamic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels
{
    public class HomeViewModel : StaticBaseViewModel
    {
        public View View { set; get; }

        public List<MobilePagesWraper> ObjectList { set; get; }

        public Command SyncButtonCommand => new Command(OnSyncClick);

        public HomeViewModel()
        {
            PageTitle = Settings.AppName;

            this.SetUpData();
            this.BuildView();

            //deploy tables for forms
            this.DeployFormTables();
        }

        public void DeployFormTables()
        {
            Task.Run(() =>
            {
                foreach (MobilePagesWraper _p in this.ObjectList)
                {
                    EbMobilePage mpage = _p.ToPage();

                    if (mpage != null && mpage.Container is EbMobileForm)
                    {
                        if (mpage.NetworkMode != NetworkMode.Online)
                            (mpage.Container as EbMobileForm).CreateTableSchema();
                    }
                }
            });
        }

        public void SetUpData()
        {
            List<MobilePagesWraper> _objlist = Store.GetJSON<List<MobilePagesWraper>>(AppConst.OBJ_COLLECTION);

            if (_objlist != null)
                this.ObjectList = _objlist;
            else
                this.ObjectList = new List<MobilePagesWraper>();
        }

        public void BuildView()
        {
            StackLayout stack = new StackLayout();
            TapGestureRecognizer tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += ObjFrame_Clicked;
            Dictionary<string, List<MobilePagesWraper>> grouped = ObjectList.Group();

            foreach (string label in ContainerLabels.ListOrder)
            {
                if (grouped.ContainsKey(label) && grouped[label].Any())
                {
                    StackLayout groupLayout = new StackLayout
                    {
                        Children =
                        {
                            new Label
                            {
                                FontSize = 16, Text = label + $" ({grouped[label].Count})", Padding = 5,
                                Style = (Style)HelperFunctions.GetResourceValue("MediumLabel")
                            }
                        }
                    };
                    this.AddGroupElement(groupLayout, grouped[label], tapGesture);
                    stack.Children.Add(groupLayout);
                }
            }
            this.View = stack;
        }

        private void AddGroupElement(StackLayout groupLayout, List<MobilePagesWraper> pageWrapers, TapGestureRecognizer gesture)
        {
            try
            {
                Grid grid = new Grid
                {
                    Padding = 5,
                    RowSpacing = 15,
                    ColumnSpacing = 15,
                    RowDefinitions = { new RowDefinition() },
                    ColumnDefinitions = {
                        new ColumnDefinition(),
                        new ColumnDefinition(),
                        new ColumnDefinition()
                    }
                };
                int rownum = 0;
                int colnum = 0;

                foreach (MobilePagesWraper wrpr in pageWrapers)
                {
                    if (wrpr.IsHidden) continue;

                    StackLayout objectTitle = this.GetObjectTile(wrpr, gesture);

                    grid.Children.Add(objectTitle, colnum, rownum);

                    if (wrpr != pageWrapers.Last())
                    {
                        if (colnum == 2)
                        {
                            grid.RowDefinitions.Add(new RowDefinition());
                            rownum++;
                            colnum = 0;
                        }
                        else
                            colnum += 1;
                    }
                }
                groupLayout.Children.Add(grid);
            }
            catch (Exception ex)
            {
                Log.Write("ObjectRenderViewModel.AddGroupElement---" + ex.Message);
            }
        }

        private StackLayout GetObjectTile(MobilePagesWraper wrpr, TapGestureRecognizer gesture)
        {
            StackLayout container = new StackLayout { Orientation = StackOrientation.Vertical };
            try
            {
                CustomShadowFrame iconFrame = new CustomShadowFrame(wrpr) { GestureRecognizers = { gesture } };
                container.Children.Add(iconFrame);

                string labelIcon = string.Empty;
                try
                {
                    if (wrpr.ObjectIcon.Length != 4) throw new Exception();
                    labelIcon = Regex.Unescape("\\u" + wrpr.ObjectIcon);
                }
                catch (Exception ex)
                {
                    labelIcon = Regex.Unescape("\\u" + wrpr.GetDefaultIcon());
                    Log.Write("font icon format is invalid." + ex.Message);
                }

                Label icon = new Label
                {
                    Text = labelIcon,
                    FontSize = 35,
                    TextColor = wrpr.IconColor,
                    VerticalTextAlignment = TextAlignment.Center,
                    HorizontalTextAlignment = TextAlignment.Center,
                    FontFamily = (OnPlatform<string>)HelperFunctions.GetResourceValue("FontAwesome")
                };
                icon.SizeChanged += IconContainer_SizeChanged;
                iconFrame.Content = icon;

                Label name = new Label
                {
                    FontSize = 13,
                    Text = wrpr.DisplayName,
                    HorizontalTextAlignment = TextAlignment.Center
                };
                container.Children.Add(name);
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
            return container;
        }

        private void IconContainer_SizeChanged(object sender, EventArgs e)
        {
            Label lay = (sender as Label);
            if (lay.Width != lay.Height)
                lay.HeightRequest = lay.Width;
        }

        private async void OnSyncClick(object sender)
        {
            IToast toast = DependencyService.Get<IToast>();
            if (!Settings.HasInternet)
            {
                toast.Show("You are not connected to internet !");
                return;
            }

            await Task.Run(() =>
            {
                try
                {
                    Device.BeginInvokeOnMainThread(() => { IsBusy = true; });

                    Auth.AuthIfTokenExpired();

                    SyncResponse response = SyncServices.Instance.Sync();

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        IsBusy = false;
                        toast.Show(response.Message);
                    });
                }
                catch (Exception ex)
                {
                    Log.Write("ObjectRender_OnSyncClick" + ex.Message);
                }
            });
        }

        private bool IsTapped;

        private async void ObjFrame_Clicked(object obj, EventArgs e)
        {
            if (IsTapped) return;

            MobilePagesWraper item = (obj as CustomShadowFrame).PageWraper;
            IToast toast = DependencyService.Get<IToast>();

            try
            {
                EbMobilePage page = HelperFunctions.GetPage(item.RefId);
                if (page == null)
                {
                    toast.Show("This page is no longer available.");
                    return;
                }
                IsTapped = true;

                ContentPage renderer = await GetPageByContainer(page);
                if (renderer != null)
                    await (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(renderer);

                IsTapped = false;
            }
            catch (Exception ex)
            {
                IsTapped = false;
                Log.Write("ObjectRender_ObjFrame_Clicked" + ex.Message);
            }
        }

        private async Task<ContentPage> GetPageByContainer(EbMobilePage page)
        {
            ContentPage renderer = null;
            try
            {
                await Task.Delay(100);

                switch (page.Container)
                {
                    case EbMobileForm f:
                        renderer = new FormRender(page);
                        break;
                    case EbMobileVisualization v:
                        renderer = new ListRender(page);
                        break;
                    case EbMobileDashBoard d:
                        renderer = new DashBoardRender(page);
                        break;
                    case EbMobilePdf p:
                        renderer = new PdfRender(page);
                        break;
                    default:
                        Log.Write("inavlid container type");
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
            return renderer;
        }
    }
}
