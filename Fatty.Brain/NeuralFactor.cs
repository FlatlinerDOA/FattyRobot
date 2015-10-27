namespace Fatty.Brain
{
    public class NeuralFactor
    {
        private double delta;
        private double weight;
#region Constructors
        public NeuralFactor(double weight)
        {
            this.weight = weight;
            this.delta = 0;
        }

#endregion
#region Properties
        public double Delta
        {
            get
            {
                return this.delta;
            }

            set
            {
                this.delta = value;
            }
        }

        public double Weight
        {
            get
            {
                return this.weight;
            }

            set
            {
                this.weight = value;
            }
        }

#endregion
#region Methods
        public void ApplyDelta()
        {
            this.weight += this.delta;
            this.delta = 0;
        }
#endregion
    }
}