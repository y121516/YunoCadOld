namespace Yuno.Cad
{
    interface IColour { }

    struct ColourNumber : IColour
    {
        short Number { get; }

        ColourNumber(short number)
        {
            Number = number;
        }

        public override string ToString()
        {
            if (Number == 0) return "By phase";
            return Number.ToString();
        }
    }

    struct ColourRGB : IColour
    {
        byte Red { get; }
        byte Green { get; }
        byte Blue { get; }

        ColourRGB(byte red, byte green, byte blue)
        {
            Red = red;
            Green = green;
            Blue = blue;
        }

        public override string ToString() => $"{Red}/{Green}/{Blue}";
    }

    struct ColourRGBA : IColour
    {
        byte Red { get; }
        byte Green { get; }
        byte Blue { get; }
        byte Alpha { get; }

        ColourRGBA(byte red, byte green, byte blue, byte alpha)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Alpha = alpha;
        }

        public override string ToString() => $"{Red}/{Green}/{Blue}/{Alpha}";
    }
}
