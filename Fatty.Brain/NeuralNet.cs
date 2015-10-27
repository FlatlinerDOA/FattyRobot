using System;

namespace Fatty.Brain
{
    public sealed class NeuralNet : INeuralNet
    {
        private NeuralLayer hiddenLayer;
        private NeuralLayer inputLayer;
        private double learningRate = 0.5d;
        private NeuralLayer outputLayer;
        public INeuralLayer Hidden
        {
            get
            {
                return this.hiddenLayer;
            }
        }

        public INeuralLayer Input
        {
            get
            {
                return this.inputLayer;
            }
        }

        public INeuralLayer Output
        {
            get
            {
                return this.outputLayer;
            }
        }

        public void ApplyLearning()
        {
            this.outputLayer.ApplyLearning(this.hiddenLayer, this.learningRate);
            this.hiddenLayer.ApplyLearning(this.inputLayer, this.learningRate);
        }

        public void Initialize(int randomSeed, int inputNeuronCount, int hiddenNeuronCount, int outputNeuronCount)
        {
            // initializations
            var rand = new Random(randomSeed);
            this.inputLayer = new NeuralLayer();
            this.outputLayer = new NeuralLayer();
            this.hiddenLayer = new NeuralLayer();
            int i, j;
            for (i = 0; i < inputNeuronCount; i++)
            {
                this.inputLayer.Add(new Neuron(new NeuralFactor(rand.NextDouble())));
            }

            for (i = 0; i < outputNeuronCount; i++)
            {
                this.outputLayer.Add(new Neuron(new NeuralFactor(rand.NextDouble())));
            }

            for (i = 0; i < hiddenNeuronCount; i++)
            {
                this.hiddenLayer.Add(new Neuron(new NeuralFactor(rand.NextDouble())));
            }

            // Wire-up input layer to hidden layer
            this.hiddenLayer.Connect(this.inputLayer, rand);

            // Wire-up output layer to hidden layer
            this.outputLayer.Connect(this.hiddenLayer, rand);
        }

        public void Pulse()
        {
            this.hiddenLayer.Pulse();
            this.outputLayer.Pulse();
        }

        public void Train(double[] input, double[] desiredResult)
        {
            int i;
            if (input.Length != this.inputLayer.Count)
            {
                throw new ArgumentException(string.Format("Expecting {0} inputs for this net", this.inputLayer.Count));
            }

            // initialize data
            for (i = 0; i < this.inputLayer.Count; i++)
            {
                Neuron n = this.inputLayer[i] as Neuron;
                if (null != n) // maybe make interface get;set;
                {
                    n.Output = input[i];
                }
            }

            this.Pulse();
            this.BackPropagate(desiredResult);
        }

        public void Train(double[][] inputs, double[][] expected)
        {
            for (int i = 0; i < inputs.Length; i++)
            {
                this.Train(inputs[i], expected[i]);
            }
        }

        private void BackPropagate(double[] desiredResults)
        {
            int i, j;
            double temp, error;
            INeuron outputNode, node;

            // Calculate output error values
            for (i = 0; i < this.outputLayer.Count; i++)
            {
                temp = this.outputLayer[i].Output;
                this.outputLayer[i].Error = (desiredResults[i] - temp) * temp * (1.0F - temp);
            }

            // calculate hidden layer error values
            for (i = 0; i < this.hiddenLayer.Count; i++)
            {
                node = this.hiddenLayer[i];
                error = 0;
                for (j = 0; j < this.outputLayer.Count; j++)
                {
                    outputNode = this.outputLayer[j];
                    error += outputNode.Error * outputNode.Input[node].Weight * node.Output * (1.0 - node.Output);
                }

                node.Error = error;
            }

            this.ApplyLearning();
        }
    }
}