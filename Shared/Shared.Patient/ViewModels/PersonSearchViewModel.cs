using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Core.Data;
using Core.Data.Misc;
using Core.Extensions;
using Core.Misc;
using Core.Services;
using Core.Wpf.Misc;
using Core.Wpf.Mvvm;
using Core.Wpf.Services;
using log4net;
using Prism.Commands;
using Prism.Mvvm;
using Shared.Patient.Misc;
using Shared.Patient.Model;
using Shared.Patient.Services;

namespace Shared.Patient.ViewModels
{
    public class PersonSearchViewModel : BindableBase, IDisposable
    {
        private readonly IPersonSearchService personSearchService;

        private readonly ILog log;

        private readonly ICacheService cacheService;

        private readonly IUserInputNormalizer userInputNormalizer;

        private readonly IFileService fileService;

        public PersonSearchViewModel(IPersonSearchService personSearchService,
                                      ILog log,
                                      ICacheService cacheService,
                                      IUserInputNormalizer userInputNormalizer,
                                      IFileService fileService)
        {
            if (personSearchService == null)
            {
                throw new ArgumentNullException("personSearchService");
            }
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            if (userInputNormalizer == null)
            {
                throw new ArgumentNullException("userInputNormalizer");
            }
            if (fileService == null)
            {
                throw new ArgumentNullException("fileService");
            }
            this.userInputNormalizer = userInputNormalizer;
            this.fileService = fileService;
            this.cacheService = cacheService;
            this.userInputNormalizer = userInputNormalizer;
            this.log = log;
            this.personSearchService = personSearchService;
            BusyMediator = new BusyMediator();
            FailureMediator = new FailureMediator();
            Persons = new ObservableCollectionEx<FoundPersonViewModel>();
            SearchPersonsCommand = new DelegateCommand<bool?>(SearchPersons);
            searchPersonsCommandWrapper = new CommandWrapper
            {
                Command = SearchPersonsCommand,
                CommandName = "Повторный поиск",
                CommandParameter = false
            };
            SelectPersonCommand = new DelegateCommand<int?>(SelectPerson);
        }

        public ObservableCollectionEx<FoundPersonViewModel> Persons { get; private set; }

        private string searchText;

        public string SearchText
        {
            get { return searchText; }
            set
            {
                if (SetProperty(ref searchText, value))
                {
                    SearchPersons(true);
                }
            }
        }

        private CancellationTokenSource currentSearchToken;

        public BusyMediator BusyMediator { get; private set; }

        public FailureMediator FailureMediator { get; private set; }

        private readonly CommandWrapper searchPersonsCommandWrapper;

        private int selectedPersonId;

        public int SelectedPersonId
        {
            get { return selectedPersonId; }
            set { SetProperty(ref selectedPersonId, value); }
        }

        public ICommand SelectPersonCommand { get; private set; }

        private void SelectPerson(int? patientId)
        {
            SelectedPersonId = patientId.GetValueOrDefault(SpecialValues.NonExistingId);
        }

        public ICommand SearchPersonsCommand { get; private set; }

        private void SearchPersons(bool? useDelay)
        {
            if (currentSearchToken != null)
            {
                currentSearchToken.Cancel();
                currentSearchToken.Dispose();
            }
            currentSearchToken = new CancellationTokenSource();
            var userInput = userInputNormalizer.NormalizeUserInput(searchText ?? string.Empty);
            if (userInput.Length < AppConfiguration.UserInputSearchThreshold)
            {
                BusyMediator.Deactivate();
                Persons.Clear();
                return;
            }
            BusyMediator.Activate("Идет поиск пациентов...");
            if (useDelay == true)
            {
                Task.Delay(AppConfiguration.UserInputDelay, currentSearchToken.Token)
                    .ContinueWith(x => SearchPatientsAsync(userInput, currentSearchToken.Token),
                                  currentSearchToken.Token,
                                  TaskContinuationOptions.OnlyOnRanToCompletion,
                                  TaskScheduler.FromCurrentSynchronizationContext());
            }
            else
            {
                SearchPatientsAsync(userInput, currentSearchToken.Token);
            }
        }

        private async void SearchPatientsAsync(string userInput, CancellationToken token)
        {
            var searchIsCompleted = false;
            FailureMediator.Deactivate();
            log.InfoFormat("Searching patients by user input \"{0}\"", userInput);
            PersonSearchQuery query = null;
            try
            {
                query = personSearchService.GetPatientSearchQuery(userInput);
                var result = await query.PersonsQuery
                                        .Select(x => new
                                        {
                                            x.Id,
                                            x.BirthDate,
                                            x.IsMale,
                                            x.Snils,
                                            x.MedNumber,
                                            CurrentName = x.PersonNames.FirstOrDefault(y => y.EndDateTime == null || y.EndDateTime == DateTime.MaxValue),
                                            PreviousName = x.PersonNames.Where(y => y.EndDateTime != null && y.EndDateTime != DateTime.MaxValue)
                                                            .OrderByDescending(y => y.BeginDateTime)
                                                            .FirstOrDefault(),
                                            IdentityDocument = x.PersonIdentityDocuments
                                                                .OrderByDescending(y => y.BeginDate)
                                                                .FirstOrDefault(),
                                            Photo = x.Document.FileData

                                        })
                                        .ToArrayAsync(token);
                var patients = await result.Select(x => new FoundPersonViewModel
                {
                    Id = x.Id,
                    BirthDate = x.BirthDate,
                    CurrentName = x.CurrentName,
                    PreviousName = x.PreviousName,
                    IsMale = x.IsMale,
                    IdentityDocument = cacheService.AutoWire(x.IdentityDocument, y => y.IdentityDocumentType),
                    MedNumber = x.MedNumber,
                    Snils = Person.DelimitizeSnils(x.Snils),
                    Photo = fileService.GetImageSourceFromBinaryData(x.Photo)
                })
                                           .ToArrayAsync(token);
                log.InfoFormat("Found {0} patients", patients.Length);
                foreach (var patient in patients)
                {
                    patient.WordsToHighlight = query.ParsedTokens;
                }
                Persons.Replace(patients);
                searchIsCompleted = true;
            }
            catch (OperationCanceledException)
            {
                //Do nothing. Cancelled operation means that user started another search before completing the previous one
            }
            catch (Exception ex)
            {
                log.Error("Failed to find patients", ex);
                searchIsCompleted = true;
                FailureMediator.Activate("Не удалось загрузить пациентов. Попробуйте еще раз или перезапустите приложение", searchPersonsCommandWrapper, ex);
            }
            finally
            {
                if (query != null)
                {
                    query.Dispose();
                }
                if (searchIsCompleted)
                {
                    BusyMediator.Deactivate();
                }
            }
        }

        public void Dispose()
        {
            searchPersonsCommandWrapper.Dispose();
        }
    }
}