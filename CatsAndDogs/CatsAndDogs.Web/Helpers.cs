using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.StaticFiles;

namespace CatsAndDogs.Web
{
    public class Helpers
    {
        static string[] _allowedExtensions = new string[] { ".png", ".jpg", ".bmp" };
        
        public static bool GetImageMIMEType(string fileName, out string contentType)
        {
            contentType = null;
            if (!_allowedExtensions.Contains(System.IO.Path.GetExtension(fileName)))
                return false;
            return new FileExtensionContentTypeProvider().TryGetContentType(fileName, out contentType);
        }
    }
}
