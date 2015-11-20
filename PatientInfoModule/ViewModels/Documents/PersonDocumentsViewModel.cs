using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using Core.Data.Misc;
using Core.Extensions;
using Core.Services;
using Core.Wpf.Misc;
using Core.Wpf.Mvvm;
using log4net;
using PatientInfoModule.Misc;
using PatientInfoModule.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Interactivity.InteractionRequest;
using PatientInfoModule.Views;
using Core.Data;
using System.Collections.Generic;
using Core.Wpf.Services;

namespace PatientInfoModule.ViewModels
{
    public class PersonDocumentsViewModel : BindableBase, INavigationAware
    {
        private readonly IPatientService patientService;
        private readonly IDocumentService documentService;
        private readonly ILog log;
        private readonly ICacheService cacheService;
        private readonly IFileService fileService;
        private readonly IEventAggregator eventAggregator;
        private readonly CommandWrapper reloadDocumentsCommandWrapper;
        private CancellationTokenSource currentLoadingToken;
        public FailureMediator FailureMediator { get; private set; }
        public BusyMediator BusyMediator { get; set; }
        public InteractionRequest<Confirmation> ConfirmationInteractionRequest { get; private set; }
        public InteractionRequest<Notification> NotificationInteractionRequest { get; private set; }
        public InteractionRequest<SelectPersonDocumentTypeViewModel> SelectPersonDocumentTypeInteractionRequest { get; private set; }
        private readonly Func<SelectPersonDocumentTypeViewModel> selectPersonDocumentTypeViewModelFactory;
        private int personId;

        public PersonDocumentsViewModel(IPatientService patientService, IDocumentService documentService, ILog log, ICacheService cacheService, IFileService fileService,
                                        IEventAggregator eventAggregator, Func<SelectPersonDocumentTypeViewModel> selectPersonDocumentTypeViewModelFactory)
        {
            if (patientService == null)
            {
                throw new ArgumentNullException("patientService");
            }
            if (documentService == null)
            {
                throw new ArgumentNullException("documentService");
            }
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            if (fileService == null)
            {
                throw new ArgumentNullException("fileService");
            }
            if (eventAggregator == null)
            {
                throw new ArgumentNullException("eventAggregator");
            }
            this.patientService = patientService;
            this.documentService = documentService;
            this.log = log;
            this.cacheService = cacheService;
            this.fileService = fileService;
            this.eventAggregator = eventAggregator;
            this.selectPersonDocumentTypeViewModelFactory = selectPersonDocumentTypeViewModelFactory;
            personId = SpecialValues.NonExistingId;
            BusyMediator = new BusyMediator();
            FailureMediator = new FailureMediator();
            ConfirmationInteractionRequest = new InteractionRequest<Confirmation>();
            NotificationInteractionRequest = new InteractionRequest<Notification>();
            SelectPersonDocumentTypeInteractionRequest = new InteractionRequest<SelectPersonDocumentTypeViewModel>();
            reloadDocumentsCommandWrapper = new CommandWrapper
                                              {
                                                  Command = new DelegateCommand(() => LoadPersonDocumentsAsync(personId)),
                                                  CommandName = "Повторить",
                                              };
            scanningCommand = new DelegateCommand(Scanning);
            addDocumentCommand = new DelegateCommand(AddDocument);
            removeDocumentCommand = new DelegateCommand(RemoveDocument);
            openDocumentCommand = new DelegateCommand(OpenDocument);
            AllDocuments = new ObservableCollectionEx<ThumbnailViewModel>();
        }

