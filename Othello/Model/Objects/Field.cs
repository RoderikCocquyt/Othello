using Othello.Model.Enums;
using System;

namespace Othello.Model.Objects
{
    public class Field
    {
        public Field()
        {
            this.GridRow = -1;
            this.GridColumn = -1;
            
            this.Row = "0";
            this.Column = ((char)('A' - 1)).ToString();
        }

        /// <summary>
        /// Creates a Field object based on the zero-based index of grid row and column.
        /// </summary>
        public Field(int gridRow, int gridCol)
        {
            this.GridRow = gridRow;
            this.GridColumn = gridCol;
            
            this.Row = (gridRow + 1).ToString();
            this.Column = ((char)('A' + gridCol)).ToString();
        }

        /// <summary>
        /// Creates a Field object based on the one-based and A-based index of grid row and column.
        /// </summary>
        public Field(string row, string column)
        {
            if (row.Length != 1 || column.Length != 1)
            {
                throw new InvalidOperationException();
            }
            
            this.GridRow = int.Parse(row) - 1;
            this.GridColumn = char.Parse(column) - 'A';

            this.Row = row;
            this.Column = column;
        }

        // Index is zero-based
        public int GridRow { get; set; }
        public int GridColumn { get; set; }

        // Index is one-based (A for columns)
        public string Row { get; set; }
        public string Column { get; set; }
        public string Name => this.Column + this.Row;

        public Side Side { get; set; } = Side.Empty;

        public static string BuildName(int row, int col)
        {
            char colCapital = (char)('A' + col);
            return colCapital.ToString() + (row + 1).ToString();
        }

        public static Field ParseName(string name)
        {
            string row = name.Substring(1, 1);
            string column = name.Substring(0, 1);

            return new Field(row, column);
        }
    }
}
