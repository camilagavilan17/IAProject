﻿using System;
using System.Collections.Generic;
using System.Text;

namespace TicTacToeAI
{
    class ConnectFour
    {
        public enum BoardState { PLAYING, WIN_RED, WIN_YELLOW, TIE }
        public int[,] board;
        public int turn;
        public int width = 7, height = 6;

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
    }
}
