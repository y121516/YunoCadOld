using M = Informatix.MGDS;
using MC = Informatix.MGDS.Cad;

namespace Yuno.Cad
{
    public class Preference
    {
        public static Preference Instance { get; } = new Preference();
        Preference() { }

        public ColorNumber BackgroundColorNumber
        {
            get { return (ColorNumber)MC.GetPrefBackgroundCol(); }
            set { MC.PrefBackgroundCol((int)value); }
        }

        public M.Backup Backup
        {
            get { return MC.GetPrefBackup(); }
            set { MC.PrefBackup(value); }
        }
        public static M.Backup BackupMinute(int minute) => (M.Backup)(-minute);

        public string BlankManFilePath
        {
            get
            {
                string path;
                MC.GetPrefBlankManFile(out path);
                return path;
            }
            set { MC.PrefBlankManFile(value); }
        }

        public string BmpDibEditorPath
        {
            get
            {
                string path;
                MC.GetPrefBmpDibEditor(out path);
                return path;
            }
            set { MC.PrefBmpDibEditor(value); }
        }

        public bool CheckOnOpen
        {
            get { return MC.GetPrefCheckOnOpen(); }
            set { MC.PrefCheckOnOpen(value); }
        }

        public ColorTable ColorTable { get; } = ColorTable.Instance;

        ColorRgba SelectColor
        {
            get { return ColorTable[ColorNumber.Select]; }
            set { ColorTable[ColorNumber.Select] = value; }
        }

        ColorRgba BackgroundColor
        {
            get { return ColorTable[ColorNumber.Background]; }
            set { ColorTable[ColorNumber.Background] = value; }
        }
    }

    public class ColorTable
    {
        internal static ColorTable Instance { get; } = new ColorTable();
        ColorTable() { }

        public ColorRgba this[ColorNumber index]
        {
            get
            {
                int red, green, blue, alpha;
                MC.GetPrefColourEx((int)index, out red, out green, out blue, out alpha);
                return new ColorRgba(red, green, blue, alpha);
            }
            set { MC.PrefColourEx((int)index, value.Red, value.Green, value.Blue, value.Alpha); }
        }
    }
}
