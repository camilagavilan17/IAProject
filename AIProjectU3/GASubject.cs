namespace Connect4 {
    /// <summary>
    /// Represents a subject for the training algorithm
    /// </summary>
    class GASubject {
        static public int Namer = 1000;

        public int Name;
        public NeuralNetwork AI;
        public int Fitness { get; set; }

        /// <summary>
        /// Creates a GASubject, by default uses a random AI, and 0 fitness. Name is assigned incrementally.
        /// </summary>
        public GASubject() {
            Name = Namer++;
            AI = new NeuralNetwork(43, 42, 1, false);
            Fitness = 0;
        }

        /// <summary>
        /// Sets genome of the current AI (genome is in this case the weights of the IA)
        /// </summary>
        /// <param name="genome">Given genome</param>
        public void SetGenome(double[] genome) {
            AI.SetWeights(genome);
        }

        /// <summary>
        /// Gets genome of the current AI (genome is in this case the weights of the IA)
        /// </summary>
        /// <returns></returns>
        public double[] GetGenome() {
            return AI.GetWeights();
        }

        /// <summary>
        /// Creates a random GASubject
        /// </summary>
        /// <returns>Random GA subject</returns>
        static public GASubject Random() {
            GASubject subject = new GASubject();
            subject.AI.Randomize();
            return subject;
        }

        /// <summary>
        /// Clones the current instance of GA subject
        /// </summary>
        /// <returns>Clone of GASubject</returns>
        public GASubject Clone() {
            GASubject subject = new GASubject();
            subject.AI.SetWeights(this.AI.GetWeights());
            subject.Fitness = 0;
            return subject;
        }

    }
}
