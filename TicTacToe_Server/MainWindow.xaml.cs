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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Sockets;
using System.Net;
using System.Windows.Threading;

namespace TicTacToe_Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Game> games = new List<Game>();
        private TcpHandler listener;
        private static const int portNumber = 12005; // Please modify this TCP port number of needed
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Starting the server
            listener = new TcpHandler( portNumber );
            listener.OnMessageReceived += new TcpHandler.OnMessageReceivedEventHandler(listener_OnMessageReceived);

            // Create some dummy games to join to
            var game = new Game();
            game.setGameName("Dummy joinable game");
            game.addPlayer1("Player 1");
            games.Add(game);

            game = new Game();
            game.setGameName("Dummy joinable game 2");
            game.addPlayer1("Player 1 for different game");
            games.Add(game);

            // In this game player 1 put his "O/X" somewhere...
            game = new Game();
            game.setGameName("Game in progress");
            game.addPlayer1("Crazy George");
            game.fillFromString("010000000");
            games.Add(game);

            game = new Game();
            game.setGameName("Game with two players");
            game.addPlayer1("Joe");
            game.addPlayer2("Nick");
            game.makeMove(0, 0, 0);
            game.makeMove(1, 1, 1);
            game.makeMove(0, 2, 0);
            games.Add(game);
        }

        void listener_OnMessageReceived(object sender, TcpHandler.MessageReceivedEventArgs e)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
            {
                // Messages have two first chars as command
                string command = e.message.Substring(0, 2);
                try
                {
                    switch (command)
                    {
                        case "LG":
                            logTextBox.Text += "List games requested.\n";
                            string tmpString = "";
                            foreach (var g in games)
                            {
                                tmpString += g.getGameName() + ";" + g.getPlayer1Name() + ";" + g.getPlayer2Name() + ";" + g.getGameState() + ";";
                            }
                            tmpString += "EOL"; // End of line
                            e.returnMsg = tmpString;
                            break;
                        case "NG":
                            {
                                string gameName = e.message.Substring(3);
                                gameName = gameName.Substring(0, gameName.IndexOf(';'));
                                string playerName = e.message.Substring(3);
                                playerName = playerName.Substring(playerName.IndexOf(';') + 1);
                                playerName = playerName.Substring(0, playerName.IndexOf('\0'));
                                logTextBox.Text += "New game to be created: Game " + gameName + ", player " + playerName + "\n";

                                var game = new Game();
                                game.setGameName(gameName);
                                game.addPlayer1(playerName);
                                games.Add(game);

                                e.returnMsg = ";EOL";
                            }
                            break;
                        case "MM":
                            {
                                string gameName = e.message.Substring(7);
                                string moveDefinition = e.message.Substring(3, 3);
                                gameName = gameName.Substring(0, gameName.IndexOf('\0'));
                                logTextBox.Text += "Make move ";
                                logTextBox.Text += moveDefinition;
                                logTextBox.Text += " for game \"";
                                logTextBox.Text += gameName;
                                logTextBox.Text += "\"\n";
                                Game game = null;
                                foreach (var gIndex in games)
                                    if (gIndex.getGameName() == gameName)
                                        game = gIndex;

                                if (game != null)
                                {
                                    var moveResult = game.makeMove(int.Parse(moveDefinition[0].ToString()),
                                                                   int.Parse(moveDefinition[1].ToString()),
                                                                   int.Parse(moveDefinition[2].ToString()) - 1); // Here we expect 0 or 1

                                    if (moveResult == Game.GameState.GameState_OverP1)
                                        logTextBox.Text += "Player 1 (" + game.getPlayer1Name() + ") wins!\n";
                                    if (moveResult == Game.GameState.GameState_OverP2)
                                        logTextBox.Text += "Player 2 (" + game.getPlayer2Name() + ") wins!\n";
                                    if (moveResult == Game.GameState.GameState_OverDraw)
                                        logTextBox.Text += "Draw on game (" + game.getGameName() + ") wins!\n";
                                }
                                e.returnMsg = ";EOL"; // No return data here
                            }
                            break;
                        case "GB": // Get board state
                            {
                                string gameName = e.message.Substring(3);
                                gameName = gameName.Substring(0, gameName.IndexOf('\0'));
                                logTextBox.Text += "Get board state command for game \"";
                                logTextBox.Text += gameName;
                                logTextBox.Text += "\"\n";
                                Game game = null;
                                foreach (var gIndex in games)
                                    if (gIndex.getGameName() == gameName)
                                        game = gIndex;

                                // Searching for game on server side
                                string statusString = "";
                                if (game != null)
                                    statusString = game.getStatusString();
                                e.returnMsg = statusString + ";EOL"; // Returning state
                            }
                            break;
                        case "SN": // Get slave's name
                            {
                                string gameName = e.message.Substring(3);
                                gameName = gameName.Substring(0, gameName.IndexOf('\0'));
                                logTextBox.Text += "Get player 2 name for game \"";
                                logTextBox.Text += gameName;
                                logTextBox.Text += "\"\n";
                                Game game = null;
                                foreach (var gIndex in games)
                                    if (gIndex.getGameName() == gameName)
                                        game = gIndex;

                                // Searching for game on server side
                                string playerName = "";
                                if (game != null)
                                    playerName = game.getPlayer2Name();
                                e.returnMsg = playerName + ";EOL"; // Returning state
                            }
                            break;
                        case "JO": // Player 2 joins
                            {
                                string gameName = e.message.Substring(3);
                                gameName = gameName.Substring(0, gameName.IndexOf(';'));
                                string playerName = e.message.Substring(3);
                                playerName = playerName.Substring(playerName.IndexOf(';') + 1);
                                playerName = playerName.Substring(0, playerName.IndexOf('\0'));
                                logTextBox.Text += "Player 2 (" + playerName + ") joins to game \"";
                                logTextBox.Text += gameName;
                                logTextBox.Text += "\"\n";
                                Game game = null;
                                foreach (var gIndex in games)
                                    if (gIndex.getGameName() == gameName)
                                        game = gIndex;

                                // Searching for game on server side
                                if (game != null)
                                    game.addPlayer2(playerName);

                                e.returnMsg = ";EOL"; // No return data here
                            }
                            break;
                        default:
                            logTextBox.Text += "Unrecognized command.\n";
                            break;

                    }
                }
                catch (Exception ex)
                {
                    logTextBox.Text += "There was a problem during message parsing! --> " + ex.Message + "\n";
                }
            }));
        }
    }
}

