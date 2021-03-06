﻿using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace Connect4 {
    class Program {
        static public GATrainer GATrainer;
        static public bool AILoadedFromFile = false;
        static public Random r = new Random();
        static public string AISavePath = @"D:\AI\";

        static void Main() {
            FirstMenu();
        }

        /// <summary>
        /// User menu that allows to change trainer options in runtime
        /// </summary>
        static public void SecondMenu() {
            while (1 == 1) {
                Console.WriteLine("Ingrese opción para modificar variables de mutación: ");
                Console.WriteLine("1) Cambiar que tantos individuos son distintos"); //SUBJECT_MUTATION
                Console.WriteLine("2) Cambiar que tan distintos son"); //GENOMA_MUTATION
                Console.WriteLine("3) Mostrar datos de la generacion actual");
                Console.WriteLine("4) Salir");

                int option = Int32.Parse(Console.ReadLine());

                switch (option) {
                    case 1:
                        ChangeSubjectMutation();
                        break;
                    case 2:
                        ChangeGenomaMutation();
                        break;
                    case 3:
                        PrintCurrentGeneration();
                        break;
                    default:
                        Environment.Exit(0);
                        break;
                }
            }
        }

        static public void ChangeCompeteMethod() {
            GATrainer.CompeteWithAI = true;
        }

        static public void PrintCurrentGeneration() {
            Console.WriteLine("Reproducing generation #" + GATrainer.Generation);
            Console.WriteLine("Genoma mutation : " + GATrainer.GENOMA_MUTATION_CHANCE);
            Console.WriteLine("Subject mutation : " + GATrainer.SUBJECT_MUTATION_CHANCE);
        }

        static public void ChangeSubjectMutation() {
            Console.WriteLine("Ingrese el valor: ");
            GATrainer.GENOMA_MUTATION_CHANCE = double.Parse(Console.ReadLine());
        }

        static public void ChangeGenomaMutation() {
            Console.WriteLine("Ingrese el valor: ");
            GATrainer.SUBJECT_MUTATION_CHANCE = double.Parse(Console.ReadLine());
        }

        /// <summary>
        /// First menu of the application. Allows the user to navigate and run
        /// the different features
        /// </summary>
        static public void FirstMenu() {
            while (1 == 1) {
                Console.WriteLine("Ingrese opción: ");
                Console.WriteLine("1) Jugar contra Random");
                Console.WriteLine("2) Jugar contra IA");
                Console.WriteLine("3) Entrenar red neuronal");
                Console.WriteLine("4) Competir IA vs Random");
                Console.WriteLine("5) Jugar 4 en linea");
                Console.WriteLine("6) Salir");

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

        /// <summary>
        /// Setups a match with the user and a random agent
        /// </summary>
        static public void PlayWithRandom() {
            ConnectFour connectFour = new ConnectFour();
            connectFour.PrintBoard();
            int firstPlayer = r.Next(0, 2);
            int column, turn = connectFour.turn;
            bool invalidColumn;
            while (true) {
                Console.WriteLine(turn == -1 ? "Le toca jugar al jugador " : "Le toca jugar al random ");
                if (turn == -1) {
                    do {
                        invalidColumn = true;
                        do {
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
                if (connectFour.GetState() != ConnectFour.BoardState.PLAYING) {
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

        /// <summary>
        /// Setups a match with the user and an AI agent.
        /// User must provide the name of the IA
        /// </summary>
        static public void PlayWithAI() {
            ConnectFour connectFour = new ConnectFour();
            connectFour.PrintBoard();

            Console.WriteLine("Binary file for AI1 (Empty for random): ");
            string fileIA1 = Console.ReadLine();

            NeuralNetwork AI;
            if (fileIA1 == "No") {
                Console.WriteLine("Generated random AI agent");
                AI = new NeuralNetwork(43, 42, 1, false);
                AI.Randomize();
            }
            else {
                AI = LoadAI(fileIA1);
            }

            int firstPlayer = r.Next(0, 2);
            int column, turn = connectFour.turn;
            bool invalidColumn;
            while (true) {
                Console.WriteLine(turn == -1 ? "Le toca jugar al jugador" : "Le toca jugar a la IA");
                if (turn == -1) {
                    do {
                        invalidColumn = true;
                        do {
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
                if (connectFour.GetState() != ConnectFour.BoardState.PLAYING) {
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

        /// <summary>
        /// Utility to print the weights of an IA
        /// </summary>
        /// <param name="weights"></param>
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

        /// <summary>
        /// Starts the training of an AI. Values must be configured in the GATrainer class
        /// </summary>
        static public void TrainAI() {
            //TODO: Include all previous AI into initial generation
            if (GATrainer == null) {
                GATrainer = new GATrainer();
            }
            Console.WriteLine("Ingrese el nombre del archivo para cargar AI: ");
            GATrainer.PreviousAI = Console.ReadLine();
            Console.WriteLine("Cambiar a competición entre AIs?: ");
            if (Console.ReadLine() == "si")
                ChangeCompeteMethod();
            Thread thread = new Thread(() => SecondMenu());
            thread.Start();
            GATrainer.Train();
            thread.Join();
        }

        /// <summary>
        /// Loads an AI from a serialized filed. Path must me configured to avoid errors.
        /// </summary>
        /// <param name="name">Name of the IA file</param>
        /// <returns></returns>
        static public NeuralNetwork LoadAI(string name = "") {
            try {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(AISavePath + name, FileMode.Open, FileAccess.Read);
                return (NeuralNetwork)formatter.Deserialize(stream);
            }
            catch (Exception e) {
                Console.WriteLine("Error cargando la IA especificada. Revise el nombre y la ruta");
                Console.WriteLine(e.Message);
                return null;
            }
        }

        /// <summary>
        /// Saves an IA to the configured path using the given suffix
        /// </summary>
        /// <param name="AI">IA object</param>
        /// <param name="suffix">Suffix for the save file</param>
        static public void SaveAI(NeuralNetwork AI, string suffix = "") {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(AISavePath + "ConnectFour" + suffix + ".ai", FileMode.Create, FileAccess.Write);
            formatter.Serialize(stream, AI);
            stream.Close();
        }

        /// <summary>
        /// Setups many matches between a given AI and the random agent
        /// </summary>
        static public void AIPlayWithRandom() {
            Console.Write("Ingrese el nombre del archivo: ");
            string name = Console.ReadLine();
            NeuralNetwork IA = LoadAI(name);
            Random r = new Random();
            ConnectFour connectFour;
            bool playing;
            int games = 1000, turn;
            int IAWins = 0, randomWins = 0, ties = 0, IAInvalidMovements = 0;
            while (games > 0) {
                turn = games / 501;
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
        }

        /// <summary>
        /// Utility to play a move using the IA for a given board
        /// </summary>
        /// <param name="AI"></param>
        /// <param name="connectFour"></param>
        /// <returns></returns>
        static public bool PlayMove(NeuralNetwork AI, ConnectFour connectFour) {
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
        }

        /// <summary>
        /// Setups a match between two users
        /// </summary>
        static public void PlayConnectFour() {
            Console.WriteLine("White on blue.");
            ConnectFour connectFour = new ConnectFour();
            connectFour.PrintBoard();

            bool playing = true;
            int column, turn;
            bool invalidColumn;
            while (playing) {
                turn = connectFour.turn;
                Console.WriteLine(turn == -1 ? "Le toca jugar al jugador 1 ('R')" : "Le toca jugar al jugador 2 ('Y')");
                do {
                    invalidColumn = true;
                    do {
                        Console.Write("Ingrese la columna: ");
                        column = int.Parse(Console.ReadLine());
                        if (column >= 1 && column <= 7)
                            invalidColumn = false;
                    } while (invalidColumn);
                } while (!connectFour.PlayOn(column - 1));
                connectFour.PrintBoard();
                Console.WriteLine("Estado tablero: " + connectFour.GetState());
                if (connectFour.GetState() != ConnectFour.BoardState.PLAYING) {
                    if (connectFour.GetState() == ConnectFour.BoardState.WIN_RED)
                        Console.WriteLine("Red wins!!!!");
                    else if (connectFour.GetState() == ConnectFour.BoardState.WIN_YELLOW)
                        Console.WriteLine("Yellow wins!!!!");
                    else if (connectFour.GetState() == ConnectFour.BoardState.TIE)
                        Console.WriteLine("Empate! nadie gano :C");
                    Console.ReadLine();
                    Environment.Exit(0);
                }
            }
        }
    }
}
