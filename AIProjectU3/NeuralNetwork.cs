using System;

namespace Connect4 {
    /// <summary>
    /// Represents a MLP neural network with one hidden layer
    /// </summary>
    [Serializable]
    class NeuralNetwork {
        private int NumInputs;
        private int HiddenPerceptrons;
        private int NumOutputs;
        private bool UseSoftmax;

        double[] Weights;

        private double[] Inputs;

        private double[][] IntHidWeights;
        private double[][] HidOutWeights;

        private double[] HidBiases;
        private double[] OutBiases;

        private double[] HidOutputs;
        private double[] Outputs;

        private static Random Rand;

        /// <summary>
        /// MLP neural network basic implementation.
        /// Reference of implementation: https://visualstudiomagazine.com/Articles/2014/06/01/Deep-Neural-Networks.aspx?Page=2
        /// </summary>
        /// <param name="numInput">Number of inputs</param>
        /// <param name="numHiddenA">Number of perceptrons on hidden layer</param>
        /// <param name="numOutput">Number of outputs</param>
        /// <param name="softmax">Should softmax be applied. Used on multiple output NNs</param>
        public NeuralNetwork(int numInput, int numHiddenA, int numOutput, bool softmax = true) {
            this.NumInputs = numInput;
            this.HiddenPerceptrons = numHiddenA;
            this.NumOutputs = numOutput;
            this.UseSoftmax = softmax;

            Rand = new Random(0);
            Inputs = new double[numInput];

            IntHidWeights = MakeMatrix(numInput, numHiddenA);
            HidOutWeights = MakeMatrix(numHiddenA, numOutput);

            HidBiases = new double[numHiddenA];
            OutBiases = new double[numOutput];

            HidOutputs = new double[numHiddenA];
            Outputs = new double[numOutput];
        }

        /// <summary>
        /// Setup all needed matrix for data storage
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="cols"></param>
        /// <returns></returns>
        private static double[][] MakeMatrix(int rows, int cols) {
            double[][] result = new double[rows][];
            for (int r = 0; r < result.Length; ++r)
                result[r] = new double[cols];
            return result;
        }

        /// <summary>
        /// Randomizes all weights for the current neural network
        /// </summary>
        public void Randomize() {
            int numWeights = (NumInputs * HiddenPerceptrons) + HiddenPerceptrons + (HiddenPerceptrons * NumOutputs) + NumOutputs;
            double[] weights = new double[numWeights];
            double lo = -0.5;
            double hi = 0.5;
            for (int i = 0; i < weights.Length; ++i)
                weights[i] = (hi - lo) * Rand.NextDouble() + lo;
            this.SetWeights(weights);
        }

        /// <summary>
        /// Returns the current weights
        /// </summary>
        /// <returns></returns>
        public double[] GetWeights() {
            return Weights;
        }

        /// <summary>
        /// Sets weights of the neural network from the given array.
        /// </summary>
        /// <param name="weights">Weights array</param>
        public void SetWeights(double[] weights) {
            this.Weights = weights;
            int numWeights = (NumInputs * HiddenPerceptrons) + HiddenPerceptrons + (HiddenPerceptrons * NumOutputs) + NumOutputs;
            if (weights.Length != numWeights)
                throw new Exception("Bad weights length");

            int k = 0;

            for (int i = 0; i < NumInputs; ++i)
                for (int j = 0; j < HiddenPerceptrons; ++j)
                    IntHidWeights[i][j] = weights[k++];

            for (int i = 0; i < HiddenPerceptrons; ++i)
                HidBiases[i] = weights[k++];

            for (int i = 0; i < HiddenPerceptrons; ++i)
                for (int j = 0; j < NumOutputs; ++j)
                    HidOutWeights[i][j] = weights[k++];

            for (int i = 0; i < NumOutputs; ++i)
                OutBiases[i] = weights[k++];
        }

