using System;
using System.IO;
namespace NeuralModels
{
  class NeuralModelsProgram
  {
    static void Main(string[] args)
    {
      Console.WriteLine("\nBegin neural network model demo\n");

      double[][] allData = LoadData("..\\..\\IrisData.txt", 150, 7); // 150 rows, 7 cols
      Console.WriteLine("Goal is to predict iris species from sepal length,");
      Console.WriteLine("sepal width, petal length, petal width");
      Console.WriteLine("Setosa = (1,0,0), vericolor = (0,1,0), virginica = (0,0,1)");
      Console.WriteLine("\nThe 150-item data set is:\n");
      ShowMatrix(allData, 4, 1, true);

      double[][] trainData = null;
      double[][] testData = null;
      double trainPct = 0.80;
      int splitSeed = 1;
      Console.WriteLine("Splitting data into 80% train, 20% test");
      SplitData(allData, trainPct, splitSeed, out trainData, out testData);
      Console.WriteLine("\nThe training data is:\n");     
      ShowMatrix(trainData, 4, 1, true);
      Console.WriteLine("The test data is:\n");     
      ShowMatrix(testData, 3, 1, true);
      
      int numInput = 4;
      int numHidden = 5;
      int numOutput = 3;

      Console.WriteLine("Creating a " + numInput + "-" + numHidden +
        "-" + numOutput + " neural network");
      NeuralNetwork nn = new NeuralNetwork(numInput, numHidden, numOutput);

      int maxEpochs = 1000;
      double learnRate = 0.05;
      double momentum = 0.01;
      bool progress = true;

      Console.WriteLine("\nSetting maxEpochs = " + maxEpochs);
      Console.WriteLine("Setting learnRate = " + learnRate);
      Console.WriteLine("Setting momentum  = " + momentum);

      Console.WriteLine("\nStarting training\n");
      double[] wts = nn.Train(trainData, maxEpochs, learnRate, momentum, progress);
      Console.WriteLine("\nTraining complete");

      Console.WriteLine("\nThe weights and biases are:");
      ShowVector(wts, 4, 10, true);

      double trainAcc = nn.Accuracy(trainData);
      double testAcc = nn.Accuracy(testData);

      Console.WriteLine("\nAccuracy on train data = " + trainAcc.ToString("F3"));
      Console.WriteLine("Accuracy on test data  = " + testAcc.ToString("F3"));

      string modelName = "..\\..\\iris_model_001.txt";
      Console.WriteLine("\nSaving model as " + modelName);
      nn.SaveModel(modelName);

      Console.WriteLine("\nLoading model into new neural network");
      NeuralNetwork nm = new NeuralNetwork();
      nm.LoadModel(modelName);
      // alternative is to define a constructor that accepts a model
      double modelAcc = nm.Accuracy(testData);
      Console.WriteLine("\nNew neural network accuracy on test data  = " + testAcc.ToString("F3"));

      Console.WriteLine("\nEnd neural network model demo\n");
      Console.ReadLine();
    } // Main

    static double[][] LoadData(string dataFile, int numRows, int numCols)
    {
      double[][] result = new double[numRows][];

      FileStream ifs = new FileStream(dataFile, FileMode.Open);
      StreamReader sr = new StreamReader(ifs);
      string line = "";
      string[] tokens = null;
      int i = 0;
      while ((line = sr.ReadLine()) != null)
      {
        tokens = line.Split(',');
        result[i] = new double[numCols];
        for (int j = 0; j < numCols; ++j)
        {
          result[i][j] = double.Parse(tokens[j]);
        }
        ++i;
      }
      sr.Close();
      ifs.Close();
      return result;
    }

    static void SplitData(double[][] allData, double trainPct,
       int seed, out double[][] trainData, out double[][] testData)
    {
      Random rnd = new Random(seed);
      int totRows = allData.Length;
      int numTrainRows = (int)(totRows * trainPct); // usually 0.80
      int numTestRows = totRows - numTrainRows;
      trainData = new double[numTrainRows][];
      testData = new double[numTestRows][];

      double[][] copy = new double[allData.Length][]; // ref copy of data
      for (int i = 0; i < copy.Length; ++i)
        copy[i] = allData[i];

      for (int i = 0; i < copy.Length; ++i) // scramble order of copy
      {
        int r = rnd.Next(i, copy.Length); // use Fisher-Yates
        double[] tmp = copy[r];
        copy[r] = copy[i];
        copy[i] = tmp;
      }
      for (int i = 0; i < numTrainRows; ++i) // by ref
        trainData[i] = copy[i];

      for (int i = 0; i < numTestRows; ++i)
        testData[i] = copy[i + numTrainRows];
    } // SplitData

