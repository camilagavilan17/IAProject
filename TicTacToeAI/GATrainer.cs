using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Connect4 {
    class GATrainer {
        static public double SUBJECT_MUTATION_CHANCE = 0.1;
        static public double GENOMA_MUTATION_CHANCE = 0.05;
        static public int INITIAL_POPULATION = 100;
        static public int TRAINING_ITERATIONS = int.MaxValue;
        static public int AI_BACKUP_GENERATIONS = 250;
        static public int THREADS_QUANTITY = 4;
        static public bool ELITIST_MODE = true;
        static public int TOP_QUANTITY_KEEP = 10;

        public int Generation = 1;
        public List<GASubject> Population;
        public GASubject TopSubject = null;
        public int MaxFitness = Int32.MinValue;
        

        public GATrainer() { }

        public void Populate() {
            Console.WriteLine("Populating generation #"+Generation);
            Population = new List<GASubject>();

            for (int i = 0; i < GATrainer.INITIAL_POPULATION; i++) {
                Population.Add(GASubject.Random());
            }
        }

        public void Train() {
            if (Population == null) {
                this.Populate();
                TopSubject = Population[0];
            }
            for (int i = 0; i < TRAINING_ITERATIONS; i++) {
                Evaluate();
                Rank();
                Store();
                Reproduce();
            }
        }

        private void Evaluate()
        {
            //NOTE: Each AI plays twice with the other AIs (to avoid winning only by starting)
            int portion = Population.Count / THREADS_QUANTITY;
            int remaining = Population.Count % THREADS_QUANTITY;
            Thread[] threads = new Thread[THREADS_QUANTITY];

            for (int i = 0; i < THREADS_QUANTITY; i++)
            {
                int start = (i * portion);
                //Adds all remaining on last iteration
                if (i == THREADS_QUANTITY - 1)
                    portion += remaining;

                threads[i] = new Thread(() => EvaluateRange(Population.GetRange(start,portion)));
                threads[i].Start();
            }

            //Joins all the tasks
            for (int i = 0; i < THREADS_QUANTITY; i++) {
                threads[i].Join();
            }
            //Task.WaitAll(tasks);
        }


        private void EvaluateRange(List<GASubject> subjects)
        {
            for (int i = 0; i < subjects.Count; i++)
            {
                Compete(subjects[i]);
            }
        }

        private void Rank() {
            Population.Sort((a, b) => b.Fitness.CompareTo(a.Fitness));
            /*Console.WriteLine("Top of generation: ");
            for (int i = 0; i < 10; i++) {
                Console.WriteLine("Name: {0} - Fitness: {1}", Population[i].Name, Population[i].Fitness);
            }*/
        }

        private void Store() {
            if(Population[0].Fitness > MaxFitness)
            {
                TopSubject = Population[0];
                Program.SaveAI(TopSubject.AI, "_" + DateTime.Now.ToString("MMddHHmm")
                    + "_I" + Thread.CurrentThread.ManagedThreadId + "_G" + Generation 
                    + "_N" + TopSubject.Name + "_F" + TopSubject.Fitness);
                MaxFitness = Population[0].Fitness;
            }
        }

        public void Reproduce() {
            Generation++;
            Console.WriteLine("Reproducing generation #"+Generation);
            Random r = new Random();
            List<GASubject> nextPopulation = new List<GASubject>();

            for (int i = 0; i < TOP_QUANTITY_KEEP; i++) {
                for (int j = i+1; j < TOP_QUANTITY_KEEP; j++) {
                    nextPopulation.AddRange(Population[i].Crossover(Population[j]));
                }
            }

            //TOO: Setup parallel reproduction
            /*for (int i = 0; i < (INITIAL_POPULATION/2)-6; i++) {
                //NOTE: Only selects from the top ten
                int parent1 = r.Next(0, 10); 
                int parent2 = r.Next(0, 10);
                while (parent1 == parent2) {
                    parent2 = r.Next(0, 10);
                }
                nextPopulation.AddRange(Population[parent1].Crossover(Population[parent2]));
            }*/

            //NOTE: Moved this to introduce mutation on parents as well
            if (GATrainer.ELITIST_MODE) {
                //NOTE: Adds the 99 best of the previous generation  (resets fitness)
                for (int i = 0; i < TOP_QUANTITY_KEEP; i++) {
                    GASubject elder = Population[i].Clone();
                    elder.Fitness = 0;
                    nextPopulation.Add(elder);
                }
            }

            double mutationChance = GATrainer.SUBJECT_MUTATION_CHANCE; 
            foreach (GASubject st in nextPopulation) {
                if (r.NextDouble() <= mutationChance) {
                    st.Mutate();
                }
            }
            
            //NOTE: Includes top subject always //Need to put -1 to include elders
            //nextPopulation.Add(TopSubject.Clone());
 
            Population = nextPopulation;
        }

        public void Compete(GASubject subject) {
            Random r = new Random();
            ConnectFour connectFour;

            int playedGames = 0;
            int turn;
            bool playing;
            int games = 1000;

            while (games > 0) {
                connectFour = new ConnectFour();
                turn = games / 501;
                playing = true;
                //Console.WriteLine("Game: "+ (10-games));
                //Console.WriteLine("Starts player: "+turn);

                while (playing) {
                    if (turn == 0) {
                        while (!connectFour.PlayOn(r.Next(0, connectFour.width))) { }
                    }
                    else {
                        if (!PlayMove(subject, connectFour)) {
                            //NOTE: Loses when doing an invalid movement
                            subject.Fitness--;
                            playedGames++;
                            break;
                        }
                    }
                    if (connectFour.GetState() != ConnectFour.BoardState.PLAYING) {
                        if (connectFour.GetState() != ConnectFour.BoardState.TIE) {
                            if (turn == 1) {
                                subject.Fitness++;
                            }
                        }
                        playedGames++;
                        break;
                    }
                    turn = (turn == 0 ? 1 : 0);
                }
                games--;
            }
        }

        public bool PlayMove(GASubject subject, ConnectFour connectFour) {
            //INFO: Compute, try to play, if invalid then play random
            /*double[] results = subject.AI.ComputeOutputs(ttt.GetBoardParameters());
            int playIndex = Cerebrum.ResultIndex(results);
            return ttt.PlayOn(playIndex / 3, playIndex % 3, false);*/
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
