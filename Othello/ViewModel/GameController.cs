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
        private readonly GameParam param;

        private HashSet<Side> possibleSkips = new HashSet<Side>();
        private Dictionary<Side, int> scores = new Dictionary<Side, int>();

        public GameController(GameParam param)
        {
            this.param = param;
            VirtualGrid = new Side[param.NumberOfRows, param.NumberOfColumns];
            Scores = new Dictionary<Side, int>() { { Side.Black, 0 }, { Side.White, 0 } };
        }

        internal Side[,] VirtualGrid { get; set; }
        internal List<Field> FieldsToFlip { get; set; }
        internal Dictionary<Side, int> Scores
        {
            get
            {
                int totalDisksBlack = 0;
                int totalDisksWhite = 0;

                foreach (Side side in VirtualGrid)
                {
                    if (side == Side.Black)
                    {
                        totalDisksBlack++;
                    }

                    if (side == Side.White)
                    {
                        totalDisksWhite++;
                    }
                }

                scores[Side.Black] = totalDisksBlack;
                scores[Side.White] = totalDisksWhite;
                return scores;
            }

            set => scores = value;
        }

        internal bool ValidateDropTarget(Ellipse dropTarget, Side side)
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
        /// A turn can't be skipped when a valid move can be made.
        /// </summary>
        /// <param name="currentSide">The color of the player who wants to skip his or her turn.</param>
        /// <returns>True when the turn can be skipped.</returns>
        internal bool ValidateSkipTurn(Side currentSide)
        {
            for (int row = 0; row < VirtualGrid.GetLength(0); row++)
            {
                for (int col = 0; col < VirtualGrid.GetLength(1); col++)
                {
                    // Only check empty fields as all the other fields are occupied already.
                    if (VirtualGrid[row, col] != Side.Empty)
                    {
                        continue;
                    }

                    var field = new Field(row, col) { Side = currentSide };
                    bool aMoveIsPossible = ValidateField(field, currentSide);

                    if (aMoveIsPossible)
                    {
                        return false;
                    }
                }
            }

            // No moves are possible -> it's valid to skip the turn.
            return true;
        }

        /// <summary>
        /// Stores the sides who have skipped a turn at least once.
        /// </summary>
        internal void UpdateSkips(Side side)
        {
            if (!possibleSkips.Contains(side))
            {
                possibleSkips.Add(side);
            }
        }

        /// <summary>
        /// Game rule: the game ends when both players are'nt able to move 
        /// (i.e. have skipped a turn).
        /// </summary>
        /// <returns>True when the game is finished.</returns>
        internal bool CheckSkips()
        {
            return possibleSkips.Contains(Side.Black) && possibleSkips.Contains(Side.White);
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

        private bool ValidateField(Field field, Side currentSide)
        {
            var surroundingFields = GetSurroundingFields(field);

            bool allSurroundingFieldsAreEmpty = !surroundingFields.Where(f => f.Side != Side.Empty).Any();
            if (allSurroundingFieldsAreEmpty)
            {
                return false;
            }

            bool validMove = CheckSurroundingFields(field, surroundingFields, currentSide);
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

        private bool CheckSurroundingFields(Field field, List<Field> surroundingFields, Side currentSide)
        {
            // We only need the surrounding disks of the opposite side
            var oppositeFields = surroundingFields.Where(f => f.Side != Side.Empty && f.Side != currentSide).ToList();
            if (!oppositeFields.Any())
            {
                return false;
            }

            // Reset the collection
            FieldsToFlip = new List<Field>();

            foreach (var surroundingField in oppositeFields)
            {
                // Right
                if (surroundingField.GridRow == field.GridRow && surroundingField.GridColumn > field.GridColumn)
                {
                    FieldsToFlip = GetNextFields(surroundingField, FieldsToFlip, currentSide, 0, 1,
                        0, param.NumberOfRows - 1, 1, param.NumberOfColumns - 2);
                }

                // Left
                if (surroundingField.GridRow == field.GridRow && surroundingField.GridColumn < field.GridColumn)
                {
                    FieldsToFlip = GetNextFields(surroundingField, FieldsToFlip, currentSide, 0, -1,
                        0, param.NumberOfRows - 1, 1, param.NumberOfColumns - 2);
                }

                // Top
                if (surroundingField.GridRow < field.GridRow && surroundingField.GridColumn == field.GridColumn)
                {
                    FieldsToFlip = GetNextFields(surroundingField, FieldsToFlip, currentSide, -1, 0,
                        1, param.NumberOfRows - 2, 0, param.NumberOfColumns - 1);
                }

                // Bottom
                if (surroundingField.GridRow > field.GridRow && surroundingField.GridColumn == field.GridColumn)
                {
                    FieldsToFlip = GetNextFields(surroundingField, FieldsToFlip, currentSide, 1, 0,
                        1, param.NumberOfRows - 2, 0, param.NumberOfColumns - 1);
                }

                // Bottom right
                if (surroundingField.GridRow > field.GridRow && surroundingField.GridColumn > field.GridColumn)
                {
                    FieldsToFlip = GetNextFields(surroundingField, FieldsToFlip, currentSide, 1, 1,
                        1, param.NumberOfRows - 2, 1, param.NumberOfColumns - 2);
                }

                // Bottom left
                if (surroundingField.GridRow > field.GridRow && surroundingField.GridColumn < field.GridColumn)
                {
                    FieldsToFlip = GetNextFields(surroundingField, FieldsToFlip, currentSide, 1, -1,
                        1, param.NumberOfRows - 2, 1, param.NumberOfColumns - 2);
                }

                // Top left
                if (surroundingField.GridRow < field.GridRow && surroundingField.GridColumn < field.GridColumn)
                {
                    FieldsToFlip = GetNextFields(surroundingField, FieldsToFlip, currentSide, -1, -1,
                        1, param.NumberOfRows - 2, 1, param.NumberOfColumns - 2);
                }

                // Top right
                if (surroundingField.GridRow < field.GridRow && surroundingField.GridColumn > field.GridColumn)
                {
                    FieldsToFlip = GetNextFields(surroundingField, FieldsToFlip, currentSide, -1, 1,
                        1, param.NumberOfRows - 2, 1, param.NumberOfColumns - 2);
                }
            }

            // Game rule: 
            // If on your turn you cannot outflank and flip at least one opposing disk,
            // your turn is forfeited and your opponent moves again. => Press button "Skip turn"
            // However, if a move is available to you, you may not forfeit your turn.
            if (!FieldsToFlip.Any())
            {
                return false;
            }

            return true;
        }

        private List<Field> GetNextFields(Field surroundingField,
                List<Field> fieldsToFlip,
                Side currentSide, 
                int rowDiff, 
                int colDiff,
                int minRow,
                int maxRow,
                int minCol,
                int maxCol)
        {
            var nextFields = new List<Field>();

            // We've only selected surrounding fields of the opposite color.
            // Therefore, the first field, the surrounding field, is certainly of the opposite color.
            Field nextField = new Field(surroundingField.GridRow, surroundingField.GridColumn)
            {
                Side = surroundingField.Side 
            };
            nextFields.Add(nextField);

            while (nextField.GridRow >= minRow && nextField.GridRow <= maxRow
                && nextField.GridColumn >= minCol && nextField.GridColumn <= maxCol)
            {
                nextField = new Field(nextField.GridRow + rowDiff, nextField.GridColumn + colDiff)
                {
                    Side = VirtualGrid[nextField.GridRow + rowDiff, nextField.GridColumn + colDiff]
                };

                if (nextField.Side == currentSide)
                {
                    fieldsToFlip.AddRange(nextFields);
                    break;
                }
                else if (nextField.Side != Side.Empty) // = opposite side
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
                Side = VirtualGrid[gridRow, gridCol]
            };
        }
    }
}
