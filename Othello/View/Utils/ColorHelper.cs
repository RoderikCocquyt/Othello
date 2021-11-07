using Othello.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Othello.View.Utils
{
    internal static class ColorHelper
    {
        internal static Brush GetOppositeColor(Brush currentColor)
        {
            Side currentSide = GetSideFromColor((SolidColorBrush)currentColor);
            Side oppositeSide = currentSide.GetOppositeSide();
            Brush oppositeColor = GetColorFromSide(oppositeSide);

            return oppositeColor;
        }

        internal static SolidColorBrush GetColorFromSide(Side side)
        {
            var color = new SolidColorBrush((Color)ColorConverter.ConvertFromString(side.ToString()));
            return color;
        }

        internal static Side GetSideFromColor(SolidColorBrush color)
        {
            string colorName = GetColorName(color);
            Side side = (Side)Enum.Parse(typeof(Side), colorName);
            return side;
        }

        internal static string GetColorName(SolidColorBrush brush)
        {
            var results = typeof(Colors).GetProperties()
                .Where(p => (Color)p.GetValue(null, null) == brush.Color)
                .Select(p => p.Name);

            return results.Count() > 0 ? results.First() : string.Empty;
        }
    }
}
