using ExpressBase.Mobile.CustomControls.Views;
using ExpressBase.Mobile.Enums;

namespace ExpressBase.Mobile
{
    public class EbMobileAudioInput : EbMobileControl, INonPersistControl
    {
        public int MaxDUration { set; get; }

        public virtual bool MultiSelect { set; get; }

        public virtual bool EnableEdit { set; get; }

        private AudioRecorder recorder;

        public override void InitXControl(FormMode mode, NetworkMode network)
        {
            base.InitXControl(mode, network);

            recorder = new AudioRecorder(this);
            this.XControl = recorder;
        }
    }
}
