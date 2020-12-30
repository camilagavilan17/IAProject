using NeuronDotNet.Core.Backpropagation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Connect4 {

    [Serializable]
    class Cerebrum {
        private int NumInputs;
        private int HiddenPerceptronsA;
        private int HiddenPerceptronsB;
        private int NumOutputs;
        private bool UseSoftmax;

        double[] Weights;

        private double[] Inputs;

        private double[][] IntHidAWeights;
        private double[][] HidAHidBWeights;
        private double[][] HidBOutWeights;

        private double[] HidABiases;
        private double[] HidBBiases;
        private double[] OutBiases;

        private double[] HidAOutputs;
        private double[] HidBOutputs;
        private double[] Outputs;

        private static Random Rand;

        public Cerebrum(int numInput, int numHiddenA, int numHiddenB, int numOutput, bool softmax = true) {
            this.NumInputs = numInput;
            this.HiddenPerceptronsA = numHiddenA;
            this.HiddenPerceptronsB = numHiddenB;
            this.NumOutputs = numOutput;
            this.UseSoftmax = softmax;

            Rand = new Random(0);
            Inputs = new double[numInput];

            IntHidAWeights = MakeMatrix(numInput, numHiddenA);
            HidAHidBWeights = MakeMatrix(numInput, numHiddenA);
            HidBOutWeights = MakeMatrix(numHiddenA, numOutput);

            HidABiases = new double[numHiddenA];
            HidBBiases = new double[numHiddenA];
            OutBiases = new double[numOutput];

            HidAOutputs = new double[numHiddenA];
            HidBOutputs = new double[numHiddenA];
            Outputs = new double[numOutput];
        }

        private static double[][] MakeMatrix(int rows, int cols) {
            double[][] result = new double[rows][];
            for (int r = 0; r < result.Length; ++r)
                result[r] = new double[cols];
            return result;
        }

        public void Randomize() {
            int numWeights = (NumInputs * HiddenPerceptronsA) + HiddenPerceptronsA +  (HiddenPerceptronsA * HiddenPerceptronsB) + HiddenPerceptronsB + (HiddenPerceptronsB * NumOutputs) + NumOutputs;
            double[] weights = new double[numWeights];
            double lo = -0.01;
            double hi = 0.01;
            for (int i = 0; i < weights.Length; ++i)
                weights[i] = (hi - lo) * Rand.NextDouble() + lo;
            this.SetWeights(weights);
        }

        public double[] GetWeights() {
            return Weights;
        }

        public void SetWeights(double[] weights) {
            this.Weights = weights;
            int numWeights = (NumInputs * HiddenPerceptronsA) + HiddenPerceptronsA + (HiddenPerceptronsA * HiddenPerceptronsB) + HiddenPerceptronsB + (HiddenPerceptronsB * NumOutputs) + NumOutputs;
            if (weights.Length != numWeights)
                throw new Exception("Bad weights length");

            int k = 0;

            for (int i = 0; i < NumInputs; ++i)
                for (int j = 0; j < HiddenPerceptronsA; ++j)
                    IntHidAWeights[i][j] = weights[k++];
            for (int i = 0; i < HiddenPerceptronsA; ++i)
                HidABiases[i] = weights[k++];

            for (int i = 0; i < HiddenPerceptronsA; ++i)
                for (int j = 0; j < HiddenPerceptronsB; ++j)
                    HidAHidBWeights[i][j] = weights[k++];
            for (int i = 0; i < HiddenPerceptronsB; ++i)
                HidBBiases[i] = weights[k++];

            for (int i = 0; i < HiddenPerceptronsA; ++i)
                for (int j = 0; j < NumOutputs; ++j)
                    HidBOutWeights[i][j] = weights[k++];
            for (int i = 0; i < NumOutputs; ++i)
                OutBiases[i] = weights[k++];
        }

        public double[] ComputeOutputs(double[] xValues) {
            double[] hidASums = new double[HiddenPerceptronsA];
            double[] hidBSums = new double[HiddenPerceptronsB];
            double[] outSums = new double[NumOutputs];

            //Copy values to inputs
            for (int i = 0; i < xValues.Length; ++i)
                this.Inputs[i] = xValues[i];

            //Sum of (input>hiddenA) weights * inputs       //
            for (int j = 0; j < HiddenPerceptronsA; ++j)
                for (int i = 0; i < NumInputs; ++i)
                    hidASums[j] += this.Inputs[i] * this.IntHidAWeights[i][j];

            //Biases of hidden layer
            for (int i = 0; i < HiddenPerceptronsA; ++i)
                hidASums[i] += this.HidABiases[i];

            //Activation function
            for (int i = 0; i < HiddenPerceptronsA; ++i)
                this.HidAOutputs[i] = HyperTanFunction(hidASums[i]);


            //Sum of (hideenA>hiddenB) weights * inputs     //
            for (int j = 0; j < HiddenPerceptronsB; ++j)
                for (int i = 0; i < HiddenPerceptronsA; ++i)
                    hidBSums[j] += this.HidAOutputs[i] * this.HidAHidBWeights[i][j];

            //Biases of hidden layer
            for (int i = 0; i < HiddenPerceptronsB; ++i)
                hidBSums[i] += this.HidBBiases[i];

            //Activation function
            for (int i = 0; i < HiddenPerceptronsB; ++i)
                this.HidBOutputs[i] = HyperTanFunction(hidBSums[i]);


            //Sum of (hiddenB>output) weights * inputs      //
            for (int j = 0; j < NumOutputs; ++j)
                for (int i = 0; i < HiddenPerceptronsB; ++i)
                    outSums[j] += HidBOutputs[i] * HidBOutWeights[i][j];

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

        private static double HyperTanFunction(double x) {
            //NOTE: approximation is correct to 30 decimals
            if (x < -20.0) return -1.0;
            else if (x > 20.0) return 1.0;
            else return Math.Tanh(x);
        }

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

        static public void PrintResult(double[] result) {
            foreach (double d in result) {
                Console.Write("{0:N3} ", d);
            }
            Console.Write("\n");
        }

        static public int ResultIndex(double[] result) {
            int pivot = -1;
            double previous = -1;
            for (int i = 0; i < result.Length; i++) {
                if (result[i] > previous) {
                    pivot = i;
                    previous = result[i];
                }
            }
            return pivot;
        }
    }
}
