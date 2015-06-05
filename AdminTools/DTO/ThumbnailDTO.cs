using System;
using GalaSoft.MvvmLight;
using System.Drawing;
using DataLib;

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

        private string documentType;
        public string DocumentType
        {
            get { return documentType; }
            set { Set("DocumentType", ref documentType, value); }
        }

        private int documentTypeId;
        public int DocumentTypeId
        {
            get { return documentTypeId; }
            set { Set("DocumentTypeId", ref documentTypeId, value); }

        }
        private string comment;
        public string Comment
        {
            get { return comment; }
            set { Set("Comment", ref comment, value); }
        }

        private bool thumbnailChecked;
        public bool ThumbnailChecked
        {
            get { return thumbnailChecked; }
            set { Set("ThumbnailChecked", ref thumbnailChecked, value); }
        }
    }
}
