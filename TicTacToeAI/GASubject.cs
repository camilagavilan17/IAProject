using System;
using System.Collections.Generic;
using System.Text;

namespace Connect4 {
    class GASubject {
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

    }
}
