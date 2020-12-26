using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace Connect4 {
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
                Console.WriteLine("4) Competir IA vs Random");
                Console.WriteLine("5) Jugar 4 en linea");
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
                        AIPlayWithRandom();
                        break;
                    case 5:
                        PlayConnectFour();
                        break;
                    default:
                        Environment.Exit(0);
                        break;
                }
            }
        }

        static public void PlayWithRandom() {
            ConnectFour connectFour = new ConnectFour();
            connectFour.PrintBoard();
            int firstPlayer = r.Next(0, 2);
            int column, turn = firstPlayer == 0 ? -1 : 1;
            bool invalidColumn;
            while (true)
            {
                Console.WriteLine(turn == -1 ? "Le toca jugar al jugador " : "Le toca jugar al random ");
                if(turn == -1)
                {
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
                    } while (!connectFour.PlayOn(column - 1));
                }
                else
                    while (!connectFour.PlayOn(r.Next(0, connectFour.width))) { }
                connectFour.PrintBoard();
                Console.WriteLine("Estado tablero: " + connectFour.GetState());
                if (connectFour.GetState() != ConnectFour.BoardState.PLAYING)
                {
                    if (connectFour.GetState() == ConnectFour.BoardState.WIN_RED)
                        Console.WriteLine("Red wins!!!!");
                    else if (connectFour.GetState() == ConnectFour.BoardState.WIN_YELLOW)
                        Console.WriteLine("Yellow wins!!!!");
                    else if (connectFour.GetState() == ConnectFour.BoardState.TIE)
                        Console.WriteLine("Empate! nadie gano :C");
                    Console.ReadLine();
                    Environment.Exit(0);
                }
                turn = connectFour.turn;
            }

        }

        static public void PlayWithAI() {
            ConnectFour connectFour = new ConnectFour();
            connectFour.PrintBoard();

            Console.WriteLine("Binary file for AI1 (Empty for random): ");
            string fileIA1 = Console.ReadLine();

            Cerebellum AI;
            if (fileIA1 == "No") {
                Console.WriteLine("Generated random AI agent");
                AI = new Cerebellum(43, 42, 1, false);
                AI.Randomize();
            }
            else {
                AI = LoadAI(fileIA1);
            }

            int firstPlayer = r.Next(0, 2);
            int column, turn = connectFour.turn;
            bool invalidColumn;
            while (true)
            {
                Console.WriteLine(turn == -1 ? "Le toca jugar al jugador" : "Le toca jugar a la IA");
                if (turn == -1)
                {
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
                    } while (!connectFour.PlayOn(column - 1));
                }
                else
                    PlayMove(AI, connectFour);
                connectFour.PrintBoard();
                Console.WriteLine("Estado tablero: " + connectFour.GetState());
                if (connectFour.GetState() != ConnectFour.BoardState.PLAYING)
                {
                    if (connectFour.GetState() == ConnectFour.BoardState.WIN_RED)
                        Console.WriteLine("Red wins!!!!");
                    else if (connectFour.GetState() == ConnectFour.BoardState.WIN_YELLOW)
                        Console.WriteLine("Yellow wins!!!!");
                    else if (connectFour.GetState() == ConnectFour.BoardState.TIE)
                        Console.WriteLine("Empate! nadie gano :C");
                    Console.ReadLine();
                    Environment.Exit(0);
                }
                turn = connectFour.turn;
            }

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
            Stream stream = new FileStream(@"D:\AI\ConnectFour" + suffix + ".ai", FileMode.Create, FileAccess.Write);
            formatter.Serialize(stream, AI);
            stream.Close();
        }
        
        static public void AIPlayWithRandom() {
            Console.Write("Ingrese el nombre del archivo: ");
            string name = Console.ReadLine();
            Cerebellum IA = LoadAI(name);
            Random r = new Random();
            ConnectFour connectFour;
            bool playing;
            int games = 10000, turn;
            int IAWins = 0, randomWins = 0, ties = 0, IAInvalidMovements = 0;
            while (games > 0) {
                turn = games / 5001;
                playing = true;
                connectFour = new ConnectFour();
                while (playing) {
                    if (turn == 0) {
                        while (!connectFour.PlayOn(r.Next(0, connectFour.width))) { }
                    }
                    else {
                        if (!PlayMove(IA, connectFour)) {
                            playing = false;
                            IAInvalidMovements++;
                        }
                    }
                    if (connectFour.GetState() != ConnectFour.BoardState.PLAYING) {
                        if (connectFour.GetState() != ConnectFour.BoardState.TIE) {
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

        static public bool PlayMove(Cerebellum AI, ConnectFour connectFour) {
            double bestValue = double.MinValue;
            ConnectFour bestBoard = null;
            foreach (ConnectFour board in connectFour.GetPossibleBoards()) {
                double boardValue = AI.ComputeOutputs(board.GetBoardParameters())[0];
                if (boardValue > bestValue) {
                    bestValue = boardValue;
                    bestBoard = board;
                }
            }
            return connectFour.PlayOn(bestBoard.lastMovement);
            /*double[] results = AI.ComputeOutputs(ttt.GetBoardParameters());
            int playIndex = Cerebrum.ResultIndex(results);
            return ttt.PlayOn(playIndex / 3, playIndex % 3, false);*/
        }

        static public void PlayConnectFour()
        {
            ConnectFour connectFour = new ConnectFour();
            connectFour.PrintBoard();

            bool playing = true;
            int column, turn;
            bool invalidColumn;
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
                Console.WriteLine("Estado tablero: "+connectFour.GetState());
                //connectFour.GetPossibleBoards();
                if (connectFour.GetState() != ConnectFour.BoardState.PLAYING)
                {
                    if (connectFour.GetState() == ConnectFour.BoardState.WIN_RED)
                        Console.WriteLine("Red wins!!!!");
                    else if (connectFour.GetState() == ConnectFour.BoardState.WIN_YELLOW)
                        Console.WriteLine("Yellow wins!!!!");
                    else if (connectFour.GetState() == ConnectFour.BoardState.TIE)
                        Console.WriteLine("Empate! nadie gano :C");
                    Console.ReadLine();
                    Environment.Exit(0);
                }
                /*
                Console.Write("Obtener tableros posibles: ");
                if (Console.ReadLine() == "si")
                {
                    List<ConnectFour> boards = new List<ConnectFour>();
                    boards = connectFour.GetPossibleBoards();
                    foreach (ConnectFour board in boards)
                    {
                        Console.WriteLine("Tablero: ");
                        board.PrintBoard();
                    }
                    Console.WriteLine(boards.Count);
                }*/      
            }
        }
    }
}