        public async void LoadPersonDocumentsAsync(int personId)
        {
            this.personId = personId;
            if (personId == SpecialValues.NewId || personId == SpecialValues.NonExistingId)
            {
                return;
            }
            if (currentLoadingToken != null)
            {
                currentLoadingToken.Cancel();
                currentLoadingToken.Dispose();
            }
            var loadingIsCompleted = false;
            currentLoadingToken = new CancellationTokenSource();
            var token = currentLoadingToken.Token;
            BusyMediator.Activate("Загрузка документов пациента...");
            log.InfoFormat("Loading documents for patient with Id {0}...", personId);
            try
            {
                AllDocuments.Clear();
                var result = await patientService.GetPersonOuterDocuments(this.personId).ToArrayAsync(token);
                AllDocuments.AddRange(result.Select(x => new ThumbnailViewModel
                                                         {
                                                             DocumentId = x.DocumentId,
                                                             DocumentTypeId = x.OuterDocumentTypeId,
                                                             DocumentType = x.Document.FileName,
                                                             DocumentTypeParentName = x.OuterDocumentType.OuterDocumentType1.Name,
                                                             Comment = x.Document.Description,
                                                             DocumentDate = x.Document.DocumentFromDate,
                                                             ThumbnailImage = fileService.GetThumbnailForFile(x.Document.FileData, x.Document.Extension),
                                                             ThumbnailChecked = false
                                                         }));
                loadingIsCompleted = true;
            }
            catch (OperationCanceledException)
            {
                //Do nothing. Cancelled operation means that user selected different patient before previous one was loaded
            }
            catch (Exception ex)
            {
                log.ErrorFormatEx(ex, "Failed to load documents for patient with Id {0}", personId);
                FailureMediator.Activate("Не удалость загрузить документы пациента. Попробуйте еще раз или обратитесь в службу поддержки", reloadDocumentsCommandWrapper, ex);
                loadingIsCompleted = true;
            }
            finally
            {
                CommandManager.InvalidateRequerySuggested();
                if (loadingIsCompleted)
                    BusyMediator.Deactivate();
                if (patientService.GetPersonOuterDocuments(this.personId) != null)
                    patientService.GetPersonOuterDocuments(this.personId).Dispose();
            }
        }

        private readonly DelegateCommand scanningCommand;

        private readonly DelegateCommand addDocumentCommand;

        private readonly DelegateCommand removeDocumentCommand;

        private readonly DelegateCommand openDocumentCommand;

        public ICommand ScanningCommand
        {
            get { return scanningCommand; }
        }

        public ICommand AddDocumentCommand
        {
            get { return addDocumentCommand; }
        }

        public ICommand RemoveDocumentCommand
        {
            get { return removeDocumentCommand; }
        }

        public ICommand OpenDocumentCommand
        {
            get { return openDocumentCommand; }
        }

        private void Scanning()
        {
            if (this.personId == SpecialValues.NewId || this.personId == SpecialValues.NonExistingId)
            {
                NotificationInteractionRequest.Raise(new Notification()
                {
                    Title = "Внимание",
                    Content = "Не выбран пациент."
                });
                return;
            };
            var scanDocumentViewModel = new ScanDocumentsViewModel(this.documentService, fileService, this.log);
            (new ScanDocumentsView() { DataContext = scanDocumentViewModel }).ShowDialog();
            
            foreach (var item in scanDocumentViewModel.PreviewImages.Where(x => x.ThumbnailSaved))
            {
                var doc = documentService.GetDocumentById(item.DocumentId).First();

                PersonOuterDocument personOuterDocument = new PersonOuterDocument();
                personOuterDocument.PersonId = this.personId;
                personOuterDocument.DocumentId = item.DocumentId;
                personOuterDocument.OuterDocumentTypeId = item.DocumentTypeId;

                if (patientService.SavePersonDocument(personOuterDocument))
                {
                    AllDocuments.Add(new ThumbnailViewModel()
                    {
                        DocumentId = item.DocumentId,
                        DocumentTypeId = item.DocumentTypeId,
                        DocumentType = doc.FileName,
                        DocumentTypeParentName = doc.PersonOuterDocuments.First().OuterDocumentType.OuterDocumentType1.Name,
                        Comment = doc.Description,
                        DocumentDate = doc.DocumentFromDate,
                        ThumbnailImage = fileService.GetThumbnailForFile(doc.FileData, doc.Extension),
                        ThumbnailChecked = false
                    });
                }
            }
            
        }

        private string[] files;

