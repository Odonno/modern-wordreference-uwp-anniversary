using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Views;
using ModernWordreference.Constants;
using ModernWordreference.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace ModernWordreference.ViewModels
{
    public class SelectDictionaryViewModel : ViewModelBase
    {
        #region Fields

        private bool _initializing;

        private readonly ResourceLoader _resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        private IDictionaryService _dictionaryService;
        private IRoamingStorageService _storageService;
        private INavigationService _navigationService;
        private IAnalyticsService _analyticsService;
        private IReactiveService _reactiveService;

        #endregion

        #region Properties

        public IEnumerable<Models.Dictionary> AllDictionaries { get; private set; }
        public IEnumerable<Models.Dictionary> RecommendedDictionaries { get; private set; }
        public IEnumerable<Models.Dictionary> DefaultDictionaries { get; private set; }

        public IEnumerable<IGrouping<string, Models.Dictionary>> Source { get; private set; }

        private Models.Dictionary _selectedDictionary;
        public Models.Dictionary SelectedDictionary
        {
            get { return _selectedDictionary; }
            set { _selectedDictionary = value; SelectDictionary(); }
        }

        private string _search;
        public string Search
        {
            get { return _search; }
            set { _search = value; UpdateUI(); RaisePropertyChanged(); }
        }

        #endregion

        #region Constructor

        public SelectDictionaryViewModel(IDictionaryService dictionaryService, IRoamingStorageService storageService, INavigationService navigationService, IAnalyticsService analyticsService, IReactiveService reactiveService)
        {
            _dictionaryService = dictionaryService;
            _storageService = storageService;
            _navigationService = navigationService;
            _analyticsService = analyticsService;
            _reactiveService = reactiveService;

            Initialize();
        }

        #endregion

        #region Private Methods

        private void Initialize()
        {
            _initializing = true;

            // Set dictionary list (all)
            AllDictionaries = _dictionaryService.GetAll();

            // Set dictionary list (recommended)
            RecommendedDictionaries = new List<Models.Dictionary>
            {
                _dictionaryService.Get("en", "fr"),
                _dictionaryService.Get("fr", "en")
            };

            // Set dictionary list (default)
            DefaultDictionaries = _dictionaryService.GetAll()
                .Except(RecommendedDictionaries);

            // Refresh UI
            UpdateUI();

            _initializing = false;
        }

        private void UpdateUI()
        {
            Func<Models.Dictionary, string, bool> dictionaryContainsFromLanguage =
                (Models.Dictionary d, string s) => d.FromLanguage.ToLower().Contains(s.ToLower());

            Func<Models.Dictionary, string, bool> dictionaryContainsToLanguage =
                (Models.Dictionary d, string s) => d.ToLanguage.ToLower().Contains(s.ToLower());

            Func<IEnumerable<IGrouping<string, Models.Dictionary>>, Models.Dictionary, bool> groupContainsDictionary =
                (IEnumerable<IGrouping<string, Models.Dictionary>> groupList, Models.Dictionary d) => groupList.Any(g => g.Any(d2 => d == d2));

            // Set group key in each dictionary
            var searchGroupList = AllDictionaries
                .Where(dictionary => !string.IsNullOrWhiteSpace(Search) &&
                                     (dictionaryContainsFromLanguage(dictionary, Search) ||
                                     dictionaryContainsToLanguage(dictionary, Search)))
                .GroupBy(_ => _resourceLoader.GetString("SearchedDictionaries"));

            var recommendGroupList = RecommendedDictionaries
                .Where(dictionary => !groupContainsDictionary(searchGroupList, dictionary))
                .GroupBy(_ => _resourceLoader.GetString("Recommended"));

            var defaultGroupList = DefaultDictionaries
                .Where(dictionary => !groupContainsDictionary(searchGroupList, dictionary))
                .GroupBy(_ => _resourceLoader.GetString("AllDictionaries"));

            // Fill collection view source with data
            Source = searchGroupList
                .Concat(recommendGroupList)
                .Concat(defaultGroupList);

            RaisePropertyChanged("Source");

            // Retrieve currently selected dictionary
            var savedDictionary = _storageService.Retrieve<Models.Dictionary>(StorageConstants.CurrentDictionary);
            if (savedDictionary != null)
            {
                SelectedDictionary = _dictionaryService.Get(savedDictionary.From, savedDictionary.To);
            }
            else
            {
                SelectedDictionary = _dictionaryService.Get("en", "fr");
            }
        }

        private void SelectDictionary()
        {
            if (_initializing)
                return;

            var savedDictionary = _storageService.Retrieve<Models.Dictionary>(StorageConstants.CurrentDictionary);

            // Check if we have really change of dictionary
            if (savedDictionary != null && (SelectedDictionary.From != savedDictionary.From || SelectedDictionary.To != savedDictionary.To))
            {
                // Save selection
                _storageService.Save(StorageConstants.CurrentDictionary, SelectedDictionary);

                // Send telemetry
                _analyticsService.TrackEvent("SelectDictionary", new Dictionary<string, string>
                {
                    { "From", SelectedDictionary.From },
                    { "To", SelectedDictionary.To }
                });

                // Send result to other ViewModels
                _reactiveService.SelectDictionaryDone.OnNext(SelectedDictionary);

                // Go back
                _navigationService.GoBack();
            }
        }

        #endregion
    }
}
