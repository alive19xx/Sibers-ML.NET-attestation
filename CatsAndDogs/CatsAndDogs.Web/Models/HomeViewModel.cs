using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace CatsAndDogs.Web.Models
{
    public class HomeViewModel
    {
        public IFormFile Image { get; set; }

        public string CurrentImageBase64Src { get; set; }
        public string CurrentImagePredictionErrorMessage { get; set; }
        public PredictionDto CurrentPrediction { get; set; }

    }
}
