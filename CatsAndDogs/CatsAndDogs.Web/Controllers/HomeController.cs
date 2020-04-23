using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CatsAndDogs.Web.Models;
using Microsoft.Extensions.ML;
using Microsoft.ML;
using CatsAndDogs.ML.DataModels;

namespace CatsAndDogs.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly PredictionService _predictionService;
        public HomeController(PredictionService predictionService)
        {
            _predictionService = predictionService;
        }
        public IActionResult Index()
        {
            return View();
        }        

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        
    }
}
