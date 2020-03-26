using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
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
    public class ObjectsRenderViewModel : StaticBaseViewModel
    {
        private bool _isRefreshing;

        public bool IsRefreshing
        {
            get { return this._isRefreshing; }
            set
            {
                this._isRefreshing = value;
                this.NotifyPropertyChanged();
            }
        }

        private string _loaderMessage;

        public string LoaderMessage
        {
            get { return this._loaderMessage; }
            set
            {
                this._loaderMessage = value;
                this.NotifyPropertyChanged();
            }
        }

        public View View { set; get; }

        public List<MobilePagesWraper> ObjectList { set; get; }

        public Command SyncButtonCommand => new Command(OnSyncClick);

        public ObjectsRenderViewModel()
        {
            LoaderMessage = "Opening page...";
            PageTitle = Settings.AppName;

            SetUpData();
            BuildView();

            //deploy tables for forms
            DeployFormTables();
        }

        public void DeployFormTables()
        {
            Task.Run(() =>
            {
                var pages = Settings.Objects;

                foreach (MobilePagesWraper _p in pages)
                {
                    EbMobilePage mpage = _p.ToPage();

                    if (mpage != null && mpage.Container is EbMobileForm)
                    {
                        if (mpage.NetworkMode == NetworkMode.Offline || mpage.NetworkMode == NetworkMode.Mixed)
                            (mpage.Container as EbMobileForm).CreateTableSchema();
                    }
                }
            });
        }

        public void SetUpData()
        {
            var _objlist = Store.GetJSON<List<MobilePagesWraper>>(AppConst.OBJ_COLLECTION);

            if (_objlist != null)
                this.ObjectList = _objlist;
            else
                this.ObjectList = new List<MobilePagesWraper>();
        }

        public void BuildView()
        {
            var stack = new StackLayout();
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += ObjFrame_Clicked;
            var grouped = ObjectList.Group();
            foreach (KeyValuePair<string, List<MobilePagesWraper>> pair in grouped)
            {
                if (!pair.Value.Any())
                    continue;
                var groupLayout = new StackLayout
                {
                    Children =
                    {
                        new Label
                        {
                            FontSize = 16, Text = pair.Key + $" ({pair.Value.Count})", Padding = 5,
                            Style = (Style)HelperFunctions.GetResourceValue("MediumLabel")
                        }
                    }
                };

                AddGroupElement(groupLayout, pair.Value, tapGesture);
                stack.Children.Add(groupLayout);
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
                    RowSpacing = 10,
                    ColumnSpacing = 10,
                    RowDefinitions =
                    {
                        new RowDefinition{Height= GridLength.Auto}
                    },
                    ColumnDefinitions = {
                        new ColumnDefinition(),
                        new ColumnDefinition()
                    }
                };
                int rownum = 0;
                int colnum = 0;

                foreach (MobilePagesWraper wrpr in pageWrapers)
                {
                    if (wrpr.IsHidden)
                        continue;

                    var iconFrame = new CustomShadowFrame
                    {
                        Padding = 10,
                        BorderColor = Color.FromHex("fafafa"),
                        HasShadow = true,
                        CornerRadius = 4,
                        PageWraper = wrpr,
                        BackgroundColor = wrpr.IconBackground
                    };
                    iconFrame.GestureRecognizers.Add(gesture);

                    var iconContainer = new FlexLayout { Direction = FlexDirection.Column };
                    iconContainer.SizeChanged += IconContainer_SizeChanged;

                    var icon = new Label
                    {
                        Text = Regex.Unescape("\\u" + wrpr.ObjectIcon),
                        FontSize = 30,
                        TextColor = wrpr.IconColor,
                        VerticalTextAlignment = TextAlignment.Center,
                        HorizontalTextAlignment = TextAlignment.Center,
                        FontFamily = (OnPlatform<string>)HelperFunctions.GetResourceValue("FontAwesome")
                    };
                    icon.SizeChanged += Icon_SizeChanged;
                    iconContainer.Children.Add(icon);
                    FlexLayout.SetAlignSelf(icon, FlexAlignSelf.Center);
                    FlexLayout.SetGrow(icon, 1);

                    var name = new Label
                    {
                        Text = wrpr.DisplayName,
                        HorizontalTextAlignment = TextAlignment.Center,
                        TextColor = wrpr.TextColor
                    };
                    iconContainer.Children.Add(name);

                    iconFrame.Content = iconContainer;
                    grid.Children.Add(iconFrame, colnum, rownum);

                    if (wrpr != pageWrapers.Last())
                    {
                        if (colnum == 1)
                        {
                            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                            rownum++;
                            colnum = 0;
                        }
                        else
                            colnum = 1;
                    }
                }
                groupLayout.Children.Add(grid);
            }
            catch (Exception ex)
            {
                Log.Write("ObjectRenderViewModel.AddGroupElement---" + ex.Message);
            }
        }

        private void Icon_SizeChanged(object sender, EventArgs e)
        {
            var icon = (sender as Label);
            icon.FontSize = icon.Height * .4;
        }

        private void IconContainer_SizeChanged(object sender, EventArgs e)
        {
            var lay = (sender as FlexLayout);
            lay.HeightRequest = lay.Width;
        }

        private void OnSyncClick(object sender)
        {
            IToast toast = DependencyService.Get<IToast>();
            if (Settings.HasInternet)
            {
                Task.Run(() =>
                {
                    try
                    {
                        Device.BeginInvokeOnMainThread(() => { IsBusy = true; LoaderMessage = "Pushing from local data..."; });

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
            else
                toast.Show("You are not connected to internet !");
        }

        private void ObjFrame_Clicked(object obj, EventArgs e)
        {
            MobilePagesWraper item = (obj as CustomShadowFrame).PageWraper;
            Task.Run(() =>
            {
                IToast toast = DependencyService.Get<IToast>();
                Device.BeginInvokeOnMainThread(() => { IsBusy = true; LoaderMessage = "Loading page..."; });
                try
                {
                    EbMobilePage page = HelperFunctions.GetPage(item.RefId);
                    if (page == null)
                    {
                        toast.Show("This page is no longer available.");
                        return;
                    }

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        switch (page.Container)
                        {
                            case EbMobileForm f:
                                (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(new FormRender(page));
                                break;
                            case EbMobileVisualization v:
                                (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(new ListViewRender(page));
                                break;
                            case EbMobileDashBoard d:
                                (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(new DashBoardRender(page));
                                break;
                            case EbMobilePdf p:
                                (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(new PdfRender(page));
                                break;
                            default:
                                toast.Show("This page is no longer available.");
                                break;
                        }
                        IsBusy = false;
                    });
                }
                catch (Exception ex)
                {
                    Device.BeginInvokeOnMainThread(() => IsBusy = false);
                    Log.Write("ObjectRender_ObjFrame_Clicked" + ex.Message);
                }
            });
        }
    }
}
