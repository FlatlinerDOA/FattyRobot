
namespace Fatty.Console
{
    using System;
    using Fatty.Brain;

    public static class Program
    {
        private static void Main(string[] args)
        {
            TestNeuralNetXor();
            Console.ReadKey();
        }

        private static void TestNeuralNetXor()
        {
            var net = new NeuralNet();

            double high, mid, low;
            high = .9;
            low = .1;
            mid = .5;

            // initialize with

            //   2 perception neurons

            //   2 hidden layer neurons

            //   1 output neuron

            net.Initialize(1, 2, 2, 1);

            double[][] input = new double[4][];

            input[0] = new double[] { high, high };

            input[1] = new double[] { low, high };

            input[2] = new double[] { high, low };

            input[3] = new double[] { low, low };

            double[][] output = new double[4][];

            output[0] = new double[] { low };

            output[1] = new double[] { high };

            output[2] = new double[] { high };

            output[3] = new double[] { low };

            double ll, lh, hl, hh;

            int count;

            count = 0;

            do

            {
                count++;

                for (int i = 0; i < 100; i++)

                    net.Train(input, output);

                net.ApplyLearning();

                net.Input[0].Output = low;

                net.Input[1].Output = low;

                net.Pulse();

                ll = net.Output[0].Output;

                net.Input[0].Output = high;

                net.Input[1].Output = low;

                net.Pulse();

                hl = net.Output[0].Output;

                net.Input[0].Output = low;

                net.Input[1].Output = high;

                net.Pulse();

                lh = net.Output[0].Output;

                net.Input[0].Output = high;

                net.Input[1].Output = high;

                net.Pulse();

                hh = net.Output[0].Output;
            }

            while (hh > mid || lh < mid || hl < mid || ll > mid);

            Console.WriteLine((count * 100).ToString() + " iterations required for training");
        }
    }
}