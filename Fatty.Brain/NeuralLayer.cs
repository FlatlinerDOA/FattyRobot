using System;
using System.Collections.ObjectModel;

namespace Fatty.Brain
{
    public sealed class NeuralLayer : Collection<INeuron>, INeuralLayer
    {
        public void ApplyLearning(INeuralLayer input, double learningRate)
        {
            // adjust output layer weight change
            int i, j;
            INeuron node;
            for (i = 0; i < input.Count; i++)
            {
                node = input[i];
                for (j = 0; j < this.Count; j++)
                {
                    var outputNode = this[j];
                    outputNode.Input[node].Weight += learningRate * this[j].Error * node.Output;
                    outputNode.Bias.Delta += learningRate * this[j].Error * outputNode.Bias.Weight;
                }
            }

        }

        public void Pulse()
        {
            foreach (INeuron n in this)
            {
                n.Pulse(this);
            }
        }

        public void Connect(NeuralLayer inputLayer, Random rand)
        {
            int i, j;
            for (i = 0; i < this.Count; i++)
            {
                for (j = 0; j < inputLayer.Count; j++)
                {
                    this[i].Input.Add(inputLayer[j], new NeuralFactor(rand.NextDouble()));
                }
            }
        }
    }
}