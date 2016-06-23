using IB = Informatix.MGDS.InfoBar;
using static Informatix.MGDS.Cad;

namespace YunoCad
{
    // Informatix.MGDS.InfoBar.SetEdit はドキュメントに依存するのでここでは実装しない
    public class InfoBar
    {
        public static InfoBar Instance { get; } = new InfoBar();

        InfoBar() { }

        public bool ZLock
        {
            get { return GetInfoBarButton(IB.ZLock); }
            set { InfoBarButton(IB.ZLock, value); }
        }

        public bool HoverHighlight
        {
            get { return GetInfoBarButton(IB.HoverHighlight); }
            set { InfoBarButton(IB.HoverHighlight, value); }
        }

        public bool SnapGuides
        {
            get { return GetInfoBarButton(IB.SnapGuides); }
            set { InfoBarButton(IB.SnapGuides, value); }
        }
    }
}
