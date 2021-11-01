using Othello.Model.Enums;
using Othello.Model.Objects;
using Othello.View;
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
        private readonly GameView gameView;
        private readonly GameParam param;

        private Side[,] virtualGrid;

        public GameController(GameView gameView, GameParam param)
        {
            this.gameView = gameView;
            this.param = param;
        }

        public bool ValidateDropTarget(Ellipse dropTarget, Side side)
        {
            bool diskHasColor = !ValidateDisk(dropTarget);
            if (diskHasColor)
            {
                return false;
            }

            var field = dropTarget.Tag as Field;
            bool isAllowedMove = ValidateField(field, side);

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

        private bool ValidateField(Field field, Side side)
        {
            var surroundingFields = GetSurroundingFields(field);

            bool allSurroundingFieldsAreEmpty = !surroundingFields.Where(f => f.Side != Side.Empty).Any();
            if (allSurroundingFieldsAreEmpty)
            {
                return false;
            }

            CheckSurroundingFields(field, surroundingFields, side);

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

        private bool CheckSurroundingFields(Field field, List<Field> surroundingFields, Side side)
        {
            //We only need the surrounding disks of the opposite side
            var oppositeFields = surroundingFields.Where(f => f.Side != Side.Empty && f.Side != side).ToList();
            if (!oppositeFields.Any())
            {
                return false;
            }

            int totalFlips = 0;
            foreach (var surroundingField in oppositeFields)
            {
                if (surroundingField.GridRow == field.GridRow && surroundingField.GridColumn > field.GridColumn)
                {
                    var rightFields = new List<Field>() { surroundingField };
                    int col = surroundingField.GridColumn;
                    Field nextField = new Field(surroundingField.GridRow, surroundingField.GridColumn);

                    while(nextField.Side != surroundingField.Side && col < param.NumberOfColumns)
                    {
                        nextField = new Field(nextField.GridRow, nextField.GridColumn + 1)
                        {
                            Side = virtualGrid[field.GridRow, nextField.GridColumn + 1]
                        };

                        if (nextField.Side != Side.Empty)
                        {
                            rightFields.Add(nextField);
                        }
                        else
                        {
                            break;
                        }
                    }

                    gameView.FlipDisks(rightFields, side);
                    totalFlips += rightFields.Count;
                }
            }

            return true;
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
