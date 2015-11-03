using Core.Wpf.Events;
using Prism.Mvvm;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PatientInfoModule.ViewModels
{
    public class ThumbnailViewModel : BindableBase
    {
        public ThumbnailViewModel()
        {

        }

        private int documentId;
        public int DocumentId
        {
            get { return documentId; }
            set { SetProperty(ref documentId, value); }
        }

        private BitmapImage thumbnailImage;
        public BitmapImage ThumbnailImage
        {
            get { return thumbnailImage; }
            set { SetProperty(ref thumbnailImage, value); }
        }

        private string documentType;
        public string DocumentType
        {
            get { return documentType; }
            set { SetProperty(ref documentType, value); }
        }

        private int documentTypeId;
        public int DocumentTypeId
        {
            get { return documentTypeId; }
            set { SetProperty(ref documentTypeId, value); }
        }

        private string documentTypeParentName;
        public string DocumentTypeParentName
        {
            get { return documentTypeParentName; }
            set { SetProperty(ref documentTypeParentName, value); }
        }

        private string comment;
        public string Comment
        {
            get { return comment; }
            set { SetProperty(ref comment, value); }
        }

        private DateTime? documentDate;
        public DateTime? DocumentDate
        {
            get { return documentDate; }
            set { SetProperty(ref documentDate, value); }
        }

        private bool thumbnailChecked;
        public bool ThumbnailChecked
        {
            get { return thumbnailChecked; }
            set { SetProperty(ref thumbnailChecked, value); }
        }
                
        private bool thumbnailSaved;
        public bool ThumbnailSaved
        {
            get { return thumbnailSaved; }
            set { SetProperty(ref thumbnailSaved, value); }
        }
    }
}
