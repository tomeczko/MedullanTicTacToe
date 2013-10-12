using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TicTacToe_Server
{
    internal class Game
    {
        internal enum GameState
        {
            GameState_Unknown,
            GameState_OnePlayerReady,
            GameState_PlayingP1, // Player 1 turn
            GameState_PlayingP2, // Player 2 turn
            GameState_OverP1, // Player 1 Won
            GameState_OverP2, // Player 2 Won
            GameState_OverDraw, // Draw
        };

        internal enum BlockState
        {
            BlockState_Empty,
            BlockState_O,
            BlockState_X,
        };

        internal BlockState[,] blocks = new BlockState[3, 3];
        bool masterMadeMove = false;

        public Game() // Empty constructor
        {
        }

        internal GameState checkWinning()
        {
            if ((blocks[0, 0] == blocks[0, 1] && blocks[0, 0] == blocks[0, 2]) &&
                  blocks[0, 0] == BlockState.BlockState_X) // Player 1 wins
                return GameState.GameState_OverP2;
            if ((blocks[0, 0] == blocks[0, 1] && blocks[0, 0] == blocks[0, 2]) &&
                 blocks[0, 0] == BlockState.BlockState_O) // Player 2 wins
                return GameState.GameState_OverP1;

            if ((blocks[1, 0] == blocks[1, 1] && blocks[1, 0] == blocks[1, 2]) &&
                 blocks[1, 0] == BlockState.BlockState_X) // Player 1 wins
                return GameState.GameState_OverP2;
            if ((blocks[1, 0] == blocks[1, 1] && blocks[1, 0] == blocks[1, 2]) &&
                 blocks[1, 0] == BlockState.BlockState_O) // Player 2 wins
                return GameState.GameState_OverP1;

            if ((blocks[2, 0] == blocks[2, 1] && blocks[2, 0] == blocks[2, 2]) &&
                 blocks[2, 0] == BlockState.BlockState_X) // Player 1 wins
                return GameState.GameState_OverP2;
            if ((blocks[2, 0] == blocks[2, 1] && blocks[2, 0] == blocks[2, 2]) &&
                 blocks[2, 0] == BlockState.BlockState_O) // Player 2 wins
                return GameState.GameState_OverP1;


            if ((blocks[0, 0] == blocks[1, 0] && blocks[0, 0] == blocks[2, 0]) &&
                 blocks[0, 0] == BlockState.BlockState_X) // Player 1 wins
                return GameState.GameState_OverP2;
            if ((blocks[0, 0] == blocks[1, 0] && blocks[0, 0] == blocks[2, 0]) &&
                 blocks[0, 0] == BlockState.BlockState_O) // Player 2 wins
                return GameState.GameState_OverP1;

            if ((blocks[0, 1] == blocks[1, 1] && blocks[0, 1] == blocks[2, 1]) &&
                 blocks[0, 1] == BlockState.BlockState_X) // Player 1 wins
                return GameState.GameState_OverP2;
            if ((blocks[0, 1] == blocks[1, 1] && blocks[0, 1] == blocks[2, 1]) &&
                 blocks[0, 1] == BlockState.BlockState_O) // Player 2 wins
                return GameState.GameState_OverP1;

            if ((blocks[0, 2] == blocks[1, 2] && blocks[0, 2] == blocks[2, 2]) &&
                 blocks[0, 2] == BlockState.BlockState_X) // Player 1 wins
                return GameState.GameState_OverP2;
            if ((blocks[0, 2] == blocks[1, 2] && blocks[0, 2] == blocks[2, 2]) &&
                 blocks[0, 2] == BlockState.BlockState_O) // Player 2 wins
                return GameState.GameState_OverP1;


            if ((blocks[0, 0] == blocks[1, 1] && blocks[0, 0] == blocks[2, 2]) &&
                 blocks[1, 1] == BlockState.BlockState_X) // Player 1 wins
                return GameState.GameState_OverP2;
            if ((blocks[0, 0] == blocks[1, 1] && blocks[0, 0] == blocks[2, 2]) &&
                 blocks[1, 1] == BlockState.BlockState_O) // Player 2 wins
                return GameState.GameState_OverP1;

            if ((blocks[2, 0] == blocks[1, 1] && blocks[2, 0] == blocks[0, 2]) &&
                 blocks[1, 1] == BlockState.BlockState_X) // Player 1 wins
                return GameState.GameState_OverP2;
            if ((blocks[2, 0] == blocks[1, 1] && blocks[2, 0] == blocks[0, 2]) &&
                 blocks[1, 1] == BlockState.BlockState_O) // Player 2 wins
                return GameState.GameState_OverP1;

            // Check if there isn't a draw...
            bool draw = true;
            for (int x = 0; x < 3; x++)
                for (int y = 0; y < 3; y++)
                    if (blocks[x, y] == BlockState.BlockState_Empty)
                        draw = false;

            if (draw)
                return GameState.GameState_OverDraw;

            return state;
        }

        public void fillFromString(string inputString)
        {
            for (int i = 0; i < 9; ++i)
            {
                if (inputString[i] == '0') blocks[i / 3, i % 3] = BlockState.BlockState_Empty;
                if (inputString[i] == '1') blocks[i / 3, i % 3] = BlockState.BlockState_O;
                if (inputString[i] == '2') blocks[i / 3, i % 3] = BlockState.BlockState_X;
            }
        }

        public GameState makeMove(int x, int y, int player)
        {
            // Let's check if we can make a move. Do NOT make a move if it is not possible
            if (player == 0 && state == GameState.GameState_PlayingP2)
                return state; 
            if (player == 1 && state == GameState.GameState_PlayingP1)
                return state;
            if (masterMadeMove && state == GameState.GameState_OnePlayerReady)
                return state;

            // Let's check if we can do a move
            if (blocks[x, y] == BlockState.BlockState_Empty)
            {
                if (player == 0) // Player 1
                    blocks[x, y] = BlockState.BlockState_O;
                else // Player 2
                    blocks[x, y] = BlockState.BlockState_X;
                GameState gs = checkWinning();

                // Avoid making multiple moves by master
                masterMadeMove = true;

                // Swap players
                if (gs == GameState.GameState_PlayingP1)
                    gs = GameState.GameState_PlayingP2;
                else if (gs == GameState.GameState_PlayingP2)
                    gs = GameState.GameState_PlayingP1;

                state = gs;
                return gs;
            }
            else
            {
                return GameState.GameState_Unknown; // Indicate an error
            }
        }

        public GameState getGameState()
        {
            return state;
        }

        public void setGameName(string name)
        {
            gameName = name;
        }

        public void addPlayer1(string name) // TODO BT: Could be renamed to "addPlayer1"
        {
            player1Name = name;
            state = GameState.GameState_OnePlayerReady;
        }

        public void addPlayer2(string name) // TODO BT: Could be renamed to "addPlayer2"
        {
            player2Name = name;
            state = GameState.GameState_PlayingP2; // After player 2 joins, he makes a move first
        }

        public string getGameName()
        {
            return gameName;
        }

        public string getPlayer1Name()
        {
            return player1Name;
        }

        public string getPlayer2Name()
        {
            return player2Name;
        }

        private GameState state = GameState.GameState_Unknown;
        private string gameName = "";
        private string player1Name = ""; // Some default names
        private string player2Name = "";
        internal bool isRemote = false;

        internal BlockState getBoard(int x, int y)
        {
            return blocks[x, y];
        }

        internal string getStatusString()
        {
            string statusString = "";

            // First nine chars are board state
            for (int i = 0; i < 9; ++i)
            {
                if (blocks[i / 3, i % 3] == BlockState.BlockState_Empty)
                    statusString += "0";
                if (blocks[i / 3, i % 3] == BlockState.BlockState_O)
                    statusString += "1";
                if (blocks[i / 3, i % 3] == BlockState.BlockState_X)
                    statusString += "2";
            }

            // Tenth char is status of game
            statusString += ((int)state).ToString();
            return statusString;
        }
    }
}
