using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.ML;
using CatsAndDogs.ML.DataModels;
using System.Linq;
using Microsoft.ML.Vision;
using System.Diagnostics;
using System.IO;

namespace CatsAndDogs.ML.Training
{
    public class Trainer
    {
        private MLContext mlContext = new MLContext();

        private string _dataFolder;
        private string _modelsFolder;
        private string _workspaceFolder;
        private string _baseModelFileName;

        public Trainer(string dataFolder, string modelsFolder, string workspaceFolder, string baseModelFileName)
        {
            _dataFolder = dataFolder;
            _modelsFolder = modelsFolder;
            _workspaceFolder = workspaceFolder;
            _baseModelFileName = baseModelFileName;
        }

        public void TrainModel(string testImagePath = null)
        {
            #region Notes: Fundamental components
            /*  Main components:
             *      IDataView, 
             *      ITransformer, 
             *      IEstimator
             */

            //IDataView demoDataView;
            //ITransformer demoITransformer;
            //IEstimator<ITransformer> demoIEstimator;
            #endregion Notes: Fundamental components
            #region Notes: Conventional column names
            /*  Conventional column names:
             *      Input:
             *          Label
             *          Features
             *      Output:
             *          PredictedLabel
             *          Score
             */
            #endregion Notes: Conventional column names
            #region Notes: Usual training process
            /*  Usual training process:
             *      1. Load training/test datasets (IDataView)
             *      2. Build training pipeline (IEstimator)
             *          2.1   Construct preProcessing pipeline (IEstimator) (optional)
             *          2.2   Configure trainer (IEstimator)
             *          2.3   Construct postProcessing pipeline (optional)
             *          2.4   Construct training pipeline (preProcessing pipelin + trainer + postProcessing pipline            
             *      3. Train model using training dataset (ITransformer)
             *      4. Evaluate model perfomance
             *          4.1 Make predictions on test data using trained model (IDataView)
             *          4.2 Compute evaluation metrics (Metrics staticsitcs)            
             *      (optional) Retrain on full dataset (Itransformer)
             *      5. Save model to filesystem
             *      6. Make single prediction            
             */
            #endregion Notes: Usual training process

            // Load data
            IDataView imagesInfo = LoadData(_dataFolder);            
            imagesInfo = mlContext.Data.ShuffleRows(imagesInfo);
            DataOperationsCatalog.TrainTestData dataSplit = mlContext.Data.TrainTestSplit(imagesInfo, testFraction: 0.2);            

            // Pre processing
            IEstimator<ITransformer> e_preProcessing_readImageBytes = mlContext.Transforms.LoadRawImageBytes(
                inputColumnName: nameof(ImageFileInputModel.ImagePath),
                outputColumnName: nameof(ImageInputModel.Image),
                imageFolder: _dataFolder);

            IEstimator<ITransformer> e_preProcessing_labelKeyMapping = mlContext.Transforms.Conversion.MapValueToKey(
                inputColumnName: nameof(BaseInputModel.Label),
                outputColumnName: "LabelAsKey",
                keyOrdinality: Microsoft.ML.Transforms.ValueToKeyMappingEstimator.KeyOrdinality.ByValue);
            

            ITransformer t_preProcessing_labelKeyMapping = e_preProcessing_labelKeyMapping.Fit(imagesInfo);
            ITransformer t_preProcessing_readImageBytes = e_preProcessing_readImageBytes.Fit(imagesInfo);
            ITransformer t_preProcessingPipeline = t_preProcessing_labelKeyMapping.Append(t_preProcessing_readImageBytes);


            // Core Model training pipeline
            IDataView testSetTransformed = t_preProcessingPipeline.Transform(dataSplit.TestSet);
            ImageClassificationTrainer.Options trainerSettings = new ImageClassificationTrainer.Options
            {
                FeatureColumnName = nameof(ImageInputModel.Image),
                LabelColumnName = "LabelAsKey",
                Arch = ImageClassificationTrainer.Architecture.ResnetV2101,
                Epoch = 100,
                BatchSize = 200,
                LearningRate = 0.05f,
                MetricsCallback = (m) => Console.WriteLine(m),
                ValidationSet = testSetTransformed,
                WorkspacePath = _workspaceFolder
            };

            IEstimator<ITransformer> e_trainer = mlContext.MulticlassClassification.Trainers.ImageClassification(trainerSettings);
            IEstimator<ITransformer> e_postProcessing_labelKeyMapping = mlContext.Transforms.Conversion.MapKeyToValue(
                    inputColumnName: "PredictedLabel",
                    outputColumnName: nameof(PredictionModel.PredictedLabel));

            IEstimator<ITransformer> trainingPipeline = e_trainer.Append(e_postProcessing_labelKeyMapping);

            // Train
            #region Notes: On metadata
            /*
             * Metadata source: https://aka.ms/mlnet-resources/resnet_v2_101_299.meta
             * System.IO.Path.GetTempPath() -  C:\Users\User\AppData\Local\Temp\
             */
            #endregion
            ITransformer trainedModel = Train(trainingPipeline, t_preProcessingPipeline.Transform(dataSplit.TrainSet));

            #region Notes: Model composition
            //var extractPixelsEst = mlContext.Transforms.ExtractPixels();
            //var resizeEst = mlContext.Transforms.ResizeImages();
            //IEstimator<ITransformer> est = mlContext.Model.LoadTensorFlowModel("MODEL_PATH")
            //.ScoreTensorFlowModel(
            //outputColumnNames: new[] { "some-name" },
            //inputColumnNames: new[] { "Features" }, addBatchDimensionInput: true);
            #endregion Model composition

            // Evaluate/Save FileSystemModel            
            ITransformer fileSystemModel = t_preProcessingPipeline.Append(trainedModel);
            Evaluate(fileSystemModel, dataSplit.TestSet);
            SaveModel(fileSystemModel,
                new DataViewSchema.Column[] {
                    imagesInfo.Schema.First(x => x.Name == nameof(ImageFileInputModel.ImagePath)),
                    imagesInfo.Schema.First(x=>x.Name==nameof(BaseInputModel.Label))
                },
                ResolveModelFileName("fromFile"));

            // Evaluate/Save InMemoryModel
            IDataView testSetImageExtracted = t_preProcessing_readImageBytes.Transform(dataSplit.TrainSet);

            ITransformer inMemoryModel = t_preProcessing_labelKeyMapping.Append(trainedModel);
            Evaluate(inMemoryModel, testSetImageExtracted);
            SaveModel(inMemoryModel,
                new DataViewSchema.Column[] {
                    testSetImageExtracted.Schema.First(x => x.Name == nameof(ImageFileInputModel.ImagePath)),
                    testSetImageExtracted.Schema.First(x=>x.Name==nameof(BaseInputModel.Label))
                },
                ResolveModelFileName("inMemory"));

            //Try a single prediction
            if (!string.IsNullOrWhiteSpace(testImagePath))
                MakeSinglePrediction(testImagePath);

        }
        public void MakeSinglePrediction(string filePath)
        {
            // Try a single prediction simulating an end-user app
            Console.WriteLine($"Trying single predictions for file: {filePath}");
            Console.WriteLine("=================IN MEMORY MODEL===============");
            TrySinglePrediction(new InMemoryInputData() { Image = File.ReadAllBytes(filePath) }, ResolveModelFileName("inMemory"));
            Console.WriteLine();
            Console.WriteLine("=================FROM FILE MODEL===============");
            TrySinglePrediction(new FileInputData { ImagePath = filePath }, ResolveModelFileName("fromFile"));
        }

