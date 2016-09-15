using M = Informatix.MGDS;

namespace Yuno.Cad
{
    public enum ColorNumber
    {
        Select = -1,
        Background = 0,
        Col1 = 1,
        Col2 = 2,
        Col3 = 3,
        Col4 = 4,
        Col5 = 5,
        Col6 = 6,
        Col7 = 7,
        Col8 = 8,
        Col9 = 9,
        Col10 = 10,
        Col11 = 11,
        Col12 = 12,
        Col13 = 13,
        Col14 = 14,
        Col15 = 15,
        Col16 = 16,
        Col17 = 17,
        Col18 = 18,
        Col19 = 19,
        Col20 = 20,
        Col21 = 21,
        Col22 = 22,
        Col23 = 23,
        Col24 = 24,
        Col25 = 25,
        Col26 = 26,
        Col27 = 27,
        Col28 = 28,
        Col29 = 29,
        Col30 = 30,
        Max = 256,
    }
    static class ColorNumberExtension
    {
    }

    public struct ColorRgb
    {
        public byte Red { get; }
        public byte Green { get; }
        public byte Blue { get; }

        public ColorRgb(int red, int green, int blue)
        {
            Red = (byte)red;
            Green = (byte)green;
            Blue = (byte)blue;
        }

        public override string ToString() => $"{Red}/{Green}/{Blue}";
    }

    public struct ColorRgba
    {
        public byte Red { get; }
        public byte Green { get; }
        public byte Blue { get; }
        public byte Alpha { get; }

        public ColorRgba(int red, int green, int blue, int alpha)
        {
            Red = (byte)red;
            Green = (byte)green;
            Blue = (byte)blue;
            Alpha = (byte)alpha;
        }

        public override string ToString() => $"{Red}/{Green}/{Blue}/{Alpha}";
    }
}