    public static void ShowMatrix(double[][] matrix, int numRows,
      int decimals, bool indices)
    {
      int len = matrix.Length.ToString().Length;
      for (int i = 0; i < numRows; ++i)
      {
        if (indices == true)
          Console.Write("[" + i.ToString().PadLeft(len) + "]  ");
        for (int j = 0; j < matrix[i].Length; ++j)
        {
          double v = matrix[i][j];
          if (v >= 0.0)
            Console.Write(" "); // '+'
          Console.Write(v.ToString("F" + decimals) + "  ");
        }
        Console.WriteLine("");
      }

      if (numRows < matrix.Length)
      {
        Console.WriteLine(". . .");
        int lastRow = matrix.Length - 1;
        if (indices == true)
          Console.Write("[" + lastRow.ToString().PadLeft(len) + "]  ");
        for (int j = 0; j < matrix[lastRow].Length; ++j)
        {
          double v = matrix[lastRow][j];
          if (v >= 0.0)
            Console.Write(" "); // '+'
          Console.Write(v.ToString("F" + decimals) + "  ");
        }
      }
      Console.WriteLine("\n");
    }

    public static void ShowVector(double[] vector, int decimals,
      int lineLen, bool newLine)
    {
      for (int i = 0; i < vector.Length; ++i)
      {
        if (i > 0 && i % lineLen == 0) Console.WriteLine("");
        if (vector[i] >= 0) Console.Write(" ");
        Console.Write(vector[i].ToString("F" + decimals) + " ");
      }
      if (newLine == true)
        Console.WriteLine("");
    }

  } // Program class 

  // ----------------

  public class NeuralNetwork
  {
    private int numInput; // number input nodes
    private int numHidden;
    private int numOutput;

    private double[] inputs;
    private double[][] ihWeights; // input-hidden
    private double[] hBiases;
    private double[] hOutputs;

    private double[][] hoWeights; // hidden-output
    private double[] oBiases;
    private double[] outputs;

    private Random rnd;

    public NeuralNetwork()
    {
      return;
    }

    public NeuralNetwork(int numInput, int numHidden, int numOutput)
    {
      this.numInput = numInput;
      this.numHidden = numHidden;
      this.numOutput = numOutput;

      this.inputs = new double[numInput];

      this.ihWeights = MakeMatrix(numInput, numHidden);
      this.hBiases = new double[numHidden];
      this.hOutputs = new double[numHidden];

      this.hoWeights = MakeMatrix(numHidden, numOutput);
      this.oBiases = new double[numOutput];
      this.outputs = new double[numOutput];

      this.rnd = new Random(4);
      this.InitializeWeights(); // all weights and biases
    } // ctor

    private static double[][] MakeMatrix(int rows,
      int cols) // helper for ctor, Train
    {
      double[][] result = new double[rows][];
      for (int r = 0; r < result.Length; ++r)
        result[r] = new double[cols];
      return result;
    }

    private void InitializeWeights() // helper for ctor
    {
      // initialize weights and biases to random values between 0.0001 and 0.001
      int numWeights = (numInput * numHidden) +
        (numHidden * numOutput) + numHidden + numOutput;
      double[] initialWeights = new double[numWeights];
      for (int i = 0; i < initialWeights.Length; ++i)
        initialWeights[i] = (0.001 - 0.0001) * rnd.NextDouble() + 0.0001;
      this.SetWeights(initialWeights);
    }

    public void SetWeights(double[] weights)
    {
      // copy serialized weights and biases in weights[] array
      // to i-h weights, i-h biases, h-o weights, h-o biases
      int numWeights = (numInput * numHidden) +
        (numHidden * numOutput) + numHidden + numOutput;
      if (weights.Length != numWeights)
        throw new Exception("Bad weights array in SetWeights");

      int k = 0; // points into weights param

      for (int i = 0; i < numInput; ++i)
        for (int j = 0; j < numHidden; ++j)
          ihWeights[i][j] = weights[k++];
      for (int i = 0; i < numHidden; ++i)
        hBiases[i] = weights[k++];
      for (int i = 0; i < numHidden; ++i)
        for (int j = 0; j < numOutput; ++j)
          hoWeights[i][j] = weights[k++];
      for (int i = 0; i < numOutput; ++i)
        oBiases[i] = weights[k++];
    }

