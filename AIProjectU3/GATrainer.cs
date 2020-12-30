using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace Connect4 {
    /// <summary>
    /// Represents the trainer for a given neural network.
    /// Uses generic algorithm schema
    /// </summary>
    class GATrainer {
        /// <summary>
        /// Changes how many subjects mutate on a population
        /// </summary>
        static public double SUBJECT_MUTATION_CHANCE = 0.5;
        /// <summary>
        /// Changes how much different are mutated subjects
        /// </summary>
        static public double GENOMA_MUTATION_CHANCE = 0.5;
        static public int INITIAL_POPULATION = 100;
        static public int TRAINING_ITERATIONS = int.MaxValue;
        static public int AI_BACKUP_GENERATIONS = 250;
        static public int THREADS_QUANTITY = 5;
        static public bool ELITIST_MODE = true;
        //NOTE: This quantity to the square 
        //must be the same as the initial population
        static public int TOP_QUANTITY_KEEP = 10;

        public int Generation = 1;
        public List<GASubject> Population;
        public GASubject TopSubject = null;
        public int MaxFitness = Int32.MinValue;
        public string PreviousAI;
        public bool CompeteWithAI = false;


        public GATrainer() { }

        /// <summary>
        /// Generates the initial population when starting the training
        /// </summary>
        public void Populate() {
            Population = new List<GASubject>();

            for (int i = 0; i < GATrainer.INITIAL_POPULATION; i++) {
                Population.Add(GASubject.Random());
            }
        }

        /// <summary>
        /// Triggers the traning of the population. Executes these procedures:
        /// - Population
        /// - Load of previous best IA (optional)
        /// - Evaluation
        /// - Ranking
        /// - Reproduction
        /// </summary>
        public void Train() {
            if (Population == null) {
                this.Populate();
                if (!String.IsNullOrEmpty(PreviousAI))
                    TryToLoadPreviousAI();
                TopSubject = Population[0];
            }
            for (int i = 0; i < TRAINING_ITERATIONS; i++) {
                if (!CompeteWithAI)
                    Evaluate();
                else
                    Evaluate2();
                Rank();
                Store();
                Reproduce();
            }
        }

        /// <summary>
        /// Tries to load an IA given by the user
        /// </summary>
        public void TryToLoadPreviousAI() {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(Program.AISavePath + PreviousAI, FileMode.Open, FileAccess.Read);
            NeuralNetwork cerebellum = (NeuralNetwork)formatter.Deserialize(stream);
            Population[0].AI = cerebellum;
        }

        /// <summary>
        /// Evaluates all the subjects on the current population
        /// </summary>
        private void Evaluate() {
            //NOTE: Each AI plays twice with the other AIs (to avoid winning only by starting)
            int portion = Population.Count / THREADS_QUANTITY;
            int remaining = Population.Count % THREADS_QUANTITY;
            Thread[] threads = new Thread[THREADS_QUANTITY];

            for (int i = 0; i < THREADS_QUANTITY; i++) {
                int start = (i * portion);
                //Adds all remaining on last iteration
                if (i == THREADS_QUANTITY - 1)
                    portion += remaining;

                threads[i] = new Thread(() => EvaluateRange(Population.GetRange(start, portion)));
                threads[i].Start();
            }
            //Joins all the tasks
            for (int i = 0; i < THREADS_QUANTITY; i++) {
                threads[i].Join();
            }
            //Task.WaitAll(tasks);
        }

        /// <summary>
        /// Triggers evaluation for a range of subjects.
        /// Used to split the population between multiple threads
        /// </summary>
        /// <param name="subjects"></param>
        private void EvaluateRange(List<GASubject> subjects) {
            for (int i = 0; i < subjects.Count; i++) {
                Compete(subjects[i]);
            }
        }

        /// <summary>
        /// Evaluates the population using the 2nd method (Matches between two IAs)
        /// </summary>
        public void Evaluate2() {
            for (int i = 0; i < Population.Count; i++) {
                for (int j = i + 1; j < Population.Count; j++) {
                    Compete2(Population[i], Population[j]);
                }
            }
        }

        /// <summary>
        /// Makes two IAs compete a given quantity of matches
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="subject2"></param>
        public void Compete2(GASubject subject, GASubject subject2) {
            ConnectFour connect4;
            int games = 2, currentTurn = 0;
            bool playing;
            while (games > 0) {
                playing = true;
                connect4 = new ConnectFour();
                if (games == 1)
                    connect4.turn = 1;
                while (playing) {
                    if (connect4.turn == -1) {
                        PlayMove(subject, connect4); //IA One plays
                        currentTurn = -1;
                    }
                    else if (connect4.turn == 1) {
                        PlayMove(subject2, connect4); //IA Two plays
                        currentTurn = 1;
                    }
                    if (connect4.GetState() != ConnectFour.BoardState.PLAYING && connect4.GetState() != ConnectFour.BoardState.TIE) {
                        if (currentTurn == -1) {
                            subject.Fitness++;
                        }
                        else if (currentTurn == 1) {
                            subject2.Fitness++;
                        }
                        playing = false;
                    }
                    else if (connect4.GetState() == ConnectFour.BoardState.TIE)
                        playing = false;
                }
                games--;
            }
        }

        /// <summary>
        /// Rank (DESC) the current population by fitness value
        /// </summary>
        private void Rank() {
            Population.Sort((a, b) => b.Fitness.CompareTo(a.Fitness));
            /*Console.WriteLine("Top of generation: ");
            for (int i = 0; i < 10; i++) {
                Console.WriteLine("Name: {0} - Fitness: {1}", Population[i].Name, Population[i].Fitness);
            }*/
        }

        /// <summary>
        /// Saves the current best IA serialized to a file.
        /// </summary>
        private void Store() {
            if (Population[0].Fitness > MaxFitness) {
                TopSubject = Population[0];
                Program.SaveAI(TopSubject.AI, "_" + DateTime.Now.ToString("MMddHHmm")
                    + "_I" + Thread.CurrentThread.ManagedThreadId + "_G" + Generation
                    + "_N" + TopSubject.Name + "_F" + TopSubject.Fitness);
                MaxFitness = Population[0].Fitness;
            }
        }

        /// <summary>
        /// Produces a new generation of population from the top N subjects
        /// of the current population. If elite mode is enable, includes
        /// parent from previos generations. Also adds the best IA to each
        /// generation.
        /// </summary>
        public void Reproduce() {
            Generation++;
            Random r = new Random();
            List<GASubject> nextPopulation = new List<GASubject>();

            for (int i = 0; i < TOP_QUANTITY_KEEP; i++) {
                for (int j = i + 1; j < TOP_QUANTITY_KEEP; j++) {
                    nextPopulation.AddRange(Population[i].AI.Crossover(Population[j]));
                }
            }

            //NOTE: Moved this to introduce mutation on parents as well
            if (GATrainer.ELITIST_MODE) {
                //NOTE: Adds the 99 best of the previous generation  (resets fitness)
                for (int i = 0; i < TOP_QUANTITY_KEEP - 1; i++) {
                    GASubject elder = Population[i].Clone();
                    elder.Fitness = 0;
                    nextPopulation.Add(elder);
                }
            }

            double mutationChance = GATrainer.SUBJECT_MUTATION_CHANCE;
            foreach (GASubject st in nextPopulation) {
                if (r.NextDouble() <= mutationChance) {
                    st.AI.Mutate();
                }
            }

            //NOTE: Includes top subject always //Need to put -1 to include elders
            nextPopulation.Add(TopSubject.Clone());
            nextPopulation.Last().Fitness = 0;

            Population = nextPopulation;
        }

        /// <summary>
        /// Makes a given IA compete N times against the random agent.
        /// Fitness is increased when the IA wins the match
        /// </summary>
        /// <param name="subject"></param>
        public void Compete(GASubject subject) {
            Random r = new Random();
            ConnectFour connectFour;
            int games = 1000;
            bool playing;
            int gamesplayed = 0, AIGamesPlayed = 0, RandomGamesPlayed = 0;
            int ties = 0, winAI = 0, winRandom = 0;
            int currentTurn = 0;
            while (games > 0) {
                playing = true;
                connectFour = new ConnectFour();
                if (games > 500)
                    connectFour.turn = 1;
                if (connectFour.turn == -1)
                    RandomGamesPlayed++;
                else if (connectFour.turn == 1)
                    AIGamesPlayed++;
                while (playing) {
                    if (connectFour.turn == -1) {
                        while (!connectFour.PlayOn(r.Next(0, connectFour.width))) { } //Random plays
                        currentTurn = -1;
                    }
                    else if (connectFour.turn == 1) {
                        PlayMove(subject, connectFour); //IA plays
                        currentTurn = 1;
                    }
                    if (connectFour.GetState() != ConnectFour.BoardState.PLAYING && connectFour.GetState() != ConnectFour.BoardState.TIE) {
                        if (currentTurn == 1) {
                            subject.Fitness++;
                            winAI++;
                        }
                        else if (currentTurn == -1) {
                            winRandom++;
                        }
                        playing = false;
                        gamesplayed++;
                    }
                    else if (connectFour.GetState() == ConnectFour.BoardState.TIE) {
                        playing = false;
                        ties++;
                        gamesplayed++;
                    }

                }
                games--;
            }
        }

        /// <summary>
        /// Utility to manage the play of a movement by the IA
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="connectFour"></param>
        /// <returns></returns>
        public bool PlayMove(GASubject subject, ConnectFour connectFour) {
            double bestValue = double.MinValue;
            ConnectFour bestBoard = null;
            foreach (ConnectFour board in connectFour.GetPossibleBoards()) {
                double boardValue = subject.AI.ComputeOutputs(board.GetBoardParameters())[0];
                if (boardValue > bestValue) {
                    bestValue = boardValue;
                    bestBoard = board;
                }
            }
            return connectFour.PlayOn(bestBoard.lastMovement);
        }

    }
}
