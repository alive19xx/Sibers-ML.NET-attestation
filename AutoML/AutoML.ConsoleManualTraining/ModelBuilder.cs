using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ML;
using Microsoft.ML.AutoML;
using Microsoft.ML.Data;

namespace AutoML.ConsoleManualTraining
{
    public class ModelBuilder
    {        
        const string TRAIN_DATA_FILEPATH = @"C:\Projects\Attestation\AutoML\products.stats.csv";
        const string MODEL_FILEPATH = @"C:\Projects\Attestation\AutoML\AutoML.ConsoleManualTraining\model\model_manual.zip";
        const string CACHE_DIRECTORY = @"C:\Projects\Attestation\AutoML\Attestiation.ModelBuilderApi\ModelBuilderApi.Console\ModelBuilderApi.AutoMLApi.ConsoleApp\cache\";

        private MLContext mlContext;
        private IDataView trainData;
        private ColumnInformation columnInformation;
        private CancellationTokenSource cancelationTokenSource;


        public ModelBuilder(MLContext _mlContext)
        {
            mlContext = _mlContext;
            cancelationTokenSource = new CancellationTokenSource();
        }
        public void Start()
        {
            //Infer columns and load train data
            var columnInferenceResult = mlContext.Auto().InferColumns(
                path: TRAIN_DATA_FILEPATH,
                labelColumnName: "next",
                groupColumns: false);
            
            TextLoader textLoader = mlContext.Data.CreateTextLoader(columnInferenceResult.TextLoaderOptions);
            trainData = textLoader.Load(TRAIN_DATA_FILEPATH);

            //Modify infered columns information
            columnInformation = columnInferenceResult.ColumnInformation;

            columnInformation.CategoricalColumnNames.Add("productId");
            columnInformation.NumericColumnNames.Remove("productId");
            
            columnInformation.CategoricalColumnNames.Add("year");
            columnInformation.NumericColumnNames.Remove("year");

            columnInformation.NumericColumnNames.Remove("units");
            columnInformation.IgnoredColumnNames.Add("units");


            var experimentSettings = new RegressionExperimentSettings()
            {
                MaxExperimentTimeInSeconds = 10,
                OptimizingMetric = RegressionMetric.RootMeanSquaredError,
                CacheDirectory = new DirectoryInfo(CACHE_DIRECTORY),
                CancellationToken = cancelationTokenSource.Token
            };

            //Exclude trainers from experiment            
            experimentSettings.Trainers.Remove(RegressionTrainer.Ols);
            
            RegressionExperiment experiment = mlContext.Auto().CreateRegressionExperiment(experimentSettings);
            ExperimentResult<RegressionMetrics> experimentResult = experiment.Execute(
                 trainData: trainData,
                 columnInformation: columnInformation,
                 progressHandler: new RegressionProgressHandler(),
                 preFeaturizer: null);

            ITransformer model = experimentResult.BestRun.Model;
            IEstimator<ITransformer> estimator = experimentResult.BestRun.Estimator;            

            //Make batch predictions
            IDataView predictionsDataView = model.Transform(trainData);
            PrintPredictions(predictionsDataView);
            PrintPredictionsEnumerable(predictionsDataView);


            model = estimator.Fit(trainData);
            mlContext.Model.Save(model, trainData.Schema, MODEL_FILEPATH);
            Console.WriteLine("Done");
        }

        void PrintPredictions(IDataView predictionsDataView)
        {                    
            var rowCursor = predictionsDataView.GetRowCursor(predictionsDataView.Schema.ToArray());
            ValueGetter<float> predictedGetter = rowCursor.GetGetter<float>(predictionsDataView.Schema.GetColumnOrNull("Score").Value);
            var actualGetter = rowCursor.GetGetter<float>(predictionsDataView.Schema.GetColumnOrNull("next").Value);

            float predicted = 0;
            float actual = 0;

            while (rowCursor.MoveNext())
            {
                predictedGetter.Invoke(ref predicted);
                actualGetter.Invoke(ref actual);
                Console.WriteLine($"Prediction. Actual value: {actual}; Predicted value: {predicted}");
            }
        }
        void PrintPredictionsEnumerable(IDataView predictionsDataView)
        {
            var predictions = mlContext.Data.CreateEnumerable<ModelOutput>(
                data: predictionsDataView,
                reuseRowObject: true);
            foreach (var prediction in predictions)
            {
                Console.WriteLine($"Prediction. Actual value: {prediction.next}; Predicted value: {prediction.Score}");
            }
        }
    }

    public class RegressionProgressHandler : IProgress<RunDetail<RegressionMetrics>>
    {
        private int _iterationIndex;
        public static int Width = 114;
        public void Report(RunDetail<RegressionMetrics> iterationResult)
        {
            if (_iterationIndex++ == 0)
            {
                PrintRegressionMetricsHeader();
            }

            if (iterationResult.Exception != null)
            {
                PrintIterationException(iterationResult.Exception);
            }
            else
            {
                PrintIterationMetrics(_iterationIndex, iterationResult.TrainerName,
                    iterationResult.ValidationMetrics, iterationResult.RuntimeInSeconds);
            }
        }
        internal static void PrintRegressionMetricsHeader()
        {
            CreateRow($"{"",-4} {"Trainer",-35} {"RSquared",8} {"Absolute-loss",13} {"Squared-loss",12} {"RMS-loss",8} {"Duration",9}", Width);
        }

        private static void CreateRow(string message, int width)
        {
            Console.WriteLine("|" + message.PadRight(width - 2) + "|");
        }
        internal static void PrintIterationException(Exception ex)
        {
            Console.WriteLine($"Exception during AutoML iteration: {ex}");
        }
        internal static void PrintIterationMetrics(int iteration, string trainerName, RegressionMetrics metrics, double? runtimeInSeconds)
        {
            CreateRow($"{iteration,-4} {trainerName,-35} {metrics?.RSquared ?? double.NaN,8:F4} {metrics?.MeanAbsoluteError ?? double.NaN,13:F2} {metrics?.MeanSquaredError ?? double.NaN,12:F2} {metrics?.RootMeanSquaredError ?? double.NaN,8:F2} {runtimeInSeconds.Value,9:F1}", Width);
        }
    }
}