    public double[] GetWeights()
    {
      int numWeights = (numInput * numHidden) +
        (numHidden * numOutput) + numHidden + numOutput;
      double[] result = new double[numWeights];
      int k = 0;
      for (int i = 0; i < ihWeights.Length; ++i)
        for (int j = 0; j < ihWeights[0].Length; ++j)
          result[k++] = ihWeights[i][j];
      for (int i = 0; i < hBiases.Length; ++i)
        result[k++] = hBiases[i];
      for (int i = 0; i < hoWeights.Length; ++i)
        for (int j = 0; j < hoWeights[0].Length; ++j)
          result[k++] = hoWeights[i][j];
      for (int i = 0; i < oBiases.Length; ++i)
        result[k++] = oBiases[i];
      return result;
    }

    public void SaveModel(string modelName)
    {
      FileStream ofs = new FileStream(modelName, FileMode.Create);
      StreamWriter sw = new StreamWriter(ofs);
      sw.WriteLine("numInput:" + this.numInput);
      sw.WriteLine("numHidden:" + this.numHidden);
      sw.WriteLine("numOutput:" + this.numOutput);

      sw.Write("weights:");
      for (int i = 0; i < ihWeights.Length; ++i)
        for (int j = 0; j < ihWeights[0].Length; ++j)
          sw.Write(ihWeights[i][j].ToString("F4") + ",");
      for (int i = 0; i < hBiases.Length; ++i)
        sw.Write(hBiases[i].ToString("F4") + ",");
      for (int i = 0; i < hoWeights.Length; ++i)
        for (int j = 0; j < hoWeights[0].Length; ++j)
          sw.Write(hoWeights[i][j].ToString("F4") + ",");
      for (int i = 0; i < oBiases.Length - 1; ++i)
        sw.Write(oBiases[i].ToString("F4") + ",");
      sw.WriteLine(oBiases[oBiases.Length-1].ToString("F4"));

      sw.Close();
      ofs.Close();
    }

    public void LoadModel(string modelName)
    {
      FileStream ifs = new FileStream(modelName, FileMode.Open);
      StreamReader sr = new StreamReader(ifs);

      int numInput = 0;
      int numHidden = 0;
      int numOutput = 0;
      double[] wts = null;
      string line = "";
      string[] tokens = null;
      while ((line = sr.ReadLine()) != null)
      {
        if (line.StartsWith("//") == true) continue;
        tokens = line.Split(':');
        if (tokens[0] == "numInput")
           numInput = int.Parse(tokens[1]);
        else if (tokens[0] == "numHidden")
          numHidden = int.Parse(tokens[1]);
        else if (tokens[0] == "numOutput")
          numOutput = int.Parse(tokens[1]);
        else if (tokens[0] == "weights")
        {
          //int tot = (numInput * numHidden) + numHidden +
          //(numHidden * numOutput) + numOutput;
          string[] vals = tokens[1].Split(',');
          wts = new double[vals.Length];
          for (int i = 0; i < wts.Length; ++i)
            wts[i] = double.Parse(vals[i]);
        }
      }

      sr.Close();
      ifs.Close();

      this.numInput = numInput;
      this.numHidden = numHidden;
      this.numOutput = numOutput;
      this.inputs = new double[numInput];

      this.ihWeights = MakeMatrix(numInput, numHidden);
      this.hBiases = new double[numHidden];
      this.hOutputs = new double[numHidden];

      this.hoWeights = MakeMatrix(numHidden, numOutput);
      this.oBiases = new double[numOutput];
      this.outputs = new double[numOutput];

      this.rnd = new Random(4);
      
      this.SetWeights(wts);
    } // LoadModel

    public double[] ComputeOutputs(double[] xValues)
    {
      double[] hSums = new double[numHidden]; // hidden nodes sums scratch array
      double[] oSums = new double[numOutput]; // output nodes sums

      for (int i = 0; i < xValues.Length; ++i) // copy x-values to inputs
        this.inputs[i] = xValues[i];
      // note: no need to copy x-values unless you implement a ToString.
      // more efficient is to simply use the xValues[] directly.


      for (int j = 0; j < numHidden; ++j)  // compute i-h sum of weights * inputs
        for (int i = 0; i < numInput; ++i)
          hSums[j] += this.inputs[i] * this.ihWeights[i][j]; // note +=

      for (int i = 0; i < numHidden; ++i)  // add biases to input-to-hidden sums
        hSums[i] += this.hBiases[i];

      for (int i = 0; i < numHidden; ++i)   // apply activation
        this.hOutputs[i] = HyperTan(hSums[i]); // hard-coded

      for (int j = 0; j < numOutput; ++j)   // compute h-o sum of weights * hOutputs
        for (int i = 0; i < numHidden; ++i)
          oSums[j] += hOutputs[i] * hoWeights[i][j];

      for (int i = 0; i < numOutput; ++i)  // add biases to input-to-hidden sums
        oSums[i] += oBiases[i];

      double[] softOut = Softmax(oSums); // all outputs at once for efficiency
      Array.Copy(softOut, outputs, softOut.Length);

      double[] retResult = new double[numOutput]; // could define a GetOutputs 
      Array.Copy(this.outputs, retResult, retResult.Length);
      return retResult;
    }

