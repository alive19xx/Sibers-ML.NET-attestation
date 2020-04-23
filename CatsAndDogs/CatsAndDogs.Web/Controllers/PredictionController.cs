using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CatsAndDogs.ML.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace CatsAndDogs.Web.Controllers
{
    public class PredictionController : Controller
    {
        private readonly PredictionService _predictionSerivce;

        public PredictionController(PredictionService predictionService)
        {
            _predictionSerivce = predictionService;

        }

        [HttpPost("prediction")]
        [Consumes("multipart/form-data")]
        public IActionResult Predict([FromForm] IFormFile image)
        {
            string mime;
            byte[] array;

            if (!Helpers.GetImageMIMEType(image.FileName, out mime))
                return BadRequest("File format is not supported");
            
            using (var memoryStream = new System.IO.MemoryStream())
            {
                image.CopyTo(memoryStream);
                array = memoryStream.ToArray();
            }
            return ReturnPrediction(array, mime);
        }

        private IActionResult ReturnPrediction(byte[] bytes, string mimeType)
        {            
            var prediction = _predictionSerivce.GetPrediction(bytes);
            var vm = new Models.PredictionViewModel
            {
                PredictedLabel = prediction.PredictedLabel,
                Base64ImageContent = $"data:{mimeType};base64, {Convert.ToBase64String(bytes)}",
                Scores = prediction.CategoryScores.Select(x=>new Models.ScoreInfoItem { Score=x.Value,Label=x.Key }).ToArray()
            };
            return Ok(vm);
        }
    }
}