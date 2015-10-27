using System;
using System.Collections.Generic;

namespace Fatty.Brain
{
    public class Neuron : INeuron
    {
        private readonly Dictionary<INeuronTransmitter, NeuralFactor> input = new Dictionary<INeuronTransmitter, NeuralFactor>();
        private NeuralFactor bias;
        private double output;
        public Neuron(NeuralFactor bias)
        {
            this.bias = bias;
            this.BiasWeight = 1;
        }

        public NeuralFactor Bias
        {
            get
            {
                return this.bias;
            }
        }

        public double BiasWeight
        {
            get;
            set;
        }

        public double Error
        {
            get;
            set;
        }

        public Dictionary<INeuronTransmitter, NeuralFactor> Input
        {
            get
            {
                return this.input;
            }
        }

        public double Output
        {
            get
            {
                return this.output;
            }

            set
            {
                this.output = value;
            }
        }

        public void ApplyLearning(INeuralLayer layer)
        {
        }

        public void Pulse(INeuralLayer layer)
        {
            this.output = 0;
            foreach (KeyValuePair<INeuronTransmitter, NeuralFactor> item in this.input)
            {
                this.output += item.Key.Output * item.Value.Weight;
            }

            this.output = Sigmoid(this.output + (this.bias.Weight * this.BiasWeight));
        }

        private static double Sigmoid(double value)
        {
            return 1 / (1 + Math.Exp(-value));
        }
    }
}