    private static double HyperTan(double x)
    {
      if (x < -20.0) return -1.0; // approximation is correct to 30 decimals
      else if (x > 20.0) return 1.0;
      else return Math.Tanh(x);
    }

    private static double[] Softmax(double[] oSums)
    {
      // does all output nodes at once so scale
      // doesn't have to be re-computed each time

      // if (oSums.Length < 2) throw . . . 

      double[] result = new double[oSums.Length];

      double sum = 0.0;
      for (int i = 0; i < oSums.Length; ++i)
        sum += Math.Exp(oSums[i]);

      for (int i = 0; i < oSums.Length; ++i)
        result[i] = Math.Exp(oSums[i]) / sum;

      return result; // now scaled so that xi sum to 1.0
    }

    public double[] Train(double[][] trainData,
      int maxEpochs, double learnRate,
      double momentum, bool progress)
    {
      // train using back-prop
      // back-prop specific arrays
      double[][] hoGrads = MakeMatrix(numHidden, numOutput); // hidden-to-output weights gradients
      double[] obGrads = new double[numOutput];                   // output biases gradients

      double[][] ihGrads = MakeMatrix(numInput, numHidden);  // input-to-hidden weights gradients
      double[] hbGrads = new double[numHidden];                   // hidden biases gradients

      double[] oSignals = new double[numOutput];                  // output signals - gradients w/o associated input terms
      double[] hSignals = new double[numHidden];                  // hidden node signals

      // back-prop momentum specific arrays 
      double[][] ihPrevWeightsDelta = MakeMatrix(numInput, numHidden);
      double[] hPrevBiasesDelta = new double[numHidden];
      double[][] hoPrevWeightsDelta = MakeMatrix(numHidden, numOutput);
      double[] oPrevBiasesDelta = new double[numOutput];

      // train a back-prop style NN classifier using learning rate and momentum
      int epoch = 0;
      double[] xValues = new double[numInput]; // inputs
      double[] tValues = new double[numOutput]; // target values

      int[] sequence = new int[trainData.Length];
      for (int i = 0; i < sequence.Length; ++i)
        sequence[i] = i;

      int errInterval = maxEpochs / 10; // interval to check validation data
      while (epoch < maxEpochs)
      {
        ++epoch;

        if (progress == true && epoch % errInterval == 0 && epoch < maxEpochs)
        {
          double trainErr = Error(trainData);
          Console.WriteLine("epoch = " + epoch + "  training error = " +
            trainErr.ToString("F4"));
          //Console.ReadLine();
        }

        Shuffle(sequence); // visit each training data in random order
        for (int ii = 0; ii < trainData.Length; ++ii)
        {
          int idx = sequence[ii];
          Array.Copy(trainData[idx], xValues, numInput);
          Array.Copy(trainData[idx], numInput, tValues, 0, numOutput);
          ComputeOutputs(xValues); // copy xValues in, compute outputs 

          // indices: i = inputs, j = hiddens, k = outputs

          // 1. compute output nodes signals (assumes softmax)
          for (int k = 0; k < numOutput; ++k)
            oSignals[k] = (tValues[k] - outputs[k]) * (1 - outputs[k]) * outputs[k];

          // 2. compute hidden-to-output weights gradients using output signals
          for (int j = 0; j < numHidden; ++j)
          {
            for (int k = 0; k < numOutput; ++k)
            {
              hoGrads[j][k] = oSignals[k] * hOutputs[j];
            }
          }

          // 2b. compute output biases gradients using output signals
          for (int k = 0; k < numOutput; ++k)
            obGrads[k] = oSignals[k] * 1.0; // dummy assoc. input value

          // 3. compute hidden nodes signals
          for (int j = 0; j < numHidden; ++j)
          {
            double sum = 0.0; // need sums of output signals times hidden-to-output weights
            for (int k = 0; k < numOutput; ++k)
            {
              sum += oSignals[k] * hoWeights[j][k];
            }
            hSignals[j] = (1 + hOutputs[j]) * (1 - hOutputs[j]) * sum;  // assumes tanh
          }

          // 4. compute input-hidden weights gradients
          for (int i = 0; i < numInput; ++i)
            for (int j = 0; j < numHidden; ++j)
              ihGrads[i][j] = hSignals[j] * inputs[i];

          // 4b. compute hidden node biases gradienys
          for (int j = 0; j < numHidden; ++j)
            hbGrads[j] = hSignals[j] * 1.0; // dummy 1.0 input

          // == update weights and biases

          // update input-to-hidden weights
          for (int i = 0; i < numInput; ++i)
          {
            for (int j = 0; j < numHidden; ++j)
            {
              double delta = ihGrads[i][j] * learnRate;
              ihWeights[i][j] += delta;
              ihWeights[i][j] += ihPrevWeightsDelta[i][j] * momentum;
              ihPrevWeightsDelta[i][j] = delta; // save for next time
            }
          }

          // update hidden biases
          for (int j = 0; j < numHidden; ++j)
          {
            double delta = hbGrads[j] * learnRate;
            hBiases[j] += delta;
            hBiases[j] += hPrevBiasesDelta[j] * momentum;
            hPrevBiasesDelta[j] = delta;
          }

          // update hidden-to-output weights
          for (int j = 0; j < numHidden; ++j)
          {
            for (int k = 0; k < numOutput; ++k)
            {
              double delta = hoGrads[j][k] * learnRate;
              hoWeights[j][k] += delta;
              hoWeights[j][k] += hoPrevWeightsDelta[j][k] * momentum;
              hoPrevWeightsDelta[j][k] = delta;
            }
          }

          // update output node biases
          for (int k = 0; k < numOutput; ++k)
          {
            double delta = obGrads[k] * learnRate;
            oBiases[k] += delta;
            oBiases[k] += oPrevBiasesDelta[k] * momentum;
            oPrevBiasesDelta[k] = delta;
          }

        } // each training item

      } // while
      double[] bestWts = GetWeights();
      return bestWts;
    } // Train


