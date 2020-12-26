using System;
using System.Collections.Generic;
using System.Text;

namespace Connect4 {
    class GASubject {
        static public Random r = new Random();
        static public int Namer = 1000;

        public int Name;
        public Cerebellum AI;
        public int Fitness { get; set; }

        public GASubject() {
            Name = Namer++;
            AI = new Cerebellum(43, 42, 1, false);
            Fitness = 0;
        }

        public void SetGenome(double[] genome) {
            AI.SetWeights(genome);
        }

        public double[] GetGenome() {
            return AI.GetWeights();
        }

        static public GASubject Random() {
            GASubject subject = new GASubject();
            subject.AI.Randomize();
            return subject;
        }

        public GASubject Clone() { 
            GASubject subject = new GASubject();
            subject.AI.SetWeights(this.AI.GetWeights());
            subject.Fitness = 0;
            return subject;
        }

        public void Mutate() {
            double genomeMutationRate = GATrainer.GENOMA_MUTATION_CHANCE;
            double[] genome = this.GetGenome();
            for (int i = 0; i < genome.Length; i++) {
                if (r.NextDouble() <= genomeMutationRate) {
                    int position = r.Next(0, genome.Length);
                    genome[position] = (r.NextDouble() * 2) - 1;
                }
            }
            this.SetGenome(genome);
        }

        public GASubject[] Crossover(GASubject partner) {
            GASubject[] childs = new GASubject[2];

            double[] genomeA = this.GetGenome();
            double[] genomeB = partner.GetGenome();
            int genomeLenght = genomeA.Length;

            double[] childA = new double[genomeLenght];
            double[] childB = new double[genomeLenght];

            int partitionA = r.Next(0, genomeLenght-1);
            int partitionB = r.Next(partitionA+1, genomeLenght);

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