        /// <summary>
        /// Calculates the output values for a given input data set
        /// </summary>
        /// <param name="xValues">Input values</param>
        /// <returns>Array with the output values</returns>
        public double[] ComputeOutputs(double[] xValues) {
            double[] hidSums = new double[HiddenPerceptrons];
            double[] outSums = new double[NumOutputs];

            //Copy values to inputs
            for (int i = 0; i < xValues.Length; ++i)
                this.Inputs[i] = xValues[i];

            //Sum of (input>hidden) weights * inputs
            for (int j = 0; j < HiddenPerceptrons; ++j)
                for (int i = 0; i < NumInputs; ++i)
                    hidSums[j] += this.Inputs[i] * this.IntHidWeights[i][j];

            //Biases of hidden layer
            for (int i = 0; i < HiddenPerceptrons; ++i)
                hidSums[i] += this.HidBiases[i];

            //Activation function
            for (int i = 0; i < HiddenPerceptrons; ++i)
                this.HidOutputs[i] = HyperTanFunction(hidSums[i]);

            //Sum of (hidden>output) weights * inputs
            for (int j = 0; j < NumOutputs; ++j)
                for (int i = 0; i < HiddenPerceptrons; ++i)
                    outSums[j] += HidOutputs[i] * HidOutWeights[i][j];

            //Biases of output layer
            for (int i = 0; i < NumOutputs; ++i)
                outSums[i] += OutBiases[i];

            double[] softOut = null;
            if (UseSoftmax) {
                //NOTE: Activation does all outputs at once
                softOut = Softmax(outSums);
            }
            else {
                softOut = outSums;
            }
            Array.Copy(softOut, Outputs, softOut.Length);

            //TODO: Define a GetOutputs method instead
            double[] retResult = new double[NumOutputs];
            Array.Copy(this.Outputs, retResult, retResult.Length);
            return retResult;
        }

        /// <summary>
        /// Activation function used for the layers
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private static double HyperTanFunction(double x) {
            //NOTE: approximation is correct to 30 decimals
            if (x < -20.0) return -1.0;
            else if (x > 20.0) return 1.0;
            else return Math.Tanh(x);
        }

        /// <summary>
        /// Softmax algorithm for output normalization
        /// </summary>
        /// <param name="oSums"></param>
        /// <returns></returns>
        private static double[] Softmax(double[] oSums) {
            //NOTE: determine max output sum
            //does all output nodes at once so scale 
            //doesn't have to be re-computed each time
            double max = oSums[0];
            for (int i = 0; i < oSums.Length; ++i)
                if (oSums[i] > max) max = oSums[i];

            //NOTE: determine scaling factor -- sum of exp(each val - max)
            double scale = 0.0;
            for (int i = 0; i < oSums.Length; ++i)
                scale += Math.Exp(oSums[i] - max);

            double[] result = new double[oSums.Length];
            for (int i = 0; i < oSums.Length; ++i)
                result[i] = Math.Exp(oSums[i] - max) / scale;

            //NOTE: now scaled so that xi sum to 1.0
            return result;
        }

        /// <summary>
        /// Prints the output of the network in a readaeable maner
        /// </summary>
        /// <param name="result"></param>
        static public void PrintResult(double[] result) {
            foreach (double d in result) {
                Console.Write("{0:N3} ", d);
            }
            Console.Write("\n");
        }

        /// <summary>
        /// Mutates the current NN instance with the GA trainer parameters
        /// </summary>
        public void Mutate() {
            //TODO: Pass values as parameters
            double genomeMutationRate = GATrainer.GENOMA_MUTATION_CHANCE;
            double[] genome = this.GetWeights();
            for (int i = 0; i < genome.Length; i++) {
                if (Rand.NextDouble() <= genomeMutationRate) {
                    int position = Rand.Next(0, genome.Length);
                    genome[position] = (Rand.NextDouble() * 2) - 1;
                }
            }
            this.SetWeights(genome);
        }

        /// <summary>
        /// Executes two-point crossover with the current and the given NN.
        /// </summary>
        /// <param name="partner">Parner NN for the crossover</param>
        /// <returns>Array containing two childs</returns>
        public GASubject[] Crossover(GASubject partner) {
            GASubject[] childs = new GASubject[2];

            double[] genomeA = this.GetWeights();
            double[] genomeB = partner.GetGenome();
            int genomeLenght = genomeA.Length;

            double[] childA = new double[genomeLenght];
            double[] childB = new double[genomeLenght];

            int partitionA = Rand.Next(0, genomeLenght - 1);
            int partitionB = Rand.Next(partitionA + 1, genomeLenght);

            //NOTE: Changed for two-point crossover
            for (int i = 0; i < partitionA; i++) {
                childA[i] = genomeA[i];
                childB[i] = genomeB[i];
            }
            for (int j = partitionA; j < partitionB; j++) {
                childA[j] = genomeB[j];
                childB[j] = genomeA[j];
            }
            for (int k = partitionB; k < genomeLenght; k++) {
                childA[k] = genomeA[k];
                childB[k] = genomeB[k];
            }

            GASubject traineeA = new GASubject();
            traineeA.SetGenome(childA);
            GASubject traineeB = new GASubject();
            traineeB.SetGenome(childB);
            childs[0] = traineeA;
            childs[1] = traineeB;

            return childs;
        }
    }
}
