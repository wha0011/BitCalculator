using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace DevTools
{
    internal class Converter
    {
        public static void Convert(string imagename, string fileType)
        {
            ImageFormat imgformat = null;
            switch (fileType)
            {
                case ".ico":
                    imgformat = ImageFormat.Icon;
                    break;
                case ".png":
                    imgformat = ImageFormat.Png;
                    break;
                case ".jpg":
                    imgformat = ImageFormat.Jpeg;
                    break;
            }

            var b = Bitmap.FromFile(imagename);
            var finalname = imagename.Split('.')[0];
            b.Save(finalname + fileType, imgformat);
            return;
        }
    }
}
