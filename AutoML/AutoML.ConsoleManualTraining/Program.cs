using System;
namespace AutoML.ConsoleManualTraining
{
    class Program
    {
        static void Main(string[] args)
        {
            var mb = new ModelBuilder(new Microsoft.ML.MLContext(seed: 1));
            mb.Start();
        }
    }
}
