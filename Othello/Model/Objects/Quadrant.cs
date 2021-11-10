using System;
using System.Collections.Generic;
using System.Text;

namespace Othello.Model.Objects
{
    public class Quadrant
    {
        public Quadrant(int firstRow, int lastRow, int firstColumn, int lastColumn)
        {
            FirstRow = firstRow;
            LastRow = lastRow;
            FirstColumn = firstColumn;
            LastColumn = lastColumn;
        }
        
        public int FirstRow { get; set; }
        public int LastRow { get; set; }
        public int FirstColumn { get; set; }
        public int LastColumn { get; set; }

        public bool ContainsField(Field field)
        {
            if (field.GridRow >= FirstRow && field.GridRow <= LastRow &&
                field.GridColumn >= FirstColumn && field.GridColumn <= LastColumn)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
