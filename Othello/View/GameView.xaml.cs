using Othello.Model.Enums;
using Othello.Model.Objects;
using Othello.ViewModel;
using Othello.ViewModel.Dto;
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
        private readonly int numberOfPlayers;
        private const int NumberOfRows = 8;
        private const int NumberOfColumns = 8;

        private readonly GameParam param;
        private readonly GameController controller;

        private Side[,] virtualGrid;

        public GameView()
        {
            InitializeComponent();
        }

        public GameView(int numberOfPlayers)
            : this()
        {
            this.numberOfPlayers = numberOfPlayers;
            this.virtualGrid = new Side[NumberOfRows, NumberOfColumns];
            this.param = new GameParam(numberOfPlayers, NumberOfRows, NumberOfColumns);
            this.controller = new GameController(this.param);

            BuildGrid();
            InitializeGrid();
        }

        internal Side[,] VirtualGrid
        {
            get => virtualGrid;
            set
            {
                if (value == virtualGrid)
                {
                    return;
                }

                virtualGrid = value;
                controller.SetVirtualGrid(virtualGrid);
            }
        }

        internal Ellipse SourceDisk { get; set; }

        private void BuildGrid()
        {
            for (int col = 0; col < NumberOfColumns; col++)
            {
                var newCol = new ColumnDefinition()
                {
                    Width = new GridLength(50),
                };
                grdGame.ColumnDefinitions.Add(newCol);
            }

            for (int row = 0; row < NumberOfRows; row++)
            {
                var newRow = new RowDefinition()
                {
                    Height = new GridLength(50),
                };
                grdGame.RowDefinitions.Add(newRow);
            }

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
                        Width = 50,
                        Height = 50,
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
                        Width = 40,
                        Height = 40,
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

            controller.SetVirtualGrid(VirtualGrid);
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
            // Set the center in the virtual grid
            foreach (var field in centerFields)
            {
                VirtualGrid[field.GridRow, field.GridColumn] = field.Side;
            }

            // Set disk color in the grid
            foreach (var child in grdGame.Children)
            {
                if (!(child is Grid))
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
                var color = (Color)ColorConverter.ConvertFromString(centerField.Side.ToString());
                circle.Fill = new SolidColorBrush(color);
            }
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
            if (!(sender is Ellipse) || e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            SourceDisk = sender as Ellipse;
            DataObject data = new DataObject("Brush", SourceDisk.Fill);
            DragDrop.DoDragDrop(SourceDisk, data, DragDropEffects.Copy);
        }

        private void Rectangle_DragEnter(object sender, DragEventArgs e)
        {
            if (!(sender is Rectangle))
            {
                return;
            }

            var rect = sender as Rectangle;

            rect.Stroke = new SolidColorBrush(Colors.Blue);
            rect.StrokeThickness = 3;
        }

        private void Rectangle_DragLeave(object sender, DragEventArgs e)
        {
            if (!(sender is Rectangle))
            {
                return;
            }

            var rect = sender as Rectangle;
            rect.Stroke = new SolidColorBrush(Colors.Black);
            rect.StrokeThickness = 1;
        }

        private void Ellipse_Drop(object sender, DragEventArgs e)
        {
            if (!(sender is Grid) || e.Data is null)
            {
                return;
            }

            foreach (var child in (sender as Grid).Children)
            {
                if (child is Ellipse)
                {
                    if (!e.Data.GetDataPresent("Brush"))
                    {
                        return;
                    }

                    var targetDisk = child as Ellipse;
                    if (controller.ValidateDropTarget(targetDisk))
                    {
                        Brush color = (Brush)e.Data.GetData("Brush");
                        targetDisk.Fill = color;
                    }
                }

                if (child is Rectangle)
                {
                    var rect = child as Rectangle;
                    rect.Stroke = new SolidColorBrush(Colors.Black);
                    rect.StrokeThickness = 1;
                }
            }
        }
    }
}
