using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace TicTacToeAI {
    class Program {
        static public GATrainer GATrainer;
        static public bool AILoadedFromFile = false;
        static public Random r = new Random();
        

        static void Main(string[] args) {
            while (1 == 1) {
                Console.WriteLine("Ingrese opción: ");
                Console.WriteLine("1) Jugar contra Random");
                Console.WriteLine("2) Jugar contra IA");
                Console.WriteLine("3) Entrenar red neuronal");
                Console.WriteLine("4) Competir entre AIs");
                Console.WriteLine("5) Competir IA vs Random");
                Console.WriteLine("6) Jugar 4 en linea");
                Console.WriteLine("?) Salir");

                int option = Int32.Parse(Console.ReadLine());

                switch (option) {
                    case 1:
                        PlayWithRandom();
                        break;
                    case 2:
                        PlayWithAI();
                        break;
                    case 3:
                        TrainAI();
                        //ParallelTrainAI();
                        break;
                    case 4:
                        AIBrawl();
                        break;
                    case 5:
                        AIPlayWithRandom();
                        break;
                    case 6:
                        PlayConnectFour();
                        break;
                    default:
                        Environment.Exit(0);
                        break;
                }
            }
        }

        static public void PlayWithRandom() {
            TicTacToe ttt = new TicTacToe();
            ttt.PrintBoard();

            while (true) {
                int i, j;
                do {
                    Console.Write("Ingrese fila: ");
                    i = int.Parse(Console.ReadLine());
                    Console.Write("Ingrese columna: ");
                    j = int.Parse(Console.ReadLine());
                } while (!ttt.PlayOn(i, j));
                if (ttt.State() != TicTacToe.BoardState.PLAYING)
                    break;
                while (!ttt.PlayOn(r.Next(0, 3), r.Next(0, 3))) { };
                if (ttt.State() != TicTacToe.BoardState.PLAYING)
                    break;
            }
            if (ttt.State() == TicTacToe.BoardState.WIN_CIRCLE)
                Console.WriteLine("Gano O");
            else if (ttt.State() == TicTacToe.BoardState.WIN_CROSS)
                Console.WriteLine("Gano X");
            else
                Console.WriteLine("Empate");
        }

        static public void PlayWithAI() {
            TicTacToe ttt = new TicTacToe();
            ttt.PrintBoard();

            Console.WriteLine("Binary file for AI1 (Empty for random): ");
            string fileIA1 = Console.ReadLine();

            Cerebellum AI = null;
            if (fileIA1 == string.Empty) {
                Console.WriteLine("Generated random AI agent");
                AI = new Cerebellum(10, 9, 1, false);
                AI.Randomize();
            }
            else {
                AI = LoadAI(fileIA1);
            }
            
            while (true) {
                int i, j;
                do {
                    Console.Write("Ingrese fila: ");
                    i = int.Parse(Console.ReadLine());
                    Console.Write("Ingrese columna: ");
                    j = int.Parse(Console.ReadLine());
                } while (!ttt.PlayOn(i, j));
                if (ttt.State() != TicTacToe.BoardState.PLAYING)
                    break;

                //INFO: Compute, try to play, if invalid then play random
                double[] results = AI.ComputeOutputs(ttt.GetBoardParameters());
                Cerebrum.PrintResult(results); //TODO: Remove
                int playIndex = Cerebrum.ResultIndex(results);
                Console.WriteLine("Index selected: "+playIndex);
                Console.WriteLine("Play values will be: {0}, {1}", playIndex / 3, playIndex % 3);
                if (!ttt.PlayOn(playIndex/3, playIndex%3)) {
                    Console.WriteLine("AI invalid move. Playing random");
                    while (!ttt.PlayOn(r.Next(0, 3), r.Next(0, 3))) { };
                }

                if(ttt.State() != TicTacToe.BoardState.PLAYING)
                    break;
            }

            if (ttt.State() == TicTacToe.BoardState.WIN_CIRCLE)
                Console.WriteLine("Gano O");
            else if (ttt.State() == TicTacToe.BoardState.WIN_CROSS)
                Console.WriteLine("Gano X");
            else
                Console.WriteLine("Empate");
        }

        static public void ConsolePrintWeights(double[] weights) {
            int i = 1;
            foreach (double d in weights) {
                if (d < 0) {
                    Console.Write("{0:N3} ", d);
                }
                else {
                    Console.Write(" {0:N3} ", d);
                }
                if (i++ % 10 == 0) {
                    Console.Write("\n");
                }
            }
            Console.Write("\n");
        }

        static public void TrainAI() {
            //TODO: Include all previous AI into initial generation
            if (GATrainer == null) {
                GATrainer = new GATrainer();
            }

            GATrainer.Train();
        }

        static public void ParallelTrainAI() {
            //INFO: Suitable for finding an initial solution
            int threads = 8;
            for (int i = 0; i < threads; i++) {
                new Thread(() =>
                {
                    Console.WriteLine("Started training of AI #" + i);
                    GATrainer trainer = new GATrainer();
                    trainer.Train();
                }).Start();
            }
        }

        static public Cerebellum LoadAI(string name = "") {
            try {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(@"D:\AI\"+name, FileMode.Open, FileAccess.Read);
                return (Cerebellum) formatter.Deserialize(stream);
            }
            catch (Exception e) {
                return null;
            }
        }

        static public void SaveAI(Cerebellum AI, string suffix = "") {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(@"D:\AI\TicTacToe" + suffix + ".ai", FileMode.Create, FileAccess.Write);
            formatter.Serialize(stream, AI);
            stream.Close();
        }
               
        static public void AIBrawl() {
            Random r = new Random();

            Console.WriteLine("Binary file for AI1: ");
            string fileIA1 = Console.ReadLine();
            Console.WriteLine("Binary file for AI2:");
            string fileIA2 = Console.ReadLine();
            Console.WriteLine("How many rounds?: ");
            int rounds = int.Parse(Console.ReadLine());

            Cerebellum AI1 = LoadAI(fileIA1);
            Cerebellum AI2 = LoadAI(fileIA2);
            int winsAI1 = 0;
            int winsAI2 = 0;
            int startingPlayer = 1;

            for (int i = 0; i < rounds; i++) {
                TicTacToe ttt = new TicTacToe();
                int currentPlayer = startingPlayer;

                while (true) {
                    //INFO: Compute, try to play, if invalid then play random
                    double[] results = (currentPlayer > 0 ? AI1 : AI2).ComputeOutputs(ttt.GetBoardParameters());
                    int playIndex = Cerebrum.ResultIndex(results);
                    if (!ttt.PlayOn(playIndex / 3, playIndex % 3, false)) {
                        //NOTE: If invalid move, the other player wins
                        if (currentPlayer > 0) {
                            winsAI2++;
                        }
                        else {
                            winsAI1++;
                        }
                        break;
                    }

                    if (ttt.State() != TicTacToe.BoardState.PLAYING) {
                        if (ttt.State() != TicTacToe.BoardState.TIE) {
                            if (currentPlayer == 1) {
                                winsAI1++;
                            }
                            else {
                                winsAI2++;
                            }
                        }
                        break;
                    }
                    //INFO: Changes player turn
                    currentPlayer *= -1;
                }
                //INFO: Changes starting player
                startingPlayer *= -1;
            }

            Console.WriteLine("Wins AI1 : "+winsAI1);
            Console.WriteLine("Wins AI2 : " + winsAI2);
        }

        static public void AIPlayWithRandom() {
            Console.Write("Ingrese el nombre del archivo: ");
            string name = Console.ReadLine();
            Cerebellum IA = LoadAI(name);
            Random r = new Random();
            TicTacToe ttt;
            bool playing;
            int games = 10000, turn;
            int IAWins = 0, randomWins = 0, ties = 0, IAInvalidMovements = 0;
            while (games > 0) {
                turn = games / 5001;
                playing = true;
                ttt = new TicTacToe();
                while (playing) {
                    if (turn == 0) {
                        while (!ttt.PlayOn(r.Next(0, 3), r.Next(0, 3), false)) { }
                    }
                    else {
                        if (!PlayMove(IA, ttt)) {
                            playing = false;
                            IAInvalidMovements++;
                        }
                    }
                    if (ttt.State() != TicTacToe.BoardState.PLAYING) {
                        if (ttt.State() != TicTacToe.BoardState.TIE) {
                            if (turn == 1)
                                IAWins++;
                            else
                                randomWins++;
                        }
                        else
                            ties++;
                        playing = false;
                    }
                    turn = (turn == 0 ? 1 : 0);
                }
                games--;
            }
            Console.WriteLine($"Victorias: {IAWins}");
            Console.WriteLine($"Derrottas: {randomWins}");
            Console.WriteLine($"Invalidos: {IAInvalidMovements}");
            Console.WriteLine($"Empates: {ties}");
            //Console.WriteLine($"En total se jugaron {IAWins + randomWins + IAInvalidMovements + ties} partidas");
        }

        static public bool PlayMove(Cerebellum AI, TicTacToe ttt) {
            double bestValue = double.MinValue;
            TicTacToe bestBoard = null;
            foreach (TicTacToe board in ttt.GetPossibleBoards()) {
                double boardValue = AI.ComputeOutputs(board.GetBoardParameters())[0];
                if (boardValue > bestValue) {
                    bestValue = boardValue;
                    bestBoard = board;
                }
            }
            return ttt.PlayOn(bestBoard.rowLastMovement, bestBoard.columnLastMovement, false);
            /*double[] results = AI.ComputeOutputs(ttt.GetBoardParameters());
            int playIndex = Cerebrum.ResultIndex(results);
            return ttt.PlayOn(playIndex / 3, playIndex % 3, false);*/
        }

        static public void PlayConnectFour()
        {
            ConnectFour connectFour = new ConnectFour();
            connectFour.PrintBoard();
            bool playing = true;
            int column = 0, turn;
            bool invalidColumn = true;
            while (playing)
            {
                turn = connectFour.turn;
                Console.WriteLine(turn == -1 ? "Le toca jugar al jugador 1 ('R')" : "Le toca jugar al jugador 2 ('Y')");
                do
                {
                    invalidColumn = true;
                    do
                    {
                        Console.Write("Ingrese la columna: ");
                        column = int.Parse(Console.ReadLine());
                        if (column >= 1 && column <= 7)
                            invalidColumn = false;
                    } while (invalidColumn);
                } while (!connectFour.PlayOn(column-1));
                connectFour.PrintBoard();
            }
        }
    }
}
