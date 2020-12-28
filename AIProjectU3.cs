using System;
using System.Collections.Generic;
using System.Text;

namespace TicTacToeAI {
    class TicTacToe {
        public enum BoardState { PLAYING, WIN_CIRCLE, WIN_CROSS, TIE }

        public int[,] board;
        public int turn;

        //NOTE: Stores last movement for using with AI possible boards
        public int rowLastMovement;
        public int columnLastMovement;

        public TicTacToe() {
            board = new int[3, 3];
            turn = -1; //-1 circle 1 cross
        }

        public bool PlayOn(int i, int j, bool print = true) {
            if (board[i, j] != 0) {
                return false;
            }
            board[i, j] = turn;
            turn *= -1;
            if(print)
                PrintBoard();
            rowLastMovement = i;
            columnLastMovement = j;
            return true;
        }

        public void PrintBoard() {
            string boardRepresentation = "";
            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 3; j++) {
                    if (board[i, j] == -1)
                        boardRepresentation += "O ";
                    else if (board[i, j] == 1)
                        boardRepresentation += "X ";
                    else
                        boardRepresentation += "_ ";
                }
                boardRepresentation = boardRepresentation.Substring(0, boardRepresentation.Length - 1);
                boardRepresentation += "\n";
            }
            Console.WriteLine(boardRepresentation);
        }

        public BoardState State() {
            int turn, i, j;
            //Revisar columnas
            for (i = 0; i < 3; i++) {
                turn = board[i, 0];
                if (turn == board[i, 1] && turn == board[i, 2]) {
                    if (turn == -1)
                        return BoardState.WIN_CIRCLE;
                    else if (turn == 1)
                        return BoardState.WIN_CROSS;
                }
            }
            //Revisar filas
            for (j = 0; j < 3; j++) {
                turn = board[0, j];
                if (turn == board[1, j] && turn == board[2, j]) {
                    if (turn == -1)
                        return BoardState.WIN_CIRCLE;
                    else if (turn == 1)
                        return BoardState.WIN_CROSS;
                }
            }
            //Revisar diagonal 1
            i = 0;
            j = 0;
            turn = board[i, j];
            if (turn == board[i + 1, j + 1] && turn == board[i + 2, j + 2]) {
                if (turn == -1)
                    return BoardState.WIN_CIRCLE;
                else if (turn == 1)
                    return BoardState.WIN_CROSS;
            }

            //Revisar diagonal 2
            i = 2;
            j = 0;
            turn = board[i, j];
            if (turn == board[i - 1, j + 1] && turn == board[i - 2, j + 2]) {
                if (turn == -1)
                    return BoardState.WIN_CIRCLE;
                else if (turn == 1)
                    return BoardState.WIN_CROSS;
            }

            if (BoardIsFull())
                return BoardState.TIE;
            else
                return BoardState.PLAYING;
        }

        public bool BoardIsFull() {
            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 3; j++) {
                    if (board[i, j] == 0)
                        return false;
                }
            }
            return true;
        }

        public List<TicTacToe> GetPossibleBoards() {
            List<TicTacToe> boards = new List<TicTacToe>();

            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 3; j++) {
                    if (board[i, j] == 0) {
                        TicTacToe ttt = this.CloneTicTacToe();
                        ttt.PlayOn(i, j, false);
                        boards.Add(ttt);
                    }
                }
            }

            Utilities.Shuffle(boards);
            return boards;
        }

        public TicTacToe CloneTicTacToe() {
            TicTacToe ttt = new TicTacToe();
            ttt.turn = this.turn;
            ttt.board = this.board.Clone() as int[,];
            return ttt;
        }

        public double[] GetBoardParameters() {
            //NOTE: Index 0-8 are positions in the board. Index 9 is current player
            return new double[10] { board[0,0], board[0,1], board[0,2],
                                    board[1,0], board[1,1], board[1,2],
                                    board[2,0], board[2,1], board[2,2],
                                    turn};
        }
    }
}