using System;
using System.Collections.Generic;
using System.Text;

namespace Othello.ViewModel.Dto
{
    public class GameParam
    {
        public GameParam()
        {
        }

        public GameParam(int numberOfPlayers, int numberOfRows, int numberOfColumns)
        {
            this.NumberOfPlayers = numberOfPlayers;
            this.NumberOfRows = numberOfRows;
            this.NumberOfColumns = numberOfColumns;
        }

        public int NumberOfRows { get; set; }
        public int NumberOfColumns { get; set; }
        public int NumberOfPlayers { get; set; }
    }
}