        private IDataView LoadData(string folder)
        {
            IEnumerable<ImageFileInputModel> imagesInfo = Directory.GetFiles(folder, "*", searchOption: SearchOption.AllDirectories)
                .Where(x => Path.GetExtension(x) == ".jpg" || Path.GetExtension(x) == ".png")
                .Select(imagePath => new ImageFileInputModel(imagePath, Directory.GetParent(imagePath).Name));
            
            IDataView imagesInfoDataView = mlContext.Data.LoadFromEnumerable(imagesInfo);
      
            return imagesInfoDataView;
        }

        private ITransformer Train(IEstimator<ITransformer> trainingPipeline, IDataView trainingData)
        {
            Console.WriteLine("--- Training the model ---");
            Stopwatch stopWatch = Stopwatch.StartNew();

            ITransformer trainedModel = trainingPipeline.Fit(trainingData);

            stopWatch.Stop();
            Console.WriteLine($"Training took: {stopWatch.ElapsedMilliseconds / 1000} seconds");
            return trainedModel;
        }

        private void Evaluate(ITransformer model, IDataView testData)
        {
            Console.WriteLine("--- Making bulk test predictions and computing evaluation metrics ---");
            Stopwatch stopWatch = Stopwatch.StartNew();

            IDataView testPredictions = model.Transform(testData);
            var metrics = mlContext.MulticlassClassification.Evaluate(testPredictions,
                labelColumnName: "LabelAsKey", predictedLabelColumnName: "PredictedLabel");            

            stopWatch.Stop();
            Console.WriteLine($"Predicting and Evaluation took: {stopWatch.ElapsedMilliseconds / 1000} seconds");
            PrintMultiClassClassificationMetrics("TensorFlow DNN Transfer Learning", metrics);
        }
        private void PrintMultiClassClassificationMetrics(string name, Microsoft.ML.Data.MulticlassClassificationMetrics metrics)
        {
            Console.WriteLine($"************************************************************");
            Console.WriteLine($"*    Metrics for {name} multi-class classification model   ");
            Console.WriteLine($"*-----------------------------------------------------------");
            Console.WriteLine($"    AccuracyMacro = {metrics.MacroAccuracy:0.####}, a value between 0 and 1, the closer to 1, the better");
            Console.WriteLine($"    AccuracyMicro = {metrics.MicroAccuracy:0.####}, a value between 0 and 1, the closer to 1, the better");
            Console.WriteLine($"    LogLoss = {metrics.LogLoss:0.####}, the closer to 0, the better");

            int i = 0;
            foreach (var classLogLoss in metrics.PerClassLogLoss)
            {
                i++;
                Console.WriteLine($"    LogLoss for class {i} = {classLogLoss:0.####}, the closer to 0, the better");
            }
            Console.WriteLine($"************************************************************");
        }

