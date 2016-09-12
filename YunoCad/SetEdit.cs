namespace Yuno.Cad
{
    public class SetEdit
    {
        public static SetEdit Instance { get; } = new SetEdit();

        SetEdit() { }

        SetEditColour Colour { get; } = SetEditColour.Instance;
    }

    class SetEditColour
    {
        public static SetEditColour Instance { get; } = new SetEditColour();

        bool Not { get; set; }

        SetEditColour() { }
        internal SetEditColour(string colour) { }
        //{
        //    get
        //    {
        //        var colour = "";
        //        GetSetEditColour(out colour);
        //        return new SetEditColour(colour);
        //    }
        //    set
        //    {
        //        SetEditColour(value.ToString());
        //    }
        //}

    }

    class SetEditLineStyle
    {
        bool Not { get; set; }
        bool IsAll { get; }
        string LineStyle { get; }
    }

    class SetEditMaterial
    {
        bool Not { get; set; }
        bool IsAll { get; }
        string Material { get; }
    }

    class SetEditIncludingList
    {

    }

    class SetEditCharStyle
    {
        bool Not { get; set; }
        bool IsAll { get; }
        string CharStyle { get; }
    }
}