        private void AddDocument()
        {
            if (this.personId == SpecialValues.NewId || this.personId == SpecialValues.NonExistingId)
            {
                NotificationInteractionRequest.Raise(new Notification()
                {
                    Title = "Внимание",
                    Content = "Не выбран пациент."
                });
                return;
            };
            files = new string[0];
            files = fileService.OpenFileDialog();
            if (files.Any())
            {
                var selectPersonDocumentTypeViewModel = selectPersonDocumentTypeViewModelFactory();
                selectPersonDocumentTypeViewModel.IntializeCreation("Информация о документе");
                SelectPersonDocumentTypeInteractionRequest.Raise(selectPersonDocumentTypeViewModel, OnAddDocumentDialogClosed);
            }
        }

        private async void OnAddDocumentDialogClosed(SelectPersonDocumentTypeViewModel viewModel)
        {
            if (viewModel.SelectedDocumentType == null) return;
            int documentTypeId = viewModel.SelectedDocumentType.Id;
            Document document = new Document();
            document.FileName = documentService.GetOuterDocumentTypeById(documentTypeId).First().Name;
            document.DocumentFromDate = viewModel.SelectedDocumentDate;
            document.Description = viewModel.Description;
            document.DisplayName = document.FileName + (document.DocumentFromDate.HasValue ? " от " + document.DocumentFromDate.Value.ToShortDateString() : string.Empty);

            document.Extension = files[0].Substring(files[0].LastIndexOf('.') + 1);
            document.FileData = fileService.GetBinaryDataFromFile(files[0]);
            document.FileSize = document.FileData.Length;
            document.UploadDate = DateTime.Now;

            int documentId = await documentService.UploadDocument(document);
            if (documentId != SpecialValues.NewId)
            {
                PersonOuterDocument personOuterDocument = new PersonOuterDocument();
                personOuterDocument.PersonId = this.personId;
                personOuterDocument.DocumentId = documentId;
                personOuterDocument.OuterDocumentTypeId = documentTypeId;

                if (patientService.SavePersonDocument(personOuterDocument))
                {
                    AllDocuments.Add(new ThumbnailViewModel()
                    {
                        DocumentId = documentId,
                        DocumentTypeId = documentTypeId,
                        DocumentType = document.FileName,
                        DocumentTypeParentName = documentService.GetDocumentById(documentId).First().PersonOuterDocuments.First().OuterDocumentType.OuterDocumentType1.Name,
                        Comment = document.Description,
                        DocumentDate = document.DocumentFromDate,
                        ThumbnailImage = fileService.GetThumbnailForFile(document.FileData, document.Extension),
                        ThumbnailChecked = false
                    });
                }
            }                        
        }

        private void RemoveDocument()
        {
            if (!allDocuments.Any() || allDocuments.All(x => !x.ThumbnailChecked))
            {
                NotificationInteractionRequest.Raise(new Notification()
                {
                    Title = "Внимание",
                    Content = "Отсутствуют документы для удаления."
                });
                return;
            }
            ConfirmationInteractionRequest.Raise(new Confirmation()
            {
                Title = "Внимание",
                Content = "Удалить отмеченные документы ?"
            }, OnDialogClosed);
        }

        private void OnDialogClosed(Confirmation confirmation)
        {
            if (confirmation.Confirmed)
            {
                foreach (var item in allDocuments.Where(x => x.ThumbnailChecked).ToList())
                {
                    patientService.DeletePersonOuterDocument(item.DocumentId);
                    AllDocuments.Remove(item);
                }
            }            
        }

        private void OpenDocument()
        {
            fileService.RunFile(documentService.GetDocumentFile(SelectedDocument.DocumentId));
        }

        private ObservableCollectionEx<ThumbnailViewModel> allDocuments;

        public ObservableCollectionEx<ThumbnailViewModel> AllDocuments
        {
            get { return allDocuments; }
            set { SetProperty(ref allDocuments, value); }
        }

        private ThumbnailViewModel selectedDocument;

        public ThumbnailViewModel SelectedDocument
        {
            get { return selectedDocument; }
            set { SetProperty(ref selectedDocument, value); }
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var targetPersonId = (int?)navigationContext.Parameters[ParameterNames.PatientId] ?? SpecialValues.NonExistingId;
            if (targetPersonId != personId)
            {
                LoadPersonDocumentsAsync(targetPersonId);
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            //We use only one view-model for patient info, thus we says that current view-model can accept navigation requests
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            //TODO: place here logic for current view being deactivated
        }
    }
}