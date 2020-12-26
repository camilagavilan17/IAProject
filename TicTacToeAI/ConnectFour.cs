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
            board[5,5]=1;

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
        public int PossibleNextStepsIA(int j)
        {
            String pos ="POS: ";
            for(int i=0; i<height; i++){
                if(board[i, j] == 1 || board[i, j] == -1){
                    if(board[i-1, j] != 1 || board[i-1, j] != -1){
                        return i-1;
                    }
                }
            }
            return 5;
        }
        public List<ConnectFour> GetPossibleBoards() {
            List<ConnectFour> boards = new List<ConnectFour>();

            for (int j = 0; j < width; j++) {
                ConnectFour connectFourNew = this.CloneConnectFour();
                connectFourNew.PlayOn(PossibleNextStepsIA(j), j);
                connectFourNew.PrintBoard();
                boards.Add(connectFourNew);
            }
            return boards;
        }

        public ConnectFour CloneConnectFour() {
            ConnectFour conecta = new ConnectFour();
            conecta.turn = this.turn;
            conecta.board = this.board.Clone() as int[,];
            return conecta;
        }

        public void PlayOn(int i, int j){
            this.board[i,j] = -1;
        }

    }
}
