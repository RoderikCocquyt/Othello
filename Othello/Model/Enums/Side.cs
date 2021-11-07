using System;
using System.Collections.Generic;
using System.Text;

namespace Othello.Model.Enums
{
    public enum Side
    {
        Empty = 0, 
        Black = 1, 
        White = 2
    }

    public static class SideExtensions
    {
        public static Side GetOppositeSide(this Side currentSide)
        {
            bool currentSideIsBlack = (int)currentSide == 1;
            Side oppositeSide = currentSideIsBlack ? Side.White : Side.Black;

            return oppositeSide;
        }
    }
}