    private void Shuffle(int[] sequence) // an instance method
    {
      for (int i = 0; i < sequence.Length; ++i)
      {
        int r = this.rnd.Next(i, sequence.Length);
        int tmp = sequence[r];
        sequence[r] = sequence[i];
        sequence[i] = tmp;
      }
    } // Shuffle

    private double Error(double[][] data)
    {
      // average squared error per training item
      double sumSquaredError = 0.0;
      double[] xValues = new double[numInput]; // first numInput values in trainData
      double[] tValues = new double[numOutput]; // last numOutput values

      // walk thru each training case. looks like (6.9 3.2 5.7 2.3) (0 0 1)
      for (int i = 0; i < data.Length; ++i)
      {
        Array.Copy(data[i], xValues, numInput);
        Array.Copy(data[i], numInput, tValues, 0, numOutput); // get target values
        double[] yValues = this.ComputeOutputs(xValues); // outputs using current weights
        for (int j = 0; j < numOutput; ++j)
        {
          double err = tValues[j] - yValues[j];
          sumSquaredError += err * err;
        }
      }
      return sumSquaredError / data.Length;
    } // Error

    public double Accuracy(double[][] data)
    {
      // percentage correct using winner-takes all
      int numCorrect = 0;
      int numWrong = 0;
      double[] xValues = new double[numInput]; // inputs
      double[] tValues = new double[numOutput]; // targets
      double[] yValues; // computed Y

      for (int i = 0; i < data.Length; ++i)
      {
        Array.Copy(data[i], xValues, numInput); // get x-values
        Array.Copy(data[i], numInput, tValues, 0, numOutput); // get t-values
        yValues = this.ComputeOutputs(xValues);
        int maxIndex = MaxIndex(yValues); // which cell in yValues has largest value?
        int tMaxIndex = MaxIndex(tValues);

        if (maxIndex == tMaxIndex)
          ++numCorrect;
        else
          ++numWrong;
      }
      return (numCorrect * 1.0) / (numCorrect + numWrong);
    }

    private static int MaxIndex(double[] vector) // helper for Accuracy()
    {
      // index of largest value
      int bigIndex = 0;
      double biggestVal = vector[0];
      for (int i = 0; i < vector.Length; ++i)
      {
        if (vector[i] > biggestVal)
        {
          biggestVal = vector[i];
          bigIndex = i;
        }
      }
      return bigIndex;
    }

    public string Predict(double[] xValues)
    {
      // specific to binary Iris data
      // (1, 0) = setosa, (0, 1) = versicolor
      double[] computedY = this.ComputeOutputs(xValues);
      int maxIndex = MaxIndex(computedY); // which cell in yValues has largest value?
      if (maxIndex == 0)
        return "setosa";
      else
        return "versicolor";
    }


  } // NeuralNetwork

  // ----------------

} // ns
