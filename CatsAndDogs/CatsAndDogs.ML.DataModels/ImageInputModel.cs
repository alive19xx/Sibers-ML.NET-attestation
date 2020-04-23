using System;
using System.Collections.Generic;
using System.Text;

namespace CatsAndDogs.ML.DataModels
{
    public class ImageInputModel : BaseInputModel
    {  
        public byte[] Image { get; set; }
    }
}
