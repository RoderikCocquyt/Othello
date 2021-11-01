using Othello.Model.Enums;
using Othello.ViewModel.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Othello.ViewModel
{
    public class GameController
    {
        private readonly GameParam param;

        private Side[,] virtualGrid;

        public GameController()
        {
        }

        public GameController(GameParam param)
        {
            this.param = param;
        }

        public bool ValidateDropTarget(Ellipse dropTarget)
        {
            if (dropTarget.Fill == null)
            {
                return true;
            }
            
            var white = (Color)ColorConverter.ConvertFromString(Side.White.ToString());
            var black = (Color)ColorConverter.ConvertFromString(Side.Black.ToString());
            var targetColor = ((SolidColorBrush)dropTarget.Fill).Color;

            bool valid = targetColor != white && targetColor != black;
            return valid;
        }

        internal void SetVirtualGrid(Side[,] virtualGrid)
        {
            this.virtualGrid = virtualGrid;
        }
    }
}
