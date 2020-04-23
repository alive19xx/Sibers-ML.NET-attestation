using System;
using Microsoft.ML;

namespace CatsAndDogs.ML.Training
{
    public class Program
    {
        //\CatsAndDogs.ML.Training\Trainer.cs
        static TrainingSettings demoSettings = new TrainingSettings
        {
            DataFolder = @"C:\Projects\Attestation\CatsAndDogs\DemoWorkspace\Dataset\train",
            ModelsFolder = @"C:\Projects\Attestation\CatsAndDogs\DemoWorkspace\Models",
            WorkspaceFolder = @"C:\Projects\Attestation\CatsAndDogs\DemoWorkspace\TempTraining",
            BaseModelFileName = "model"
        };
        static TrainingSettings normalSettings = new TrainingSettings
        {
            DataFolder = @"C:\Projects\Attestation\CatsAndDogs\Workspace\Dataset\train",
            ModelsFolder = @"C:\Projects\Attestation\CatsAndDogs\Workspace\",
            WorkspaceFolder = @"C:\Projects\Attestation\CatsAndDogs\Workspace\TempTraining",
            BaseModelFileName = "model"
        };

        static bool isDemo = true;

        static void Main(string[] args)
        {
            var settings = isDemo ? demoSettings : normalSettings;

            var trainerDemo = new Trainer(
                dataFolder: settings.DataFolder,
                modelsFolder: settings.ModelsFolder,
                workspaceFolder: settings.WorkspaceFolder,
                baseModelFileName: settings.BaseModelFileName
                );
            trainerDemo.TrainModel(@"C:\Users\User\Desktop\19.jpg");

            //trainerDemo.TrainModel();
            //trainerDemo.MakeSinglePrediction(@"C:\Users\User\Desktop\19.jpg");


        }
    }

    public class TrainingSettings
    {
        public string DataFolder { get; set; }
        public string ModelsFolder { get; set; }
        public string WorkspaceFolder { get; set; }
        public string BaseModelFileName { get; set; }
    }
}