        private void SaveModel(ITransformer model, IEnumerable<DataViewSchema.Column> schemaColumns, string path)
        {
            var schemaBuilder = new DataViewSchema.Builder();
            schemaBuilder.AddColumns(schemaColumns);
            var schema = schemaBuilder.ToSchema();

            Console.WriteLine($"--- Saving the model");
            mlContext.Model.Save(model, schema, path);
            Console.WriteLine($"Model saved to: {path}");
        }

        private void TrySinglePrediction<TSrc>(TSrc src, string modelPath) where TSrc : class
        {
            try
            {
                var model = mlContext.Model.Load(modelPath, out var schema);
                var engine = mlContext.Model.CreatePredictionEngine<TSrc, OutputData>(model);
                var result = engine.Predict(src);
                Console.WriteLine();
                Console.WriteLine($"Test prediction for model {Path.GetFileName(modelPath)}: {result.PredictedLabel}");
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("=================Error==============");
                Console.WriteLine(ex?.Message);
            }
        }

        private string ResolveModelFileName(string suffix)
        {
            string fileName = $"{_baseModelFileName}_{suffix}.zip";
            return Path.Combine(_modelsFolder, fileName);
        }
        

    }

    public class InMemoryInputData
    {
        public byte[] Image { get; set; }
        public string Label { get; set; }
    }

    public class FileInputData
    {
        public string ImagePath { get; set; }
        public string Label { get; set; }
    }

    public class OutputData
    {
        
        public string PredictedLabel { get; set; }
        public float[] Score { get; set; }
    }

}
