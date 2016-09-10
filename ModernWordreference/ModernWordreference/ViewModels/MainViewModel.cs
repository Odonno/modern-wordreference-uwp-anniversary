using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using Microsoft.Services.Store.Engagement;
using ModernWordreference.Constants;
using ModernWordreference.Messages;
using ModernWordreference.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.Networking.Connectivity;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace ModernWordreference.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region Fields

        private bool _clickOnCard = false;

        private readonly ResourceLoader _resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        private INavigationService _navigationService;
        private IApiService _apiService;
        private IDictionaryService _dictionaryService;
        private IRoamingStorageService _storageService;
        private IAnalyticsService _analyticsService;
        private INetworkService _networkService;

        #endregion

        #region Properties

        private Models.TranslationResult _currentTranslation;
        public Models.TranslationResult CurrentTranslation
        {
            get { return _currentTranslation; }
            set
            {
                _currentTranslation = value; RaisePropertyChanged();
                RaisePropertyChanged(nameof(CanPlayAudio));
                RaisePropertyChanged(nameof(CurrentAudioSource));
            }
        }

        public ObservableCollection<Models.TranslationResult> AllTranslations { get; } = new ObservableCollection<Models.TranslationResult>();

        public ObservableCollection<object> TranslationsCardItems { get; private set; } = new ObservableCollection<object>();

        public IEnumerable<IGrouping<string, Models.TranslationLine>> Source { get; private set; }

        public bool CanPlayAudio { get { return CurrentTranslation?.AudioSources.Count > 0; } }

        public string CurrentAudioSource
        {
            get
            {
                if (CurrentTranslation?.AudioSources.Count > 0)
                    return CurrentTranslation.AudioSources.First();

                return "ms-appx:///"; // TODO : better solution ?
            }
        }

        public bool ShowNoPreviousTranslationGrid
        {
            get { return AllTranslations.Count == 0; }
        }

        public bool ShowWithPreviousTranslationGrid
        {
            get { return AllTranslations.Count > 0; }
        }

        public bool HasNetwork
        {
            get { return _networkService.IsInternetAvailable; }
        }

        private bool _showNewTranslationControl;
        public bool ShowNewTranslationControl
        {
            get { return _showNewTranslationControl; }
            private set { _showNewTranslationControl = value; RaisePropertyChanged(); }
        }

        #endregion

        #region Constructor

        public MainViewModel(INavigationService navigationService, IApiService apiService, IDictionaryService dictionaryService, IRoamingStorageService storageService, IAnalyticsService analyticsService, INetworkService networkService)
        {
            _navigationService = navigationService;
            _apiService = apiService;
            _dictionaryService = dictionaryService;
            _storageService = storageService;
            _analyticsService = analyticsService;
            _networkService = networkService;

            InitializeAsync();

            Messenger.Default.Register<NewTranslationMessage>(this, async (message) =>
            {
                await AddTranslationAsync(message.Translation);
            });
        }

        #endregion

        #region Private Methods

        private async void InitializeAsync()
        {
            // Handle network connection
            RaisePropertyChanged(nameof(HasNetwork));
            NetworkInformation.NetworkStatusChanged += async _ =>
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher
                    .RunAsync(CoreDispatcherPriority.High, () =>
                    {
                        RaisePropertyChanged(nameof(HasNetwork));
                    });
            };

            // Create TranslationsCardItems list
            TranslationsCardItems.Add("New Translation");

            // Retrieve list of translation summaries
            var translationSummaries = _storageService.Retrieve<List<Models.TranslationSummary>>(StorageConstants.TranslationSummaries);
            if (translationSummaries != null)
            {
                var lastTranslations = translationSummaries
                    .OrderByDescending(ts => ts.SearchedDate)
                    .Take(AppConstants.MaxSavedTranslations)
                    .Reverse();

                foreach (var translationSummary in lastTranslations)
                {
                    var translation = await _storageService.RetrieveFileAsync<Models.TranslationResult>(translationSummary.Filename);

                    AllTranslations.Add(translation);
                    TranslationsCardItems.Insert(1, translation);
                }

                CurrentTranslation = AllTranslations.Last();
                UpdateSource();
            }
        }

        private void UpdateSource()
        {
            // Set group key in each translation line
            var primaryTranslationsGroup = CurrentTranslation.PrimaryTranslations
                .GroupBy(_ => _resourceLoader.GetString("PrimaryTranslations"));
            var additionalTranslationsGroup = CurrentTranslation.AdditionalTranslations
                .GroupBy(_ => _resourceLoader.GetString("AdditionalTranslations"));
            var compoundFormsTranslationsGroup = CurrentTranslation.CompoundForms
                .GroupBy(_ => _resourceLoader.GetString("CompoundForms"));

            // Fill collection view source with data
            Source = primaryTranslationsGroup
                .Concat(additionalTranslationsGroup)
                .Concat(compoundFormsTranslationsGroup);

            RaisePropertyChanged(nameof(ShowNoPreviousTranslationGrid));
            RaisePropertyChanged(nameof(ShowWithPreviousTranslationGrid));
            RaisePropertyChanged(nameof(Source));
        }

        private void LoadExistingTranslation(Models.TranslationResult translation)
        {
            CurrentTranslation = translation;
            UpdateSource();
        }

        private async Task AddTranslationAsync(Models.TranslationResult translation)
        {
            CurrentTranslation = translation;
            TranslationsCardItems.Insert(1, translation);
            AllTranslations.Add(translation);
            UpdateSource();
            ShowNewTranslationControl = false;

            // Save translation in the file
            await _storageService.SaveFileAsync(translation.Filename, translation);

            // Add the new translation in the summaries list
            var translationSummaries = _storageService.Retrieve<List<Models.TranslationSummary>>(StorageConstants.TranslationSummaries);
            if (translationSummaries == null)
            {
                translationSummaries = new List<Models.TranslationSummary>();
            }
            translationSummaries.Add(new Models.TranslationSummary
            {
                SearchedDate = DateTime.Now,
                Filename = translation.Filename
            });
            _storageService.Save(StorageConstants.TranslationSummaries, translationSummaries);

            // Remove translations if it exceed size of the card list
            while (TranslationsCardItems.Count - 1 > AppConstants.MaxSavedTranslations)
            {
                TranslationsCardItems.RemoveAt(TranslationsCardItems.Count - 1);
            }
        }

        #endregion

        #region Methods

        public void GoToAboutPage()
        {
            _navigationService.NavigateTo("About");
        }

        public async Task GoToFeedbackPageAsync()
        {
            // Send telemetry
            _analyticsService.TrackEvent("OpenFeedbackApp");

            // Open feedback hub app
            bool executed = await StoreServicesFeedbackLauncher.GetDefault().LaunchAsync();

            if (executed)
            {
                // Feedback app opened
            }
        }

        public void GoToLovePage()
        {
            // Send telemetry
            _analyticsService.TrackEvent("OpenLoveModal");

            _navigationService.NavigateTo("Love");
        }

        public void GoToSettingsPage()
        {
            _navigationService.NavigateTo("Settings");
        }

        public async void ClickOnCard(object sender, ItemClickEventArgs e)
        {
            var translation = e.ClickedItem as Models.TranslationResult;
            if (translation != null)
            {
                LoadExistingTranslation(translation);
            }
            else
            {
                await StartNewTranslationAsync();
            }
        }

        public async Task StartNewTranslationAsync()
        {
            _clickOnCard = true;

            ShowNewTranslationControl = true;
            RaisePropertyChanged(nameof(ShowNewTranslationControl));

            await Task.Delay(50);
            _clickOnCard = false;
            Messenger.Default.Send<ShowNewTranslationControlMessage>(new ShowNewTranslationControlMessage());
        }

        public void TapOnPage()
        {
            if (!_clickOnCard)
                ShowNewTranslationControl = false;
        }

        #endregion
    }
}
