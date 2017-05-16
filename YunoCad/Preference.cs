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
                MC.GetPrefBlankManFile(out string path);
                return path;
            }
            set { MC.PrefBlankManFile(value); }
        }

        public string BmpDibEditorPath
        {
            get
            {
                MC.GetPrefBmpDibEditor(out string path);
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
                MC.GetPrefColourEx((int)index, out int red, out int green, out int blue, out int alpha);
                return new ColorRgba(red, green, blue, alpha);
            }
            set { MC.PrefColourEx((int)index, value.Red, value.Green, value.Blue, value.Alpha); }
        }
    }
}
