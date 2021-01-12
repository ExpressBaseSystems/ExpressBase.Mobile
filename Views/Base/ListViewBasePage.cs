using ExpressBase.Mobile.CustomControls.XControls;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.ViewModels.Dynamic;
using System;

namespace ExpressBase.Mobile.Views.Base
{
    public class PagingMeta
    {
        public string Meta { set; get; }

        public string PageCount { set; get; }
    }

    public class ListViewBasePage : EbContentPage, IListRenderer
    {
        protected int PageCount { set; get; } = 1;

        protected ListViewBaseViewModel ViewModel { set; get; }

        protected bool HasFabLink => ViewModel.Visualization.ShowNewButton;

        protected bool HasLink => ViewModel.Visualization.HasLink();

        protected bool HasFabText => !string.IsNullOrEmpty(ViewModel.Visualization.NewButtonText);

        public async void PagingPrevButton_Clicked(object sender, EventArgs e)
        {
            if (ViewModel.Offset <= 0) return;

            ViewModel.Offset -= ViewModel.Visualization.PageLength;
            PageCount--;
            await ViewModel.RefreshDataAsync();
        }

        public async void PagingNextButton_Clicked(object sender, EventArgs e)
        {
            if (ViewModel.Offset + ViewModel.Visualization.PageLength >= ViewModel.DataCount) return;

            ViewModel.Offset += ViewModel.Visualization.PageLength;
            PageCount++;
            await ViewModel.RefreshDataAsync();
        }

        public PagingMeta GetPagingMeta()
        {
            var meta = new PagingMeta();
            try
            {
                int pageLength = ViewModel.Visualization.PageLength;
                int total = ViewModel.DataCount;
                int offset = ViewModel.Offset + 1;
                int length = pageLength + ViewModel.Offset - 1;

                if (total < pageLength || pageLength + offset > total)
                    length = total;

                meta.Meta = $"{offset} - {length}/{total}";
                meta.PageCount = $"{PageCount}/{(int)Math.Ceiling((double)total / pageLength)}";
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
            return meta;
        }

        public override void RefreshPage()
        {
            this.UpdatePaginationBar();
            this.ToggleDataLength();
        }

        public override void UpdateRenderStatus() => IsRendered = false;

        public override bool CanRefresh() => true;

        protected virtual void UpdatePaginationBar() { }

        protected virtual void ToggleDataLength() { }

        public virtual void ShowAudioFiles(EbPlayButton playButton) { }
    }
}
