using Othello.Model.Enums;
using Othello.Model.Objects;
using Othello.ViewModel.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
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
            bool diskHasColor = !ValidateDisk(dropTarget);
            if (diskHasColor)
            {
                return false;
            }

            var field = dropTarget.Tag as Field;
            bool isAllowedMove = ValidateField(field);

            return isAllowedMove;
        }

        /// <summary>
        /// Checks whether the target disk already has a color.
        /// </summary>
        /// <returns>True when the target disk has no color.</returns>
        private bool ValidateDisk(Ellipse dropTarget)
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

        private bool ValidateField(Field field)
        {
            var surroundingFields = GetSurroundingFields(field);
            if (!surroundingFields.Where(f => f.Side != Side.Empty).Any())
            {
                return false;
            }

            return true;
        }

        private List<Field> GetSurroundingFields(Field field)
        {
            var surroundingFields = new List<Field>();
            int row = field.GridRow;
            int col = field.GridColumn;

            for (int i = row - 1; i <= row + 1; i++)
            {
                for (int j = col - 1; j <= col + 1; j++)
                {
                    bool isThisField = i == row && j == col;
                    if (isThisField)
                    {
                        continue;
                    }
                    
                    var surroundingField = GetField(i, j);
                    if (surroundingField != null)
                    {
                        surroundingFields.Add(surroundingField);
                    }
                }
            }

            return surroundingFields;
        }

        private Field GetField(int gridRow, int gridCol)
        {
            if (gridRow < 0 || gridRow >= param.NumberOfRows
                || gridCol < 0 || gridCol >= param.NumberOfColumns)
            {
                return null;
            }

            return new Field(gridRow, gridCol)
            {
                Side = virtualGrid[gridRow, gridCol]
            };
        }

        internal void SetVirtualGrid(Side[,] virtualGrid)
        {
            this.virtualGrid = virtualGrid;
        }
    }
}
