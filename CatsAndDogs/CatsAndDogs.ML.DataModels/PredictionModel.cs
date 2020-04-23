using System;
using System.Collections.Generic;
using System.Text;


namespace CatsAndDogs.ML.DataModels
{
    public class PredictionModel
    {
        public string PredictedLabel { get; set; }        
        public float[] Score { get; set; }
    }
}
