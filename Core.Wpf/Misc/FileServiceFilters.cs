using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Wpf.Misc
{
    public static class FileServiceFilters
    {
        public const string Default = "All files (*.*)|*.*|Office Files|*.doc;*.docx;*.xls;*.xlsx;*.ppt;*.pptx|Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF|Text files (*.txt)|*.txt";
        
        public const string DocX = "Документ Microsoft Word (*.docx)|*.docx";
        public const string DocXExtention = "docx";
    }
}
