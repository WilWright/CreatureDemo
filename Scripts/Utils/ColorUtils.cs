using UnityEngine;

namespace Utils
{
    public static class ColorUtils
    {
        public static Color RED        => Color.red;
        public static Color ORANGE     => new(1.00f, 0.50f, 0.00f);
        public static Color YELLOW     => Color.yellow;
        public static Color GREEN      => Color.green;
        public static Color CYAN       => Color.cyan;
        public static Color BLUE       => Color.blue;
        public static Color PURPLE     => new(0.50f, 0.00f, 1.00f);
        public static Color PINK       => new(1.00f, 0.00f, 1.00f);
        public static Color WHITE      => Color.white;
        public static Color BLACK      => Color.black;
        public static Color LIGHT_GRAY => new(0.75f, 0.75f, 0.75f);
        public static Color GRAY       => new(0.50f, 0.50f, 0.50f);
        public static Color DARK_GRAY  => new(0.25f, 0.25f, 0.25f);
        public static Color BROWN      => new(0.50f, 0.30f, 0.00f);
        public static Color CLEAR      => Color.clear;

        public static Color SetAlpha(this Color c, float alpha)
        {
            c.a = alpha;
            return c;
        }
    }
}
