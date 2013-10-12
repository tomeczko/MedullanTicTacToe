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

namespace TicTacToeClient
{
    /// <summary>
    /// Interaction logic for ListGamesWindow.xaml
    /// </summary>
    public partial class ListGamesWindow : Window
    {
        // Internal class for remote games entries
        internal class RemoteGameDef
        {
            public string gameName;
            public string player1Name;
            public string player2Name;
            public string state; // Descriptive state of game (for debugging purposes)
        }

        internal string gameName = "";
        internal string playerName = "";
        internal string opponentName = "";
        internal List<RemoteGameDef> remoteGames = new List<RemoteGameDef>();
        internal bool createNewGame = false;
        public ListGamesWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var g in remoteGames)
            {
                string gn = g.gameName + " (" + g.state + ")";
                listBox1.Items.Add(gn);
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (listBox1.SelectedItems.Count != 1)
            {
                MessageBox.Show("Please select exactly one game!");
                return;
            }
            // Finding game with given name (joint with status)
            RemoteGameDef d = null;
            foreach( var g in remoteGames )
                if( g.gameName + " (" + g.state + ")" == (string)listBox1.SelectedItem )
                    d = g;

            if (d != null)
            {
                if (d.state != "GameState_OnePlayerReady")
                {
                    MessageBox.Show("Please select game, which has only one player present!");
                    return;
                }
                playerName = playerNameTextBox.Text;
                gameName = d.gameName;
                opponentName = d.player1Name; // This is for being "slave" only since when starting a game, we don't know opponent's name
                DialogResult = true;
            }
        }

        private void newGameBtn_Click(object sender, RoutedEventArgs e)
        {
            if (playerNameTextBox.Text.Length == 0)
            {
                MessageBox.Show("Please enter also your name!");
                return;
            }

            gameName = gameNameTextBox.Text;
            playerName = playerNameTextBox.Text;
            createNewGame = true;
            DialogResult = true;
        }

        internal void parseGamesList(string gamesList)
        {
            while (gameName != "EOL")
            {
                RemoteGameDef remoteGame = new RemoteGameDef();
                remoteGame.gameName = gamesList.Substring(0, gamesList.IndexOf(';'));
                gamesList = gamesList.Substring(gamesList.IndexOf(';')+1);
                remoteGame.player1Name = gamesList.Substring(0, gamesList.IndexOf(';'));
                gamesList = gamesList.Substring(gamesList.IndexOf(';')+1);
                remoteGame.player2Name = gamesList.Substring(0, gamesList.IndexOf(';'));
                gamesList = gamesList.Substring(gamesList.IndexOf(';')+1);
                remoteGame.state = gamesList.Substring(0, gamesList.IndexOf(';'));
                gamesList = gamesList.Substring(gamesList.IndexOf(';')+1);

                remoteGames.Add(remoteGame);
                if (gamesList == "EOL")
                    break;
            }

        }

        private void playerNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (playerNameTextBox.Text.Length > 0)
                button1.IsEnabled = true;
            else
                button1.IsEnabled = false;
        }

        private void gameNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (gameNameTextBox.Text.Length > 0)
                newGameBtn.IsEnabled = true;
            else
                newGameBtn.IsEnabled = false;
        }
    }
}
