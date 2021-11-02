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

        internal bool ValidateSkipTurn(Side currentSide)
        {
            throw new NotImplementedException();
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

            bool validMove = CheckSurroundingFields(field, surroundingFields, side);
            return validMove;
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
                    fieldsToFlip = GetNextFields(surroundingField, fieldsToFlip, side, 0, 1);
                }

                // Left
                if (surroundingField.GridRow == field.GridRow && surroundingField.GridColumn < field.GridColumn)
                {
                    fieldsToFlip = GetNextFields(surroundingField, fieldsToFlip, side, 0, -1);
                }

                // Top
                if (surroundingField.GridRow < field.GridRow && surroundingField.GridColumn == field.GridColumn)
                {
                    fieldsToFlip = GetNextFields(surroundingField, fieldsToFlip, side, -1, 0);
                }

                // Bottom
                if (surroundingField.GridRow > field.GridRow && surroundingField.GridColumn == field.GridColumn)
                {
                    fieldsToFlip = GetNextFields(surroundingField, fieldsToFlip, side, 1, 0);
                }

                // Bottom right
                if (surroundingField.GridRow > field.GridRow && surroundingField.GridColumn > field.GridColumn)
                {
                    fieldsToFlip = GetNextFields(surroundingField, fieldsToFlip, side, 1, 1);
                }

                // Bottom left
                if (surroundingField.GridRow > field.GridRow && surroundingField.GridColumn < field.GridColumn)
                {
                    fieldsToFlip = GetNextFields(surroundingField, fieldsToFlip, side, 1, -1);
                }

                // Top left
                if (surroundingField.GridRow < field.GridRow && surroundingField.GridColumn < field.GridColumn)
                {
                    fieldsToFlip = GetNextFields(surroundingField, fieldsToFlip, side, -1, -1);
                }

                // Top right
                if (surroundingField.GridRow < field.GridRow && surroundingField.GridColumn > field.GridColumn)
                {
                    fieldsToFlip = GetNextFields(surroundingField, fieldsToFlip, side, -1, 1);
                }
            }

            // Game rule: 
            // If on your turn you cannot outflank and flip at least one opposing disk,
            // your turn is forfeited and your opponent moves again. => Press button "Skip turn"
            // However, if a move is available to you, you may not forfeit your turn.
            if (!fieldsToFlip.Any())
            {
                return false;
            }

            gameView.FlipDisks(fieldsToFlip, side);
            totalFlips += fieldsToFlip.Count;
            gameView.SwitchSide();

            return true;
        }

        private List<Field> GetNextFields(Field surroundingField,
                List<Field> fieldsToFlip,
                Side side, 
                int rowDiff, 
                int colDiff)
        {
            var nextFields = new List<Field>() { surroundingField };
            Field nextField = new Field(surroundingField.GridRow, surroundingField.GridColumn);

            while (nextField.GridRow > 0 && nextField.GridRow < param.NumberOfRows - 1
                && nextField.GridColumn > 0 && nextField.GridColumn < param.NumberOfColumns - 1)
            {
                nextField = new Field(nextField.GridRow + rowDiff, nextField.GridColumn + colDiff)
                {
                    Side = virtualGrid[nextField.GridRow + rowDiff, nextField.GridColumn + colDiff]
                };

                if (nextField.Side == side)
                {
                    fieldsToFlip.AddRange(nextFields);
                    break;
                }
                else if (nextField.Side != Side.Empty)
                {
                    nextFields.Add(nextField);
                }
                else
                {
                    break;
                }
            }

            return fieldsToFlip;
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
