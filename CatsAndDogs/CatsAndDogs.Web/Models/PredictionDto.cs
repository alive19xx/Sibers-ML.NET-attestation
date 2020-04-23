using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CatsAndDogs.Web.Models
{
    public class PredictionDto
    {
        public string PredictedLabel { get; set; }
        public IDictionary<string,float> CategoryScores { get; set; }        
    }


}
