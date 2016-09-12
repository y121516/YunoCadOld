using MC = Informatix.MGDS.Cad;
using IB = Informatix.MGDS.InfoBar;

namespace Yuno.Cad
{
    // Informatix.MGDS.InfoBar.SetEdit はドキュメントに依存するのでここでは実装しない
    public class InfoBar
    {
        public static InfoBar Instance { get; } = new InfoBar();

        InfoBar() { }

        public bool ZLock
        {
            get { return MC.GetInfoBarButton(IB.ZLock); }
            set { MC.InfoBarButton(IB.ZLock, value); }
        }

        public bool HoverHighlight
        {
            get { return MC.GetInfoBarButton(IB.HoverHighlight); }
            set { MC.InfoBarButton(IB.HoverHighlight, value); }
        }

        public bool SnapGuides
        {
            get { return MC.GetInfoBarButton(IB.SnapGuides); }
            set { MC.InfoBarButton(IB.SnapGuides, value); }
        }
    }
}
