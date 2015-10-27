using System.Collections.Generic;

namespace Fatty.Brain
{
    public interface INeuronReceptor
    {
        Dictionary<INeuronTransmitter, NeuralFactor> Input
        {
            get;
        }
    }
}