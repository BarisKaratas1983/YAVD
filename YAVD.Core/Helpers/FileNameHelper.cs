using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAVD.Core.Helpers
{
    public class FileNameHelper
    {
        public static string CleanFileName(string fileName)
        {
            // Geçersiz karakterleri boşlukla değiştir
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, ' ');
            }
            return fileName.Trim();
        }
    }
}
