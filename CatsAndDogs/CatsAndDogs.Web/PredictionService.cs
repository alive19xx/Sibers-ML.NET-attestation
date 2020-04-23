using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.ML;
using Microsoft.ML.Data;
using Microsoft.ML;
using CatsAndDogs.ML.DataModels;

namespace CatsAndDogs.Web
{
    public class PredictionService
    {
        private readonly PredictionEnginePool<ImageInputModel, PredictionModel> _predictionEnginePool;
        public PredictionService(PredictionEnginePool<ImageInputModel, PredictionModel> predictionEnginePool)
        {
            _predictionEnginePool = predictionEnginePool;
        }

        public Models.PredictionDto GetPrediction(byte[] bytes)
        {
            var imageData = new ImageInputModel()
            {
                Image = bytes
            };
            var predictionEngine = _predictionEnginePool.GetPredictionEngine();
            var prediction = predictionEngine.Predict(imageData);
            var scores = GetSortedLabelValues(predictionEngine.OutputSchema, "Score", prediction.Score);
            var result = new Models.PredictionDto
            {
                CategoryScores = scores,
                PredictedLabel = prediction.PredictedLabel,
            };
            return result;
        }

        private static IDictionary<string, float> GetSortedLabelValues(DataViewSchema schema, string columnName, float[] columnValues)
        {
            var result = new Dictionary<string, float>();
            var column = schema.GetColumnOrNull(columnName);

            var slotNames = new VBuffer<ReadOnlyMemory<char>>();
            column.Value.GetSlotNames(ref slotNames);

            var num = 0;
            foreach (var denseValue in slotNames.DenseValues())
            {
                result.Add(denseValue.ToString(), columnValues[num++]);
            }

            return result.OrderByDescending(c => c.Value).ToDictionary(i => i.Key, i => i.Value);
        }
    }
}
