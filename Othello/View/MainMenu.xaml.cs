using System;
using System.Collections.Generic;
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
    /// Interaction logic for MainMenu.xaml
    /// </summary>
    public partial class MainMenu : Window
    {
        public MainMenu()
        {
            InitializeComponent();
        }

        public int NumberOfPlayers { get; set; } = 2;

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            // One player game isn't implemented yet.
            if (NumberOfPlayers == 1)
                return;
            
            var gameView = new GameView(NumberOfPlayers);
            gameView.Show();
            this.Close();
        }

        private void btnPlayers1_Checked(object sender, RoutedEventArgs e)
        {
            NumberOfPlayers = 1;
            btnPlayers2.IsChecked = false;
        }

        private void btnPlayers2_Checked(object sender, RoutedEventArgs e)
        {
            NumberOfPlayers = 2;
            btnPlayers1.IsChecked = false;
        }

        private void btnPlayers1_Unchecked(object sender, RoutedEventArgs e)
        {
            btnPlayers2.IsChecked = true;
        }

        private void btnPlayers2_Unchecked(object sender, RoutedEventArgs e)
        {
            btnPlayers1.IsChecked = true;
        }
    }
}
