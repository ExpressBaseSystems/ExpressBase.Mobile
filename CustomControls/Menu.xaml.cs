using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.CustomControls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Menu : ContentView
    {
        public static readonly BindableProperty ItemSourceProperty =
            BindableProperty.Create("ItemSource", typeof(IEnumerable<MobilePagesWraper>), typeof(Menu));

        public static readonly BindableProperty ItemTapedProperty =
            BindableProperty.Create(propertyName: "ItemTaped", typeof(ICommand), typeof(Menu));

        public IEnumerable<MobilePagesWraper> ItemSource
        {
            get { return (IEnumerable<MobilePagesWraper>)GetValue(ItemSourceProperty); }
            set { SetValue(ItemSourceProperty, value); }
        }

        public ICommand ItemTaped
        {
            get { return (ICommand)GetValue(ItemTapedProperty); }
            set { SetValue(ItemTapedProperty, value); }
        }

        private TapGestureRecognizer gesture;

        private int rownum;

        private int colnum;

        private MobilePagesWraper lastItem;

        public Menu()
        {
            InitializeComponent();

            gesture = new TapGestureRecognizer { NumberOfTapsRequired = 1 };
            gesture.Tapped += MenuItemTapped;
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == ItemSourceProperty.PropertyName)
            {
                Container.Content = null;

                if (ItemSource != null && ItemSource.Any())
                {
                    lastItem = ItemSource.Last();
                    Render();
                }
            }
        }

        private void Render()
        {
            rownum = colnum = 0;

            Grid grid = this.CreateGrid();
            Container.Content = grid;

            try
            {
                foreach (MobilePagesWraper wrpr in this.ItemSource)
                {
                    if (wrpr.IsHidden) continue;

                    var container = new StackLayout { Orientation = StackOrientation.Vertical };

                    CustomShadowFrame iconFrame = new CustomShadowFrame(wrpr)
                    {
                        Style = (Style)HelperFunctions.GetResourceValue("MenuItemFrame"),
                        GestureRecognizers = { gesture }
                    };
                    container.Children.Add(iconFrame);

                    Label icon = new Label
                    {
                        Text = this.GetIcon(wrpr),
                        TextColor = wrpr.IconColor,
                        Style = (Style)HelperFunctions.GetResourceValue("MenuIconLabel")
                    };
                    icon.SizeChanged += IconContainer_SizeChanged;
                    iconFrame.Content = icon;

                    Label name = new Label
                    {
                        Text = wrpr.DisplayName,
                        Style = (Style)HelperFunctions.GetResourceValue("MenuDisplayNameLabel")
                    };
                    container.Children.Add(name);

                    SetGrid(grid, container, wrpr);
                }
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
        }

        private Grid CreateGrid()
        {
            return new Grid
            {
                RowSpacing = 15,
                ColumnSpacing = 15,
                RowDefinitions = { new RowDefinition() },
                ColumnDefinitions = {
                        new ColumnDefinition(),
                        new ColumnDefinition(),
                        new ColumnDefinition()
                }
            };
        }

        private void IconContainer_SizeChanged(object sender, EventArgs e)
        {
            Label lay = (sender as Label);
            lay.HeightRequest = lay.Width;
        }

        private string GetIcon(MobilePagesWraper wrpr)
        {
            string labelIcon;
            try
            {
                if (wrpr.ObjectIcon.Length != 4)
                    throw new Exception();
                labelIcon = Regex.Unescape("\\u" + wrpr.ObjectIcon);
            }
            catch (Exception ex)
            {
                labelIcon = Regex.Unescape("\\u" + wrpr.GetDefaultIcon());
                EbLog.Error("font icon format is invalid." + ex.Message);
            }
            return labelIcon;
        }

        private void SetGrid(Grid grid, View item, MobilePagesWraper current)
        {
            grid.Children.Add(item, colnum, rownum);

            if (colnum == 2)
            {
                if (current != lastItem)
                {
                    grid.RowDefinitions.Add(new RowDefinition());
                    rownum++;
                }
                colnum = 0;
            }
            else
                colnum += 1;
        }

        private void MenuItemTapped(object sender, EventArgs e)
        {
            if (ItemTaped == null)
                return;
            else
            {
                if (ItemTaped.CanExecute(sender))
                {
                    ItemTaped.Execute(sender);
                }
            }
        }
    }
}