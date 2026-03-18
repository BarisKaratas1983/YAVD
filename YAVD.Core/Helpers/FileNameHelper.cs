using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAVD.Core.Helpers
{
    public static class FileNameHelper
    {
        public static string CleanFileName(string fileName)
        {            
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, ' ');
            }
            return fileName.Trim();
        }
    }
}
