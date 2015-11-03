﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Core.Data;
using Core.Data.Misc;
using Core.Extensions;
using Core.Misc;
using Core.Services;
using Core.Wpf.Events;
using Core.Wpf.Misc;
using Core.Wpf.Mvvm;
using log4net;
using PatientInfoModule.Misc;
using PatientInfoModule.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;

namespace PatientInfoModule.ViewModels
{
    public class PersonDocumentsViewModel : BindableBase, INavigationAware
    {
        private readonly IPatientService patientService;
        private readonly IDocumentService documentService;
        private readonly ILog log;
        private readonly ICacheService cacheService;
        private readonly IEventAggregator eventAggregator;
        public BusyMediator BusyMediator { get; set; }
        public CriticalFailureMediator CriticalFailureMediator { get; private set; }
        private readonly CommandWrapper reloadPatientDataCommandWrapper;
        private CancellationTokenSource currentLoadingToken;
        private int personId;

        public PersonDocumentsViewModel(IPatientService patientService, IDocumentService documentService, ILog log, ICacheService cacheService, IEventAggregator eventAggregator)
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
            if (eventAggregator == null)
            {
                throw new ArgumentNullException("eventAggregator");
            }
            this.patientService = patientService;
            this.documentService = documentService;
            this.log = log;
            this.cacheService = cacheService;
            this.eventAggregator = eventAggregator;
            personId = SpecialValues.NonExistingId;
            BusyMediator = new BusyMediator();
            CriticalFailureMediator = new CriticalFailureMediator();
            reloadPatientDataCommandWrapper = new CommandWrapper
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
            IDisposableQueryable<PersonOuterDocument> personOuterDocumentsQuery = null;
            try
            {
                AllDocuments.Clear();
                personOuterDocumentsQuery = patientService.GetPersonOuterDocuments(this.personId);
                var loadDocumentsTask = personOuterDocumentsQuery.ToArrayAsync(token);
                await Task.WhenAll(loadDocumentsTask, Task.Delay(AppConfiguration.PendingOperationDelay, token));
                var result = loadDocumentsTask.Result;
                AllDocuments.AddRange(result.Select(x => new ThumbnailViewModel()
                    {
                        DocumentId = x.DocumentId,
                        DocumentTypeId = x.OuterDocumentTypeId,
                        DocumentType = x.Document.FileName,
                        DocumentTypeParentName = x.OuterDocumentType.OuterDocumentType1.Name,
                        Comment = x.Document.Description,
                        DocumentDate = x.Document.DocumentFromDate,
                        ThumbnailImage = documentService.GetThumbnailForFile(x.Document.FileData, x.Document.Extension),
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
                CriticalFailureMediator.Activate("Не удалость загрузить документы пациента. Попробуйте еще раз или обратитесь в службу поддержки", reloadPatientDataCommandWrapper, ex);
                loadingIsCompleted = true;
            }
            finally
            {
                CommandManager.InvalidateRequerySuggested();
                if (loadingIsCompleted)
                {
                    BusyMediator.Deactivate();
                }
                if (personOuterDocumentsQuery != null)
                {
                    personOuterDocumentsQuery.Dispose();
                }
            }   
        }

        private readonly DelegateCommand scanningCommand;
        private readonly DelegateCommand addDocumentCommand;
        private readonly DelegateCommand removeDocumentCommand;
        private readonly DelegateCommand openDocumentCommand;

        public ICommand ScanningCommand { get { return scanningCommand; } }
        public ICommand AddDocumentCommand { get { return addDocumentCommand; } }
        public ICommand RemoveDocumentCommand { get { return removeDocumentCommand; } }
        public ICommand OpenDocumentCommand { get { return openDocumentCommand; } }   
        
        private void Scanning()
        {
        }

        private void AddDocument()
        {
        }

        private void RemoveDocument()
        {
            if (!allDocuments.Any() || allDocuments.All(x => !x.ThumbnailChecked))
            {
                //this.dialogService.ShowMessage("Отсутствуют документы для удаления");
                return;
            }

            //if (this.dialogService.AskUser("Удалить отмеченные документы ?", true) == true)
            //{
                foreach (var item in allDocuments.Where(x => x.ThumbnailChecked).ToList())
                {
                    patientService.DeletePersonOuterDocument(item.DocumentId);
                    documentService.DeleteDocumentById(item.DocumentId);
                    AllDocuments.Remove(item);
                }
            //}
        }

        private void OpenDocument()
        {
            var doc = documentService.GetDocumentById(SelectedDocument.DocumentId).First();
            documentService.RunFile(documentService.GetFileFromBinaryData(doc.FileData, doc.Extension));
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