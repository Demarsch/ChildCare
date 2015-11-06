using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ReportingModule.Services
{
    public class ReportModuleFileOperations : IReportModuleFileOperations
    {
        public string CreateFileName(string folder, string title, string extention)
        {
            string filename = folder.Trim();
            
            if (filename.Length > 0)
                filename += Path.DirectorySeparatorChar;
            
            string name = title;
            Path.GetInvalidFileNameChars().Where(x => name.Contains(x)).ToList().ForEach(x => name = name.Replace(x.ToString(), string.Empty));
            name = name.Trim();
            if (name.Length > 128)
                filename += name.Substring(0, 128).Trim();
            else if (name.Length == 0)
                filename += "Документ";
            else
                filename += name;

            if (extention.Length > 0)
                return "." + extention;

            return filename;
        }

        public string CreateTempFolder(string prefix)
        {
            string PathName = Path.GetTempPath();
            if (prefix.Trim().Length > 0)
                PathName += prefix;
            else
                PathName += "Temp";
            int i = 1;
            while (Directory.Exists(PathName + i)) i++;
            PathName += i.ToString();
            Directory.CreateDirectory(PathName);
            return PathName;
        }

        public void StartDocument(string filename)
        {
            Process.Start(filename);
        }

        public void StartDocument(string filename, string verb)
        {
            ProcessStartInfo info = new ProcessStartInfo(filename)
            {
                Verb = verb,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            Process.Start(info);
        }
    }
}
