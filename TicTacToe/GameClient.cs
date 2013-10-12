using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TicTacToeClient
{
    class GameClient
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

        internal enum PlayerRole
        {
            PlayerRole_Unknown,
            PlayerRole_Master,
            PlayerRole_Slave,
        };

        internal enum BlockState
        {
            BlockState_Empty,
            BlockState_O,
            BlockState_X,
        };

        public string gameName { set; get; }
        public string opponentName { set; get; }
        internal GameState state;
        internal PlayerRole playerRole = PlayerRole.PlayerRole_Unknown;
        internal BlockState[,] blocks = new BlockState[3, 3];

        internal GameState getGameState()
        {
            return state;
        }

        internal void parseBoard(string boardStatus)
        {
            if (boardStatus.Length < 10)
                throw new Exception("boardStatus string was too short!");

            for (int i = 0; i < 9; ++i)
            {
                if (boardStatus[i] == '0') blocks[i / 3, i % 3] = BlockState.BlockState_Empty;
                if (boardStatus[i] == '1') blocks[i / 3, i % 3] = BlockState.BlockState_O;
                if (boardStatus[i] == '2') blocks[i / 3, i % 3] = BlockState.BlockState_X;
            }

            state = (GameState)int.Parse(boardStatus[9].ToString());
        }

        internal BlockState getBoard(int x, int y)
        {
            return blocks[x, y];
        }

        internal string parseOpponentName(string p)
        {
            opponentName = p.Substring(0, p.IndexOf(';'));
            return opponentName;
        }
    }
}
