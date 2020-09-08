using Xamarin.Forms;

namespace ExpressBase.Mobile.Views.Base
{
    public class EbContentPage : ContentPage, IRefreshable
    {
        protected bool IsRendered { set; get; }

        public virtual bool CanRefresh()
        {
            return false;
        }

        public virtual void RefreshPage() { }

        public virtual void UpdateRenderStatus() { }

        protected virtual bool BeforeBackButtonPressed() { return true; }
    }
}
