using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CatsAndDogs.Web.Models
{
    public class PredictionViewModel
    {
        public string PredictedLabel { get; set; }
        public string Base64ImageContent { get; set; }
        public IEnumerable<ScoreInfoItem> Scores { get; set; }
    }
    public class ScoreInfoItem
    {
        public string Label { get; set; }
        public float Score { get; set; }
    }
}
