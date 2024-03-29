﻿using Othello.Model.Enums;
using Othello.Model.Objects;
using Othello.View.Utils;
using Othello.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Othello.View
{
    /// <summary>
    /// Interaction logic for GameView.xaml
    /// </summary>
    public partial class GameView : Window
    {
        private const int NumberOfRows = 8;
        private const int NumberOfColumns = 8;
        private const int RectangleSize = 50;
        private const int CircleSize = 40;

        private readonly GameParam param;
        private readonly GameController controller;

        private Ellipse previousTargetDisk;
        private List<Quadrant> quadrants = new List<Quadrant>();
        private Dictionary<Side, int> scores;

        public GameView()
        {
            InitializeComponent();
        }

        public GameView(int numberOfPlayers)
            : this()
        {
            this.param = new GameParam(numberOfPlayers, NumberOfRows, NumberOfColumns);
            this.controller = new GameController(this.param);

            BuildGrid();
            InitializeGrid();
            ToggleButtons(enableGameButtons: true);
        }

        internal Ellipse SourceDisk { get; set; }

        private void BuildGrid()
        {
            CreateColumns();
            CreateRows();
            CreateFields();
        }

        private void CreateColumns()
        {
            for (int col = 0; col < NumberOfColumns; col++)
            {
                var newCol = new ColumnDefinition()
                {
                    Width = new GridLength(RectangleSize),
                };
                grdGame.ColumnDefinitions.Add(newCol);
            }
        }

        private void CreateRows()
        {
            for (int row = 0; row < NumberOfRows; row++)
            {
                var newRow = new RowDefinition()
                {
                    Height = new GridLength(RectangleSize),
                };
                grdGame.RowDefinitions.Add(newRow);
            }
        }

        private void CreateFields()
        {
            for (int row = 0; row < NumberOfRows; row++)
            {
                for (int col = 0; col < NumberOfColumns; col++)
                {
                    var field = new Field(row, col);
                    var grdField = new Grid()
                    {
                        AllowDrop = true,
                    };
                    grdField.Drop += Ellipse_Drop;

                    var rect = new Rectangle()
                    {
                        Name = "rect" + field.Name,
                        Width = RectangleSize,
                        Height = RectangleSize,
                        Fill = new SolidColorBrush(Colors.Green),
                        Stroke = new SolidColorBrush(Colors.Black),
                        StrokeThickness = 1,
                        Tag = field,
                    };
                    rect.DragEnter += this.Rectangle_DragEnter;
                    rect.DragLeave += this.Rectangle_DragLeave;
                    grdField.Children.Add(rect);

                    var circle = new Ellipse()
                    {
                        Name = "ell" + field.Name,
                        Width = CircleSize,
                        Height = CircleSize,
                        Tag = field,
                    };
                    circle.MouseMove += this.Ellipse_MouseMove;
                    circle.Drop += this.Ellipse_Drop;
                    grdField.Children.Add(circle);

                    Grid.SetRow(grdField, row);
                    Grid.SetColumn(grdField, col);
                    grdGame.Children.Add(grdField);
                }
            }
        }

        private void InitializeGrid()
        {
            var centerFields = GetCenter();
            SetCenter(centerFields);

            BuildQuadrants();
        }

        private List<Field> GetCenter()
        {
            Field topLeft = new Field(NumberOfRows / 2 - 1, NumberOfColumns / 2 - 1);
            Field topRight = new Field(NumberOfRows / 2 - 1, NumberOfColumns / 2);
            Field bottomLeft = new Field(NumberOfRows / 2, NumberOfColumns / 2 - 1);
            Field bottomRight = new Field(NumberOfRows / 2, NumberOfColumns / 2);

            topLeft.Side = Side.White;
            topRight.Side = Side.Black;
            bottomLeft.Side = Side.Black;
            bottomRight.Side = Side.White;

            var centerFields = new List<Field>() { topLeft, topRight, bottomLeft, bottomRight };
            return centerFields;
        }

        private void SetCenter(List<Field> centerFields)
        {
            // In the virtual grid
            foreach (var field in centerFields)
            {
                controller.UpdateVirtualGridField(field, field.Side);
            }

            // In the grid
            foreach (var child in grdGame.Children)
            {
                if (child is not Grid)
                {
                    continue;
                }

                var grdField = child as Grid;
                var centerField = centerFields.Where(f => f.GridColumn == Grid.GetColumn(grdField)
                        && f.GridRow == Grid.GetRow(grdField))
                        .FirstOrDefault();
                if (centerField == null)
                {
                    continue;
                }

                Ellipse circle = GetEllipse(grdField);
                SolidColorBrush color = ColorHelper.GetColorFromSide(centerField.Side);
                circle.Fill = color;
            }
        }

        private void BuildQuadrants()
        {
            if (NumberOfRows % 2 != 0 || NumberOfColumns % 2 != 0)
            {
                throw new ApplicationException("The number of rows and the number of columns must be an even number.");
            }

            quadrants = new List<Quadrant>()
            {
                new Quadrant(Direction.TopLeft,
                        0, NumberOfRows / 2 - 1, 0, NumberOfColumns / 2 - 1),
                new Quadrant(Direction.TopRight,
                        0, NumberOfRows / 2 - 1, NumberOfColumns / 2, NumberOfColumns - 1),
                new Quadrant(Direction.BottomLeft,
                        NumberOfRows / 2, NumberOfRows - 1, 0, NumberOfColumns / 2 - 1),
                new Quadrant(Direction.BottomRight,
                        NumberOfRows / 2, NumberOfRows - 1, NumberOfColumns / 2, NumberOfColumns - 1),
            };
        }

        private Ellipse GetEllipse(Grid grdField)
        {
            foreach (var ch in grdField.Children)
            {
                if (ch is Ellipse)
                {
                    var circle = ch as Ellipse;
                    return circle;
                }
            }

            return null;
        }

        private void Ellipse_MouseMove(object sender, MouseEventArgs e)
        {
            if (sender is not Ellipse || e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            SourceDisk = sender as Ellipse;
            DataObject data = new DataObject("Brush", SourceDisk.Fill);
            DragDrop.DoDragDrop(SourceDisk, data, DragDropEffects.Copy);
        }

        private void Rectangle_DragEnter(object sender, DragEventArgs e)
        {
            if (sender is not Rectangle)
            {
                return;
            }

            var rect = sender as Rectangle;

            rect.Stroke = new SolidColorBrush(Colors.Blue);
            rect.StrokeThickness = 3;
        }

        private void Rectangle_DragLeave(object sender, DragEventArgs e)
        {
            if (sender is not Rectangle)
            {
                return;
            }

            var rect = sender as Rectangle;
            rect.Stroke = new SolidColorBrush(Colors.Black);
            rect.StrokeThickness = 1;
        }

        private void Ellipse_Drop(object sender, DragEventArgs e)
        {
            if (sender is not Grid || e.Data is null)
            {
                return;
            }

            Grid grdField = sender as Grid;
            foreach (var child in grdField.Children)
            {
                if (child is Ellipse)
                {
                    if (!e.Data.GetDataPresent("Brush"))
                    {
                        return;
                    }

                    Ellipse targetDisk = child as Ellipse;
                    Brush currentColor = (Brush)e.Data.GetData("Brush");
                    Side currentSide = ColorHelper.GetSideFromColor((SolidColorBrush)currentColor);

                    if (controller.ValidateDropTarget(targetDisk, currentSide))
                    {
                        SetTargetDiskAppearance(targetDisk, currentColor);
                        controller.UpdateVirtualGridField(targetDisk.Tag as Field, currentSide);
                        ExecuteMove(currentSide);
                    }
                }

                if (child is Rectangle)
                {
                    Rectangle rect = child as Rectangle;
                    rect.Stroke = new SolidColorBrush(Colors.Black);
                    rect.StrokeThickness = 1;
                }
            }
        }

        private void SetTargetDiskAppearance(Ellipse targetDisk, Brush currentColor)
        {
            // First, give the previously highlighted disk its original appearance.
            if (previousTargetDisk != null)
            {
                HighlightDisk(previousTargetDisk, isHighlighted: false);
            }

            targetDisk.Fill = currentColor;
            previousTargetDisk = targetDisk;    // Save the current target disk to reset its appearance in the next move.
            HighlightDisk(targetDisk, isHighlighted: true);
        }

        /// <summary>
        /// A disk is highlighted when it's placed during the last move.
        /// </summary>
        /// <param name="isHighlighted">
        /// Toggle value. True to highlight, false to give a disk its normal appearance.
        /// </param>
        private void HighlightDisk(Ellipse disk, bool isHighlighted = true)
        {
            disk.Stroke = isHighlighted ? ColorHelper.GetOppositeColor(disk.Fill) : disk.Fill;
            disk.StrokeThickness = isHighlighted ? 2 : 1;
        }

        private void ExecuteMove(Side currentSide)
        {
            FlipDisks(controller.FieldsToFlip, currentSide);
            ShowNumberOfFlippedDisks(controller.FieldsToFlip);
            SwitchSide();
            ResetSkippedTurns(currentSide);
        }

        private void FlipDisks(List<Field> fields, Side sideToSwitchTo)
        {
            // Update virtual grid
            foreach (var field in fields)
            {
                controller.UpdateVirtualGridField(field, sideToSwitchTo);
            }

            // Set disk color in the grid
            foreach (var child in grdGame.Children)
            {
                if (child is not Grid)
                {
                    continue;
                }

                Grid grdField = child as Grid;
                Field fieldToChange = fields.Where(f => f.GridColumn == Grid.GetColumn(grdField)
                        && f.GridRow == Grid.GetRow(grdField))
                        .FirstOrDefault();

                if (fieldToChange == null)
                {
                    continue;
                }

                Ellipse circle = GetEllipse(grdField);
                SolidColorBrush color = ColorHelper.GetColorFromSide(sideToSwitchTo);
                circle.Fill = color;
                circle.Stroke = color;
            }
        }

        private void ShowNumberOfFlippedDisks(List<Field> fieldsToFlip)
        {
            if (!fieldsToFlip.Any())
            {
                return;
            }

            var flippedDisksLabels = GetFlippedDisksLabels();
            flippedDisksLabels.ForEach(l => ResetLabel(l));

            Quadrant quadrantHavingMostFields = GetQuadrantHavingMostFields(fieldsToFlip);
            Label displayLabel = GetLabelToUseForDisplay(flippedDisksLabels, quadrantHavingMostFields);
            displayLabel.Visibility = Visibility.Visible;
            displayLabel.Content = $"Flipped disks: {fieldsToFlip.Count}";
        }

        private List<Label> GetFlippedDisksLabels()
        {
            var flippedDisksLabels = new List<Label>();

            foreach (var stackPanelChild in pnlGrdGame.Children)
            {
                if (stackPanelChild is not DockPanel)
                {
                    continue;
                }

                foreach (var dockPanelChild in ((DockPanel)stackPanelChild).Children)
                {
                    if (dockPanelChild is Label)
                    {
                        Label label = dockPanelChild as Label;
                        flippedDisksLabels.Add(label);
                    }
                }
            }

            return flippedDisksLabels;
        }

        private void ResetLabel(Label label)
        {
            label.Visibility = Visibility.Hidden;
            label.Content = string.Empty;
        }

        private Quadrant GetQuadrantHavingMostFields(List<Field> fieldsToFlip)
        {
            Quadrant quadrantHavingMostFields = null;
            int fieldsMax = 0;
            quadrants.ForEach(q => q.NumberOfFields = 0);

            foreach (Field field in fieldsToFlip)
            {
                foreach (Quadrant quadrant in quadrants)
                {
                    if (!quadrant.ContainsField(field))
                    {
                        continue;
                    }

                    quadrant.NumberOfFields++;

                    if (quadrant.NumberOfFields > fieldsMax)
                    {
                        fieldsMax = quadrant.NumberOfFields;
                        quadrantHavingMostFields = quadrant;
                    }

                    break;
                }
            }

            return quadrantHavingMostFields;
        }

        private Label GetLabelToUseForDisplay(List<Label> flippedDisksLabels, Quadrant quadrant)
        {
            string searchString = "lblFlippedDisks" + quadrant.Direction.ToString();

            foreach (var label in flippedDisksLabels)
            {
                if (label.Name.Equals(searchString, StringComparison.InvariantCultureIgnoreCase))
                {
                    return label;
                }
            }

            return null;
        }

        private void SwitchSide()
        {
            Ellipse newDisk = GetEllipse(grdNewDisk);
            Side currentSide = ColorHelper.GetSideFromColor((SolidColorBrush)newDisk.Fill);
            Side newSide = currentSide == Side.Black ? Side.White : Side.Black;
            newDisk.Fill = ColorHelper.GetColorFromSide(newSide);
        }

        private void ResetSkippedTurns(Side currentSide)
        {
            controller.UpdateSkips(currentSide, skipped: false);
            ResetLblSkipTurn();
        }

        private void ResetLblSkipTurn()
        {
            if (lblSkipTurn.Content != null)
            {
                lblSkipTurn.Content = string.Empty;
            }
        }

        private void btnSkipTurn_Click(object sender, RoutedEventArgs e)
        {
            Side currentSide = ColorHelper.GetSideFromColor((SolidColorBrush)ellCurrentPlayer.Fill);
            bool isValidSkip = controller.ValidateSkipTurn(currentSide);

            if (isValidSkip)
            {
                controller.UpdateSkips(currentSide, skipped: true);

                bool gameIsFinished = controller.CheckSkips();
                if (gameIsFinished)
                {
                    EndGame();
                }
                else
                {
                    SwitchSide();
                }
            }
            else
            {
                // Notify user
                lblSkipTurn.Content = "You can't skip your turn as there's at least one possible move.";
            }
        }

        private void btnEndGame_Click(object sender, RoutedEventArgs e)
        {
            string message = "Are you sure?";
            string caption = "End game";
            var result = MessageBox.Show(message, caption, MessageBoxButton.YesNoCancel, MessageBoxImage.Information, MessageBoxResult.Yes);
            
            if (result == MessageBoxResult.Yes)
            {
                EndGame();
            }
        }

        private void EndGame()
        {
            ToggleButtons(enableGameButtons: false);
            
            scores = controller.GetScores();
            ShowFinalScores();
        }

        private void ToggleButtons(bool enableGameButtons)
        {
            grdNewDisk.IsEnabled = enableGameButtons;
            btnSkipTurn.IsEnabled = enableGameButtons;
            btnEndGame.IsEnabled = enableGameButtons;
            btnNewGame.IsEnabled = !enableGameButtons;
        }

        private void ShowFinalScores()
        {
            string winner = scores[Side.Black] > scores[Side.White]
                    ? Side.Black.ToString()
                    : Side.White.ToString();
            string gameStatus = "Game over! Winner: " + winner;
            lblGameStatus.Visibility = Visibility.Visible;
            lblGameStatus.Content = gameStatus;

            string scoreBlack = $"Score of player {Side.Black}: {scores[Side.Black]} disks";
            lblScoreBlack.Visibility = Visibility.Visible;
            lblScoreBlack.Content = scoreBlack;

            string scoreWhite = $"Score of player {Side.White}: {scores[Side.White]} disks";
            lblScoreWhite.Visibility = Visibility.Visible;
            lblScoreWhite.Content = scoreWhite;
        }

        private void btnNewGame_Click(object sender, RoutedEventArgs e)
        {
            ShowMainMenu();
        }

        private void ShowMainMenu()
        {
            var mainMenu = new MainMenu();
            mainMenu.Show();
            this.Close();
        }
    }
}
