using System;
using GalaSoft.MvvmLight;
using System.Windows.Media.Imaging;

namespace MainLib
{
    public class ThumbnailDTO : ObservableObject
    {
        private int documentId;
        public int DocumentId
        {
            get { return documentId; }
            set { Set("DocumentId", ref documentId, value); }
        }

        private BitmapImage thumbnailImage;
        public BitmapImage ThumbnailImage
        {
            get { return thumbnailImage; }
            set { Set("ThumbnailImage", ref thumbnailImage, value); }
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

        private string documentTypeParentName;
        public string DocumentTypeParentName
        {
            get { return documentTypeParentName; }
            set { Set("DocumentTypeParentName", ref documentTypeParentName, value); }
        }

        private string comment;
        public string Comment
        {
            get { return comment; }
            set { Set("Comment", ref comment, value); }
        }

        private DateTime? documentDate;
        public DateTime? DocumentDate
        {
            get { return documentDate; }
            set { Set("DocumentDate", ref documentDate, value); }
        }

        private bool thumbnailChecked;
        public bool ThumbnailChecked
        {
            get { return thumbnailChecked; }
            set { Set("ThumbnailChecked", ref thumbnailChecked, value); }
        }
                
        private bool thumbnailSaved;
        public bool ThumbnailSaved
        {
            get { return thumbnailSaved; }
            set { Set("ThumbnailSaved", ref thumbnailSaved, value); }
        }
    }
}
