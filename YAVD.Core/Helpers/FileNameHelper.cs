using System.Text.RegularExpressions;
using YAVD.Core.Models;

namespace YAVD.Core.Helpers
{
    public static class FileNameHelper
    {
        public static string BuildFileName(string pattern, Dictionary<string, string> tags, string extension)
        {
            string fileName = pattern;

            foreach (var tag in tags)
            {
                fileName = fileName.Replace(tag.Key, tag.Value);
            }

            fileName = CleanFileName(fileName);

            int maxFileNameLength = 240 - extension.Length;
            if (fileName.Length > maxFileNameLength)
            {
                fileName = fileName.Substring(0, maxFileNameLength).Trim();
            }

            return fileName + extension;
        }
        public static string CleanFileName(string fileName)
        {
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]|[\.]$)", invalidChars);

            fileName = Regex.Replace(fileName, invalidRegStr, " ");

            return Regex.Replace(fileName, @"\s+", " ").Trim();
        }
        public static string GetUniqueFilePath(string folder, string fileNameWithExtension)
        {
            string fullPath = Path.Combine(folder, fileNameWithExtension);

            if (!File.Exists(fullPath)) return fullPath;

            string fileNameOnly = Path.GetFileNameWithoutExtension(fileNameWithExtension);
            string extension = Path.GetExtension(fileNameWithExtension);
            int counter = 2;

            while (File.Exists(fullPath))
            {
                string newName = $"{fileNameOnly} ({counter}){extension}";
                fullPath = Path.Combine(folder, newName);
                counter++;
            }
            return fullPath;
        }
    }
}