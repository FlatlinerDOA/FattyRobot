using System.Collections.Generic;

namespace Fatty.Brain
{
    public interface INeuralLayer : IList<INeuron>
    {
        void ApplyLearning(INeuralLayer input, double learningRate);
        void Pulse();
    }
}