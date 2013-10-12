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
using System.Windows.Threading;

namespace TicTacToeClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        internal enum ClientStatus
        {
            ClientStatus_Unknown,
            ClientStatus_IPlay,
            ClientStatus_HePlays,
            ClientStatus_Over,
        }

        private static const string serverIpAddress = "127.0.0.1"; // Please modify this IP address of needed
        private static const int serverTcpPortNumber = 12005; // Please modify this TCP port number of needed

        internal TcpSender TcpSender = new TcpSender();
        GameClient gameClient = null;

        public MainWindow()
        {
            InitializeComponent();
            var timer = new System.Timers.Timer(250); // Refreshes board every 1/4s
            timer.Elapsed += (s, ea) =>
            {
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    updateBoard();
                }));
            };
            timer.Enabled = true;
        }

        private void listGamesBtn_Click(object sender, RoutedEventArgs e)
        {
            ListGamesWindow lgw = new ListGamesWindow();
            try
            {
                var gamesList = TcpSender.sendCommand(serverIpAddress, serverTcpPortNumber, "LG"); // TODO BT: Hardcoded IP and PORT of server!
                lgw.parseGamesList(gamesList);
                if (lgw.ShowDialog() == true)
                {
                    if (lgw.createNewGame)
                    {
                        gameClient = new GameClient();
                        TcpSender.sendCommand(serverIpAddress, serverTcpPortNumber, "NG " + lgw.gameName + ";" + lgw.playerName);
                        gameClient.playerRole = GameClient.PlayerRole.PlayerRole_Master;
                        gameClient.gameName = lgw.gameName;
                        updateBoard();
                    }
                    else
                    {
                        gameClient = new GameClient();
                        gameClient.gameName = lgw.gameName;
                        gameClient.opponentName = lgw.opponentName;
                        gameClient.playerRole = GameClient.PlayerRole.PlayerRole_Slave; // Second player is always slave
                        updateBoard();
                        if (gameClient.state == GameClient.GameState.GameState_OnePlayerReady)
                            TcpSender.sendCommand(serverIpAddress, serverTcpPortNumber, "JO " + gameClient.gameName + ";" + lgw.playerName); // Player 2 joins
                    }
                    statusTextBlock.Text = "Playing remote game: " + gameClient.gameName;
                    updateBoard();
                }
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void handleButton( Button B)
        {
            int x = 0, y = 0;
            if (B == B1) { x = 0; y = 0; }
            if (B == B2) { x = 0; y = 1; }
            if (B == B3) { x = 0; y = 2; }
            if (B == B4) { x = 1; y = 0; }
            if (B == B5) { x = 1; y = 1; }
            if (B == B6) { x = 1; y = 2; }
            if (B == B7) { x = 2; y = 0; }
            if (B == B8) { x = 2; y = 1; }
            if (B == B9) { x = 2; y = 2; }

            if( gameClient.playerRole == GameClient.PlayerRole.PlayerRole_Master && ( gameClient.state == GameClient.GameState.GameState_PlayingP1 ||
                gameClient.state == GameClient.GameState.GameState_OnePlayerReady ) ) // Playing as master
            {
                TcpSender.sendCommand(serverIpAddress, serverTcpPortNumber, "MM " + x.ToString() + y.ToString() + "1 " + gameClient.gameName); // 1 is master
            }

            if (gameClient.playerRole == GameClient.PlayerRole.PlayerRole_Slave && gameClient.state == GameClient.GameState.GameState_PlayingP2) // Playing as slave
            {
                TcpSender.sendCommand(serverIpAddress, serverTcpPortNumber, "MM " + x.ToString() + y.ToString() + "2 " + gameClient.gameName); // 2 is slave
            }
            updateBoard();

        }

        private void updateBoard()
        {
            // Send request to server
            if (gameClient == null)
                return;

            var boardStatus = TcpSender.sendCommand(serverIpAddress, serverTcpPortNumber, "GB " + gameClient.gameName);
            gameClient.parseBoard(boardStatus);
            if (gameClient.state != GameClient.GameState.GameState_OnePlayerReady && gameClient.opponentName == null) // There is no name available during waiting
                gameClient.parseOpponentName(TcpSender.sendCommand(serverIpAddress, serverTcpPortNumber, "SN " + gameClient.gameName));
            opponentName.Text = "Opponent: " + gameClient.opponentName;
            if (gameClient.state == GameClient.GameState.GameState_OnePlayerReady)
                statusTextBlock.Text = "Waiting for opponent...";
            else if ( ( gameClient.state == GameClient.GameState.GameState_PlayingP1 && gameClient.playerRole == GameClient.PlayerRole.PlayerRole_Master ) ||
                ( gameClient.state == GameClient.GameState.GameState_PlayingP2 && gameClient.playerRole == GameClient.PlayerRole.PlayerRole_Slave ) )
                statusTextBlock.Text = "Your turn!";
            else if ((gameClient.state == GameClient.GameState.GameState_PlayingP2 && gameClient.playerRole == GameClient.PlayerRole.PlayerRole_Master) ||
                (gameClient.state == GameClient.GameState.GameState_PlayingP1 && gameClient.playerRole == GameClient.PlayerRole.PlayerRole_Slave))
                statusTextBlock.Text = "Opponent's turn...";

            // Enable all buttons
            B1.IsEnabled = B2.IsEnabled = B3.IsEnabled = B4.IsEnabled = B5.IsEnabled = B6.IsEnabled = B7.IsEnabled = B8.IsEnabled = B9.IsEnabled = true;
            B1.Content = B2.Content = B3.Content = B4.Content = B5.Content = B6.Content = B7.Content = B8.Content = B9.Content = "";

            GameClient.BlockState bs;
            bs = gameClient.getBoard(0, 0);
            if (bs != GameClient.BlockState.BlockState_Empty)
            {
                B1.IsEnabled = false;
                if (bs == GameClient.BlockState.BlockState_O)
                    B1.Content = "O";
                else
                    B1.Content = "X";
            }
            bs = gameClient.getBoard(0, 1);
            if (bs != GameClient.BlockState.BlockState_Empty)
            {
                B2.IsEnabled = false;
                if (bs == GameClient.BlockState.BlockState_O)
                    B2.Content = "O";
                else
                    B2.Content = "X";
            }
            bs = gameClient.getBoard(0, 2);
            if (bs != GameClient.BlockState.BlockState_Empty)
            {
                B3.IsEnabled = false;
                if (bs == GameClient.BlockState.BlockState_O)
                    B3.Content = "O";
                else
                    B3.Content = "X";
            }
            bs = gameClient.getBoard(1, 0);
            if (bs != GameClient.BlockState.BlockState_Empty)
            {
                B4.IsEnabled = false;
                if (bs == GameClient.BlockState.BlockState_O)
                    B4.Content = "O";
                else
                    B4.Content = "X";
            }
            bs = gameClient.getBoard(1,1);
            if (bs != GameClient.BlockState.BlockState_Empty)
            {
                B5.IsEnabled = false;
                if (bs == GameClient.BlockState.BlockState_O)
                    B5.Content = "O";
                else
                    B5.Content = "X";
            }
            bs = gameClient.getBoard(1,2);
            if (bs != GameClient.BlockState.BlockState_Empty)
            {
                B6.IsEnabled = false;
                if (bs == GameClient.BlockState.BlockState_O)
                    B6.Content = "O";
                else
                    B6.Content = "X";
            }
            bs = gameClient.getBoard(2,0);
            if (bs != GameClient.BlockState.BlockState_Empty)
            {
                B7.IsEnabled = false;
                if (bs == GameClient.BlockState.BlockState_O)
                    B7.Content = "O";
                else
                    B7.Content = "X";
            }
            bs = gameClient.getBoard(2,1);
            if (bs != GameClient.BlockState.BlockState_Empty)
            {
                B8.IsEnabled = false;
                if (bs == GameClient.BlockState.BlockState_O)
                    B8.Content = "O";
                else
                    B8.Content = "X";
            }
            bs = gameClient.getBoard(2, 2);
            if (bs != GameClient.BlockState.BlockState_Empty)
            {
                B9.IsEnabled = false;
                if (bs == GameClient.BlockState.BlockState_O)
                    B9.Content = "O";
                else
                    B9.Content = "X";
            }

            handleEndOfGame();
        }

        private void handleEndOfGame()
        {
            var gameResult = gameClient.getGameState();

            if (gameClient.getGameState() != GameClient.GameState.GameState_OnePlayerReady &&
                gameClient.getGameState() != GameClient.GameState.GameState_PlayingP1 &&
                gameClient.getGameState() != GameClient.GameState.GameState_PlayingP2)
            {
                gameClient = null; // Destroy the "local" game
                B1.IsEnabled = B2.IsEnabled = B3.IsEnabled = B4.IsEnabled = B5.IsEnabled = B6.IsEnabled = B7.IsEnabled = B8.IsEnabled = B9.IsEnabled = false;
                B1.Content = B2.Content = B3.Content = B4.Content = B5.Content = B6.Content = B7.Content = B8.Content = B9.Content = "";
            }

            if (gameResult == GameClient.GameState.GameState_OverP1)
                MessageBox.Show("Player 1 wins!");
            if (gameResult == GameClient.GameState.GameState_OverP2)
                MessageBox.Show("Player 2 wins!");
            if (gameResult == GameClient.GameState.GameState_OverDraw)
                MessageBox.Show("Draw!");
        }

        private void B1_Click(object sender, RoutedEventArgs e)
        {
            handleButton(B1);
        }

        private void B2_Click(object sender, RoutedEventArgs e)
        {
            handleButton(B2);
        }

        private void B3_Click(object sender, RoutedEventArgs e)
        {
            handleButton(B3);
        }

        private void B4_Click(object sender, RoutedEventArgs e)
        {
            handleButton(B4);
        }

        private void B5_Click(object sender, RoutedEventArgs e)
        {
            handleButton(B5);
        }

        private void B6_Click(object sender, RoutedEventArgs e)
        {
            handleButton(B6);
        }

        private void B7_Click(object sender, RoutedEventArgs e)
        {
            handleButton(B7);
        }

        private void B8_Click(object sender, RoutedEventArgs e)
        {
            handleButton(B8);
        }

        private void B9_Click(object sender, RoutedEventArgs e)
        {
            handleButton(B9);
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            updateBoard();
        }
    }
}
