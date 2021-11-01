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
            var fieldsToFlip = new List<Field>();
            foreach (var surroundingField in oppositeFields)
            {
                // Right
                if (surroundingField.GridRow == field.GridRow && surroundingField.GridColumn > field.GridColumn)
                {
                    var rightFields = new List<Field>() { surroundingField };
                    int col = surroundingField.GridColumn;
                    Field nextField = new Field(surroundingField.GridRow, surroundingField.GridColumn);

                    while(nextField.Side != surroundingField.Side && nextField.GridColumn < param.NumberOfColumns)
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

                    fieldsToFlip.AddRange(rightFields);
                }

                // Left
                if (surroundingField.GridRow == field.GridRow && surroundingField.GridColumn < field.GridColumn)
                {
                    var leftFields = new List<Field>() { surroundingField };
                    int col = surroundingField.GridColumn;
                    Field nextField = new Field(surroundingField.GridRow, surroundingField.GridColumn);

                    while (nextField.Side != surroundingField.Side && nextField.GridColumn >= 0)
                    {
                        nextField = new Field(nextField.GridRow, nextField.GridColumn - 1)
                        {
                            Side = virtualGrid[field.GridRow, nextField.GridColumn - 1]
                        };

                        if (nextField.Side != Side.Empty)
                        {
                            leftFields.Add(nextField);
                        }
                        else
                        {
                            break;
                        }
                    }

                    fieldsToFlip.AddRange(leftFields);
                }

                // Top
                if (surroundingField.GridRow > field.GridRow && surroundingField.GridColumn == field.GridColumn)
                {
                    var topFields = new List<Field>() { surroundingField };
                    int row = surroundingField.GridRow;
                    Field nextField = new Field(surroundingField.GridRow, surroundingField.GridColumn);

                    while (nextField.Side != surroundingField.Side && nextField.GridRow >= 0)
                    {
                        nextField = new Field(nextField.GridRow - 1, nextField.GridColumn)
                        {
                            Side = virtualGrid[field.GridRow - 1, nextField.GridColumn]
                        };

                        if (nextField.Side != Side.Empty)
                        {
                            topFields.Add(nextField);
                        }
                        else
                        {
                            break;
                        }
                    }

                    fieldsToFlip.AddRange(topFields);
                }

                // Bottom
                if (surroundingField.GridRow < field.GridRow && surroundingField.GridColumn == field.GridColumn)
                {
                    var bottomFields = new List<Field>() { surroundingField };
                    int row = surroundingField.GridRow;
                    Field nextField = new Field(surroundingField.GridRow, surroundingField.GridColumn);

                    while (nextField.Side != surroundingField.Side && nextField.GridRow < param.NumberOfRows)
                    {
                        nextField = new Field(nextField.GridRow + 1, nextField.GridColumn)
                        {
                            Side = virtualGrid[field.GridRow + 1, nextField.GridColumn]
                        };

                        if (nextField.Side != Side.Empty)
                        {
                            bottomFields.Add(nextField);
                        }
                        else
                        {
                            break;
                        }
                    }

                    fieldsToFlip.AddRange(bottomFields);
                }

                // Bottom right
                if (surroundingField.GridRow > field.GridRow && surroundingField.GridColumn > field.GridColumn)
                {
                    var bottomRightFields = new List<Field>() { surroundingField };
                    int row = surroundingField.GridRow;
                    int col = surroundingField.GridColumn;
                    Field nextField = new Field(row, col);

                    while (nextField.Side != surroundingField.Side &&
                            nextField.GridRow < param.NumberOfRows && nextField.GridColumn < param.NumberOfColumns)
                    {
                        nextField = new Field(nextField.GridRow + 1, nextField.GridColumn + 1)
                        {
                            Side = virtualGrid[field.GridRow + 1, nextField.GridColumn + 1]
                        };

                        if (nextField.Side != Side.Empty)
                        {
                            bottomRightFields.Add(nextField);
                        }
                        else
                        {
                            break;
                        }
                    }

                    fieldsToFlip.AddRange(bottomRightFields);
                }

                // Bottom left
                if (surroundingField.GridRow > field.GridRow && surroundingField.GridColumn < field.GridColumn)
                {
                    var bottomLeftFields = new List<Field>() { surroundingField };
                    int row = surroundingField.GridRow;
                    int col = surroundingField.GridColumn;
                    Field nextField = new Field(row, col);

                    while (nextField.Side != surroundingField.Side &&
                            nextField.GridRow < param.NumberOfRows && nextField.GridColumn >= 0)
                    {
                        nextField = new Field(nextField.GridRow + 1, nextField.GridColumn - 1)
                        {
                            Side = virtualGrid[field.GridRow + 1, nextField.GridColumn - 1]
                        };

                        if (nextField.Side != Side.Empty)
                        {
                            bottomLeftFields.Add(nextField);
                        }
                        else
                        {
                            break;
                        }
                    }

                    fieldsToFlip.AddRange(bottomLeftFields);
                }

                // Top left
                if (surroundingField.GridRow < field.GridRow && surroundingField.GridColumn < field.GridColumn)
                {
                    var topLeftFields = new List<Field>() { surroundingField };
                    int row = surroundingField.GridRow;
                    int col = surroundingField.GridColumn;
                    Field nextField = new Field(row, col);

                    while (nextField.Side != surroundingField.Side &&
                            nextField.GridRow >= 0 && nextField.GridColumn >= 0)
                    {
                        nextField = new Field(nextField.GridRow - 1, nextField.GridColumn - 1)
                        {
                            Side = virtualGrid[field.GridRow - 1, nextField.GridColumn - 1]
                        };

                        if (nextField.Side != Side.Empty)
                        {
                            topLeftFields.Add(nextField);
                        }
                        else
                        {
                            break;
                        }
                    }

                    fieldsToFlip.AddRange(topLeftFields);
                }

                // Top right
                if (surroundingField.GridRow < field.GridRow && surroundingField.GridColumn > field.GridColumn)
                {
                    var topRightFields = new List<Field>() { surroundingField };
                    int row = surroundingField.GridRow;
                    int col = surroundingField.GridColumn;
                    Field nextField = new Field(row, col);

                    while (nextField.Side != surroundingField.Side &&
                            nextField.GridRow >= 0 && nextField.GridColumn < param.NumberOfColumns)
                    {
                        nextField = new Field(nextField.GridRow - 1, nextField.GridColumn + 1)
                        {
                            Side = virtualGrid[field.GridRow - 1, nextField.GridColumn + 1]
                        };

                        if (nextField.Side != Side.Empty)
                        {
                            topRightFields.Add(nextField);
                        }
                        else
                        {
                            break;
                        }
                    }

                    fieldsToFlip.AddRange(topRightFields);
                }
            }

            gameView.FlipDisks(fieldsToFlip, side);
            totalFlips += fieldsToFlip.Count;

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
