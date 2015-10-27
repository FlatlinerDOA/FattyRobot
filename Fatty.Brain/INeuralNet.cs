namespace Fatty.Brain
{
    public interface INeuralNet
    {
        INeuralLayer Hidden
        {
            get;
        }

        INeuralLayer Input
        {
            get;
        }

        INeuralLayer Output
        {
            get;
        }

        void ApplyLearning();
        void Pulse();
    }
}