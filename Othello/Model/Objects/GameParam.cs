using System;
using System.Collections.Generic;
using System.Text;

namespace Othello.Model.Objects
{
    public class GameParam
    {
        public GameParam()
        {
        }

        public GameParam(int numberOfPlayers, int numberOfRows, int numberOfColumns)
        {
            NumberOfPlayers = numberOfPlayers;
            NumberOfRows = numberOfRows;
            NumberOfColumns = numberOfColumns;
        }

        public int NumberOfRows { get; set; }
        public int NumberOfColumns { get; set; }
        public int NumberOfPlayers { get; set; }
    }
}
