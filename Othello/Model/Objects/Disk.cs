using Othello.Model.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Othello.Model.Objects
{
    public class Disk
    {
        public Disk(Side side)
        {
            this.Side = side;
        }

        public Side Side { get; set; }
        public Field Field { get; set; }
        public string ImageSource { get; set; }
    }
}
