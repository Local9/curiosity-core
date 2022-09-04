namespace Curiosity.Framework.Shared.SerializedModels
{
    [Serializable]
    public partial class A2
    {
        public int Style;
        public float Opacity;

        public A2() {}

        public A2(int style, float opacity)
        {
            Style = style;
            Opacity = opacity;
        }
    }

    [Serializable]
    public partial class A3 : A2
    {
        public int[] Color = new int[2] {0, 0};

        public A3() { }

        public A3(int style, float opacity, int[] color) : base(style, opacity)
        {
            Style = style;
            Opacity = opacity;
            Color = color;
        }
    }

    [Serializable]
    public partial class Hair
    {
        public int Style;
        public int[] Color = new int[2] { 0, 0 };

        public Hair() { }

        public Hair(int style, int[] color)
        {
            Style = style;
            Color = color;
        }
    }

    [Serializable]
    public partial class Eye
    {
        public int Style;

        public Eye() { }

        public Eye(int style)
        {
            Style = style;
        }
    }

    [Serializable]
    public partial class Ears
    {
        public int Style;
        public int Color;

        public Ears() { }

        public Ears(int style, int color)
        {
            Style = style;
            Color = color;
        }
    }
}
