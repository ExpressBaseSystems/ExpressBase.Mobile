using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
                Container.Children.Clear();

                if (ItemSource != null && ItemSource.Any())
                {
                    //dynamic rendering of links
                    this.Render();
                }
            }
        }

        private void Render()
        {
            try
            {
                var category = ItemSource.GroupByCategory();

                foreach (var pair in category)
                {
                    var catFrame = new Frame
                    {
                        Style = (Style)HelperFunctions.GetResourceValue("MenuCategoryFrame")
                    };
                    var catFrameCont = new StackLayout();
                    catFrame.Content = catFrameCont;
                    Container.Children.Add(catFrame);

                    if (pair.Key != "All")
                    {
                        catFrameCont.Children.Add(new Label
                        {
                            Text = pair.Key,
                            Style = (Style)HelperFunctions.GetResourceValue("CategoryHeadLabel")
                        });
                    }
                    if (pair.Value.Any())
                    {
                        this.RenderLinks(pair.Value, catFrameCont);
                    }
                }
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
        }

        private void RenderLinks(List<MobilePagesWraper> collection, StackLayout layout)
        {
            rownum = colnum = 0;
            lastItem = collection.Last();

            try
            {
                Grid grid = this.CreateGrid();
                layout.Children.Add(grid);

                foreach (MobilePagesWraper wrpr in collection)
                {
                    EbMenuItem item = new EbMenuItem(wrpr)
                    {
                        Style = (Style)HelperFunctions.GetResourceValue("MenuItemFrame"),
                        GestureRecognizers = { gesture }
                    };

                    SetGrid(grid, item, wrpr);
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
                Style = (Style)HelperFunctions.GetResourceValue("MenuGrid"),
                RowDefinitions = { new RowDefinition { Height = GridLength.Auto } },
                ColumnDefinitions = {
                        new ColumnDefinition(),
                        new ColumnDefinition(),
                        new ColumnDefinition()
                }
            };
        }

        private void SetGrid(Grid grid, View item, MobilePagesWraper current)
        {
            grid.Children.Add(item, colnum, rownum);

            if (colnum == 2)
            {
                if (current != lastItem)
                {
                    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
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