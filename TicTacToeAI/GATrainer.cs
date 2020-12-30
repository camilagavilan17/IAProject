using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Connect4 {
    class GATrainer {
        static public double SUBJECT_MUTATION_CHANCE = 0.5;
        static public double GENOMA_MUTATION_CHANCE = 0.5;
        static public int INITIAL_POPULATION = 100;
        static public int TRAINING_ITERATIONS = int.MaxValue;
        static public int AI_BACKUP_GENERATIONS = 250;
        static public int THREADS_QUANTITY = 5;
        static public bool ELITIST_MODE = true;
        static public int TOP_QUANTITY_KEEP = 10;

        public int Generation = 1;
        public List<GASubject> Population;
        public GASubject TopSubject = null;
        public int MaxFitness = Int32.MinValue;
        public string PreviousAI;
        public bool CompeteWithAI = false;
        

        public GATrainer() { }

        public void Populate() {
            Population = new List<GASubject>();

            for (int i = 0; i < GATrainer.INITIAL_POPULATION; i++) {
                Population.Add(GASubject.Random());
            }
        }

        public void Train() {
            if (Population == null) {
                this.Populate();
                if(!String.IsNullOrEmpty(PreviousAI))
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

        public void TryToLoadPreviousAI()
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(@"D:\AI\" + PreviousAI, FileMode.Open, FileAccess.Read);
            Cerebellum cerebellum = (Cerebellum)formatter.Deserialize(stream);
            Population[0].AI = cerebellum;
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

        public void Evaluate2()
        {
            for (int i = 0; i < Population.Count; i++)
            {
                for (int j = i; j < Population.Count; j++)
                {
                    Compete2(Population[i], Population[j]);
                }
            }
        }

        public void Compete2(GASubject subject, GASubject subject2)
        {
            ConnectFour connect4;
            int games = 2, currentTurn = 0;
            bool playing;
            while (games > 0)
            {
                playing = true;
                connect4 = new ConnectFour();
                if (games == 1)
                    connect4.turn = 1;
                while (playing)
                {
                    if(connect4.turn == -1)
                    {
                        PlayMove(subject, connect4); //IA One plays
                        currentTurn = -1;
                    } 
                    else if (connect4.turn == 1)
                    {
                        PlayMove(subject2, connect4); //IA Two plays
                        currentTurn = 1;
                    }
                    if (connect4.GetState() != ConnectFour.BoardState.PLAYING && connect4.GetState() != ConnectFour.BoardState.TIE)
                    {
                        if (currentTurn == -1)
                            subject.Fitness++;
                        else if (currentTurn == 1)
                            subject2.Fitness++;
                        playing = false;
                    }
                    else if (connect4.GetState() == ConnectFour.BoardState.TIE)
                        playing = false;
                }
                games--;
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
            Random r = new Random();
            List<GASubject> nextPopulation = new List<GASubject>();

            for (int i = 0; i < TOP_QUANTITY_KEEP; i++) {
                for (int j = i+1; j < TOP_QUANTITY_KEEP; j++) {
                    nextPopulation.AddRange(Population[i].AI.Crossover(Population[j]));
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
                for (int i = 0; i < TOP_QUANTITY_KEEP-1; i++) {
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

        public void Compete(GASubject subject)
        {
            Random r = new Random();
            ConnectFour connectFour;
            int games = 1000;
            bool playing;
            int gamesplayed = 0, AIGamesPlayed = 0, RandomGamesPlayed = 0;
            int ties = 0, winAI = 0, winRandom = 0;
            int currentTurn = 0;
            while (games > 0)
            {
                playing = true;
                connectFour = new ConnectFour();
                if (games > 500)
                    connectFour.turn = 1;
                if (connectFour.turn == -1)
                    RandomGamesPlayed++;
                else if (connectFour.turn == 1)
                    AIGamesPlayed++;
                while (playing)
                {
                    if (connectFour.turn == -1)
                    {
                        while (!connectFour.PlayOn(r.Next(0, connectFour.width))) { } //Random plays
                        currentTurn = -1;
                    }
                    else if (connectFour.turn == 1)
                    {
                        PlayMove(subject, connectFour); //IA plays
                        currentTurn = 1;
                    }
                    if (connectFour.GetState() != ConnectFour.BoardState.PLAYING && connectFour.GetState() != ConnectFour.BoardState.TIE)
                    {
                        if (currentTurn == 1)
                        {
                            subject.Fitness++;
                            winAI++;
                        }
                        else if (currentTurn == -1)
                        {
                            winRandom++;
                        }
                        playing = false;
                        gamesplayed++;
                    }
                    else if (connectFour.GetState() == ConnectFour.BoardState.TIE)
                    {
                        playing = false;
                        ties++;
                        gamesplayed++;
                    }

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
