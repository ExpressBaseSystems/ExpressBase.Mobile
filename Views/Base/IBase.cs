﻿using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.CustomControls.XControls;
using System.Collections.Generic;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Views.Base
{
    public interface IRefreshable
    {
        void RefreshPage();

        void UpdateRenderStatus();

        bool CanRefresh();
    }

    public interface IToolBarHandler
    {
        bool DeviceBackButtonPressed();
    }

    public interface IDynamicContent
    {
        Dictionary<string, string> PageContent { get; }

        void OnDynamicContentRendering();
    }

    public interface IFormRenderer
    {
        void ShowFullScreenImage(ImageSource source);
    }

    public interface IListRenderer
    {
        void ShowAudioFiles(EbPlayButton playButton);

        void ShowFullScreenImage(ImageSource source);
    }

    public interface IDashBoardRenderer
    {

    }

    public interface IMasterPage
    {
        void UpdateMasterLayout();

        EbCPLayout GetCurrentLayout();
    }
}
