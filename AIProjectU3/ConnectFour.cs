using System;
using System.Collections.Generic;

namespace Connect4 {

    /// <summary>
    /// Represents the game connect four or four in a line.
    /// The goal state is to line up four tiles on a board made 
    /// up of six rows and seven columns.
    /// </summary>
    class ConnectFour {
        public enum BoardState { PLAYING, WIN_RED, WIN_YELLOW, TIE }
        public int[,] board;
        public int turn;
        public int width = 7, height = 6;
        /// <summary>
        /// This parameter is used to know what movement the AI wants to play by choosing a board.
        /// </summary>
        public int lastMovement;

        public ConnectFour() {
            board = new int[height, width];
            turn = -1; //-1 red 1 yellow
        }

        /// <summary>
        /// This is the method to play a tile in the board, choosing only the column.
        /// </summary>
        /// <param name="column">This is column chosen to play</param> 
        /// <returns>True if the play could be made and 
        /// False if the play could not be made</returns>
        public bool PlayOn(int column) {
            if (ValidMove(column)) {
                board[FindRow(column), column] = turn;
                turn *= -1;
                //Console.WriteLine("Jugada valida");
                lastMovement = column;
                return true;
            }
            else {
                //Console.WriteLine("Jugada invalida");
                return false;
            }
        }

        /// <summary>
        /// This is a method to find the available row to play taking like a reference the column to play
        /// </summary>
        /// <param name="column">This is the column where do you want to play</param>
        /// <returns> The row if there is any available</returns>
        public int FindRow(int column) {
            int row;
            for (row = height - 1; row >= 0; row--) {
                if (board[row, column] == 0)
                    return row;
            }
            return row;
        }

        /// <summary>
        /// This method is to check if it is possible to play in a column, 
        /// that is, if there are still empty rows.
        /// </summary>
        /// <param name="column">This is the column where do you want to play</param>
        /// <returns>True if the column is valid and False if is not</returns>
        public bool ValidMove(int column) {
            bool validMove = false;
            for (int row = height - 1; row >= 0; row--) {
                if (board[row, column] == 0)
                    validMove = true;
            }
            return validMove;
        }

        /// <summary>
        /// This method is used to determine what state the board is in, 
        /// the states can be "in play", "red won or yellow won" and "tie".
        /// </summary>
        /// <returns>The board current state</returns>
        public BoardState GetState() {
            int row, row2;
            int column;
            int turn;

            //Check vertical
            for (column = 0; column < width; column++) {
                for (row = 0; row < height; row++) {
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
            for (row = 0; row < height; row++) {
                for (column = 0; column < width; column++) {
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
            for (row = 0; row < height; row++) {
                row2 = row;
                column = 0;
                while (row2 >= 0 && column < width) {
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
            for (row = height; row >= 0; row--) {
                row2 = row;
                column = width - 1;
                while (row2 < height && column >= 0) {
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
            for (row = 0; row < height; row++) {
                row2 = row;
                column = width - 1;
                while (row2 >= 0 && column >= 0) {
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
            for (row = height; row >= 0; row--) {
                row2 = row;
                column = 0;
                while (row2 < height && column < width) {
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

        public bool BoardIsFull() {
            for (int row = 0; row < height; row++) {
                for (int column = 0; column < width; column++) {
                    if (board[row, column] == 0)
                        return false;
                }
            }
            return true;
        }

        public void PrintBoard() {
            //string boardRepresentation = "";
            Console.WriteLine(" 1   2   3   4   5   6   7   ");
            for (int row = 0; row < height; row++) {
                for (int column = 0; column < width; column++) {
                    if (board[row, column] == -1) {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write(" ■ ");
                        Console.ResetColor();
                    }
                    else if (board[row, column] == 1) {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write(" ■ ");
                        Console.ResetColor();
                    }
                    else
                        Console.Write("   ");
                    if (column != width - 1)
                        Console.Write("|");
                }
                Console.Write("\n");
                if (row != height - 1)
                    Console.WriteLine("---------------------------");
            }
        }

        /// <summary>
        /// This method is to get all the possible boards from the position of this current board.
        /// This method is use by the AI, the AI rates this boards and we choose the one with the 
        /// best score as the one chosen by the AI.
        /// </summary>
        /// <returns> All the possible boards</returns>
        public List<ConnectFour> GetPossibleBoards() {
            List<ConnectFour> boards = new List<ConnectFour>();

            for (int j = 0; j < width; j++) {
                ConnectFour connectFourNew = this.CloneConnectFour();
                if (connectFourNew.PlayOn(j))
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

        /// <summary>
        ///  This are the parameters that the AI receive of any board 
        ///  because the AI only processes inputs in a number representation.
        /// </summary>
        /// <returns>Any position of the board + the current player turn</returns>
        public double[] GetBoardParameters() {
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
