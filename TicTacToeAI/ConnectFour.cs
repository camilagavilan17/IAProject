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

        public bool PlayOn(int column)
        {
            if (ValidMove(column))
            {
                board[FindRow(column), column] = turn;
                turn *= -1;
                Console.WriteLine("Jugada valida");
                return true;
            }
            else
            {
                Console.WriteLine("Jugada invalida");
                return false;
            } 
        }

        public int FindRow(int column)
        {
            int row;
            for (row = height - 1; row >= 0; row--)
            {
                if (board[row, column] == 0)
                    return row;
            }
            return row;
        }

        public bool ValidMove(int column)
        {
            bool validMove = false;
            for (int row = height-1; row >= 0; row--)
            {
                if (board[row, column] == 0)
                    validMove = true;
            }
            return validMove;
        }

        public void PrintBoard()
        {
            string boardRepresentation = "";
            Console.WriteLine(" 1   2   3   4   5   6   7   ");
            for (int row = 0; row < height; row++)
            {
                for (int column = 0; column < width; column++)
                {
                    if (board[row, column] == -1)
                        boardRepresentation += " R ";
                    else if (board[row, column] == 1)
                        boardRepresentation += " Y ";
                    else
                        boardRepresentation += "   ";
                    if (column != width - 1)
                        boardRepresentation += "|";
                }
                boardRepresentation += "\n";
                if (row != height - 1)
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
                    if(i-1 >=0){
                        if(board[i-1, j] != 1 || board[i-1, j] != -1){
                            return i-1;
                        } 
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
