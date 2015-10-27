namespace Fatty.Brain
{
    public interface INeuron : INeuronTransmitter, INeuronReceptor
    {
        NeuralFactor Bias
        {
            get;
        }

        double BiasWeight
        {
            get;
        }

        double Error
        {
            get;
            set;
        }

        void ApplyLearning(INeuralLayer layer);
        void Pulse(INeuralLayer layer);
    }
}