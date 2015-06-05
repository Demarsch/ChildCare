using System;
using GalaSoft.MvvmLight;
using System.Drawing;

namespace AdminTools.DTO
{
    public class ThumbnailDTO : ObservableObject
    {
        private Image imageSource;
        public Image ImageSource
        {
            get { return imageSource; }
            set { Set("ImageSource", ref imageSource, value); }
        }

        private string description;
        public string Description
        {
            get { return description; }
            set { Set("Description", ref description, value); }
        }

        
    }
}
