using System;
using System.Collections.Generic;
using System.Text;

namespace Connect4
{
    class ConnectFour
    {
        public enum BoardState { PLAYING, WIN_RED, WIN_YELLOW, TIE }
        public int[,] board;
        public int turn;
        public int width = 7, height = 6;
        public int lastMovement;

        public ConnectFour()
        {
            board = new int[height, width];
            turn = -1; //-1 red 1 yellow
        }

        public bool PlayOn(int column)
        {
            if (ValidMove(column))
            {
                board[FindRow(column), column] = turn;
                turn *= -1;
                //Console.WriteLine("Jugada valida");
                lastMovement = column;
                return true;
            }
            else
            {
                //Console.WriteLine("Jugada invalida");
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

        public BoardState GetState()
        {
            int row, row2;
            int column;
            int turn;

            //Check vertical
            for (column = 0; column < width; column++)
            {
                for (row = 0; row < height; row++)
                {
                    turn = board[row, column];
                    if (row + 3 >= height)
                        break;
                    else if (board[row + 1, column] == turn && board[row + 2, column] == turn && board[row + 3, column] == turn)
                        if (turn == -1)
                            return BoardState.WIN_RED;
                        else if (turn == 1)
                            return BoardState.WIN_YELLOW;
                }
            }

            //Check horizontal
            for (row = 0; row < height; row++)
            {
                for (column = 0; column < width; column++)
                {
                    turn = board[row, column];
                    if (column + 3 >= width)
                        break;
                    else if (board[row, column + 1] == turn && board[row, column + 2] == turn && board[row, column + 3] == turn)
                        if (turn == -1)
                            return BoardState.WIN_RED;
                        else if (turn == 1)
                            return BoardState.WIN_YELLOW;
                }
            }

            //Check diagonal from left to right - half 1           
            for (row = 0; row < height; row++)
            {
                row2 = row;
                column = 0;
                while (row2 >= 0 && column < width)
                {
                    turn = board[row2, column];
                    if (row2 - 3 >= 0 && column + 3 < width)
                        if (board[row2 - 1, column + 1] == turn && board[row2 - 2, column + 2] == turn && board[row2 - 3, column + 3] == turn)
                            if (turn == -1)
                                return BoardState.WIN_RED;
                            else if (turn == 1)
                                return BoardState.WIN_YELLOW;
                    row2--;
                    column++;

                }
               
            }

            //Check diagonal from left to right - half 2           
            for (row = height; row >= 0; row--)
            {
                row2 = row;
                column = width - 1;
                while (row2 < height && column >= 0)
                {
                    turn = board[row2, column];
                    if (row2 + 3 < height && column - 3 >= 0)
                        if (board[row2 + 1, column - 1] == turn && board[row2 + 2, column - 2] == turn && board[row2 + 3, column - 3] == turn)
                            if (turn == -1)
                                return BoardState.WIN_RED;
                            else if (turn == 1)
                                return BoardState.WIN_YELLOW;
                    row2++;
                    column--;

                }

            }

            //Check diagonal from right to left - half 1  
            for (row = 0; row < height; row++)
            {
                row2 = row;
                column = width - 1;
                while (row2 >= 0 && column >= 0)
                {
                    turn = board[row2, column];
                    if (row2 - 3 >= 0 && column - 3 >= 0)
                        if (board[row2 - 1, column - 1] == turn && board[row2 - 2, column - 2] == turn && board[row2 - 3, column - 3] == turn)
                            if (turn == -1)
                                return BoardState.WIN_RED;
                            else if (turn == 1)
                                return BoardState.WIN_YELLOW;
                    row2--;
                    column--;

                }

            }


            //Check diagonal from right to left - half 2  
            for (row = height; row >= 0; row--)
            {
                row2 = row;
                column = 0;
                while (row2 < height && column < width)
                {
                    turn = board[row2, column];
                    if (row2 + 3 < height && column + 3 < width)
                        if (board[row2 + 1, column + 1] == turn && board[row2 + 2, column + 2] == turn && board[row2 + 3, column + 3] == turn)
                            if (turn == -1)
                                return BoardState.WIN_RED;
                            else if (turn == 1)
                                return BoardState.WIN_YELLOW;
                    row2++;
                    column++;
                }

            }

            //Check if board is full 
            if (BoardIsFull())
                return BoardState.TIE;
            else
                return BoardState.PLAYING;
        }

        public bool BoardIsFull()
        {
            for (int row = 0; row < height; row++)
            {
                for (int column = 0; column < width; column++)
                {
                    if (board[row, column] == 0)
                        return false;
                }
            }
            return true;
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

        /*
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
        }*/

        public List<ConnectFour> GetPossibleBoards() {
            List<ConnectFour> boards = new List<ConnectFour>();

            for (int j = 0; j < width; j++) {
                ConnectFour connectFourNew = this.CloneConnectFour();
                if(connectFourNew.PlayOn(j))
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

        public double[] GetBoardParameters()
        {
            //NOTE: Index 0-42 are positions in the board. Index 43 is current player
            return new double[43] { board[0,0], board[0,1], board[0,2], board[0,3], board[0,4], board[0,5], board[0,6],
                                    board[1,0], board[1,1], board[1,2], board[1,3], board[1,4], board[1,5], board[1,6],
                                    board[2,0], board[2,1], board[2,2], board[2,3], board[2,4], board[2,5], board[2,6],
                                    board[3,0], board[3,1], board[3,2], board[3,3], board[3,4], board[3,5], board[3,6],
                                    board[4,0], board[4,1], board[4,2], board[4,3], board[4,4], board[4,5], board[4,6],
                                    board[5,0], board[5,1], board[5,2], board[5,3], board[5,4], board[5,5], board[5,6],
                                    turn};
        }

    }
}
