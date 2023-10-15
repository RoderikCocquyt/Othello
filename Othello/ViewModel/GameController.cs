using Othello.Model.Enums;
using Othello.Model.Objects;
using Othello.View;
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
        private readonly HashSet<Side> possibleSkips = new HashSet<Side>();

        private Side[,] virtualGrid;

        public GameController(GameParam param)
        {
            this.param = param;
            InitializeGame();
        }

        internal List<Field> FieldsToFlip { get; private set; } = new List<Field>();

        internal Dictionary<Side, int> Scores { get; private set; } = new Dictionary<Side, int>();

        internal bool ValidateDropTarget(Ellipse dropTarget, Side side)
        {
            if (!IsEmpty(dropTarget))
            {
                return false;
            }

            Field field = dropTarget.Tag as Field;
            bool isValidMove = ValidateField(field, side);

            return isValidMove;
        }

        /// <summary>
        /// Validates if a turn can be skipped.
        /// </summary>
        /// <param name="currentSide">The color of the player who wants to skip his or her turn.</param>
        /// <returns>True when the turn can be skipped.</returns>
        /// <remarks>A turn can't be skipped when a valid move can be made.</remarks>
        internal bool ValidateSkipTurn(Side currentSide)
        {
            for (int row = 0; row < virtualGrid.GetLength(0); row++)
            {
                for (int col = 0; col < virtualGrid.GetLength(1); col++)
                {
                    // Only check empty fields as all the other fields are occupied already.
                    if (virtualGrid[row, col] != Side.Empty)
                    {
                        continue;
                    }

                    Field field = new Field(row, col) { Side = currentSide };
                    bool aMoveIsPossible = ValidateField(field, currentSide);

                    if (aMoveIsPossible)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Stores the sides who have skipped a turn at least once.
        /// </summary>
        internal void UpdateSkips(Side side, bool skipped)
        {
            if (skipped && !possibleSkips.Contains(side))
            {
                possibleSkips.Add(side);
            }
            else if (!skipped && possibleSkips.Contains(side))
            {
                possibleSkips.Remove(side);
            }
        }

        /// <summary>
        /// Sets a field in the virtual grid to the designated side.
        /// </summary>
        /// <param name="side">The field to update.</param>
        internal void UpdateVirtualGridField(Field field, Side side)
        {
            virtualGrid[field.GridRow, field.GridColumn] = side;
        }

        /// <summary>
        /// Checks whether both players have skipped their turn.
        /// </summary>
        /// <returns>True when the game is finished.</returns>
        /// <remarks>
        /// Game rule: the game ends when both players are'nt able to move 
        /// (i.e. have skipped a turn).
        /// </remarks>
        internal bool CheckSkips()
        {
            return possibleSkips.Contains(Side.Black) && possibleSkips.Contains(Side.White);
        }

        internal Dictionary<Side, int> GetScores()
        {
            CalculateScores();
            return Scores;
        }

        private void CalculateScores()
        {
            int totalDisksBlack = 0;
            int totalDisksWhite = 0;

            foreach (Side side in virtualGrid)
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

            Scores[Side.Black] = totalDisksBlack;
            Scores[Side.White] = totalDisksWhite;
        }

        private void InitializeGame()
        {
            virtualGrid = new Side[param.NumberOfRows, param.NumberOfColumns];
            Scores = new Dictionary<Side, int>() { { Side.Black, 0 }, { Side.White, 0 } };
        }

        /// <summary>
        /// Checks whether the target disk already has a color.
        /// </summary>
        /// <returns>True when the target disk has no color.</returns>
        private bool IsEmpty(Ellipse dropTarget)
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

        /// <summary>
        /// Validates the field as a valid move.
        /// </summary>
        /// <remarks>
        /// If on your turn you cannot outflank and flip at least one opposing disc,
        /// your turn is forfeited and your opponent moves again.
        /// However, if a move is available to you, you may not forfeit your turn.
        /// </remarks>
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
                    if (surroundingField is not InvalidField)
                    {
                        surroundingFields.Add(surroundingField);
                    }
                }
            }

            return surroundingFields;
        }

        private bool CheckSurroundingFields(Field field, List<Field> surroundingFields, Side currentSide)
        {
            // We only need the surrounding disks of the opponent
            var fieldsOpponent = surroundingFields.Where(f => f.Side != Side.Empty && f.Side != currentSide).ToList();
            if (!fieldsOpponent.Any())
            {
                return false;
            }

            // Reset the collection
            FieldsToFlip = new List<Field>();

            foreach (var surroundingField in fieldsOpponent)
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

        /// <summary>
        /// Checks the next fields in the specified direction.
        /// </summary>
        /// <param name="surroundingField">The first field in that direction.</param>
        /// <param name="fieldsToFlip">The collection of disks that will change sides.</param>
        /// <returns>The new collection of disks that will change sides.</returns>
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
            Field nextField = new Field(surroundingField.GridRow, surroundingField.GridColumn)
            {
                Side = surroundingField.Side 
            };

            // We've only selected surrounding fields of the opposite color.
            // Therefore, the first surrounding field (var nextField) is certainly of the opposite color.
            Side oppositeSide = nextField.Side;
            nextFields.Add(nextField);

            while (nextField.GridRow >= minRow && nextField.GridRow <= maxRow &&
                    nextField.GridColumn >= minCol && nextField.GridColumn <= maxCol)
            {
                nextField = new Field(nextField.GridRow + rowDiff, nextField.GridColumn + colDiff)
                {
                    Side = virtualGrid[nextField.GridRow + rowDiff, nextField.GridColumn + colDiff]
                };

                bool closingDiskCurrentSideMet = nextField.Side == currentSide;
                if (closingDiskCurrentSideMet)
                {
                    fieldsToFlip.AddRange(nextFields);
                    break;
                }
                else if (nextField.Side == oppositeSide)
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
                return new InvalidField();
            }

            return new Field(gridRow, gridCol)
            {
                Side = virtualGrid[gridRow, gridCol]
            };
        }
    }
}
