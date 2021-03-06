﻿using ExpressBase.Mobile.CustomControls.Layout;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.ViewModels.BaseModels;
using ExpressBase.Mobile.Views.Dynamic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels.Dynamic.ListView
{
    public class StaticListViewModel : DynamicBaseViewModel
    {
        public EbMobileVisualization Visualization { set; get; }

        public SeparatorVisibility ShowRowSeperator => Visualization.XFSeperator();

        private ObservableCollection<EbMobileStaticListItem> items;

        public ObservableCollection<EbMobileStaticListItem> Items
        {
            get => items;
            set
            {
                items = value;
                NotifyPropertyChanged();
            }
        }

        public IntRef ListItemIndex { set; get; }

        private bool isSearchable;

        public bool IsSearchVisible
        {
            get => isSearchable;
            set
            {
                isSearchable = value;
                NotifyPropertyChanged();
            }
        }

        private List<string> searchParameters;

        bool isTapped;

        public Command ItemTappedCommand => new Command<StaticLSFrame>(async (o) => await NavigateToLink(o));

        public Command SearchCommand => new Command<string>((query) => SearchData(query));

        public StaticListViewModel(EbMobilePage page) : base(page)
        {
            Visualization = (EbMobileVisualization)page.Container;
            Items = new ObservableCollection<EbMobileStaticListItem>(Visualization.Items);

            InitSearchable();
        }

        private void InitSearchable()
        {
            if (Visualization.StaticParameters == null || Items == null)
                IsSearchVisible = false;
            else
            {
                searchParameters = Visualization.StaticParameters.FindAll(x => x.EnableSearch).Select(y => y.Name).ToList();
                if (searchParameters != null && searchParameters.Any())
                {
                    IsSearchVisible = true;
                }
            }
        }

        private async Task NavigateToLink(StaticLSFrame item)
        {
            if (isTapped)
                return;
            try
            {
                EbMobilePage page = EbPageHelper.GetPage(item.StaticItem.LinkRefId);

                if (page != null)
                {
                    isTapped = true;

                    if (page.Container is EbMobileVisualization)
                    {
                        EbDataRow row = item.StaticItem.GetAsDataRow();
                        await App.Navigation.NavigateMasterAsync(new ListRender(page, row));
                    }
                    else
                    {
                        ContentPage renderer = EbPageHelper.GetPageByContainer(page);
                        await App.Navigation.NavigateMasterAsync(renderer);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            isTapped = false;
        }

        public void SearchData(string search)
        {
            if (search == null)
                return;

            search = search.ToLower();

            Items.Clear();

            foreach (var p in Visualization.Items)
            {
                List<EbMobileStaticParameter> parameters = p.Parameters?.FindAll(x => searchParameters.Contains(x.Name));

                if (parameters == null || !parameters.Any())
                    continue;

                foreach (var par in parameters)
                {
                    string pvalue = par.Value ?? string.Empty;
                    if (pvalue.ToLower().Contains(search))
                    {
                        Items.Add(p);
                        break;
                    }
                }
            }
        }

        public void SetInitialState()
        {
            if (Items.Count != Visualization.Items.Count)
            {
                Items.Clear();
                Items.AddRange(Visualization.Items);
            }
        }
    }
}
