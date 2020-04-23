using System;

namespace CatsAndDogs.ML.DataModels
{
    public class ImageFileInputModel : BaseInputModel
    {
        public ImageFileInputModel(){}
        public ImageFileInputModel(string imagePath, string imageLabel)
        {
            ImagePath = imagePath;
            Label = imageLabel;
        }

        public string ImagePath { get; set; }
        
    }
}
