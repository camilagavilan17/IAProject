using System;
using System.Collections.Generic;
using System.Text;

namespace TicTacToeAI
{
    class ConnectFour
    {
        public int[,] board;
        public int turn;
        public int width = 7, height = 6;

        public ConnectFour()
        {
            board = new int[height, width];
            turn = -1; //-1 red 1 yellow
        }

        public void PrintBoard()
        {
            string boardRepresentation = "";
            Console.WriteLine(" 1   2   3   4   5   6   7   ");
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (board[i, j] == -1)
                        boardRepresentation += " R ";
                    else if (board[i, j] == 1)
                        boardRepresentation += " Y ";
                    else
                        boardRepresentation += "   ";
                    if (j != width - 1)
                        boardRepresentation += "|";
                }
                boardRepresentation += "\n";
                if (i != height - 1)
                    boardRepresentation += "---------------------------";
                Console.WriteLine(boardRepresentation);
                boardRepresentation = "";
            }
        }
    }
}
