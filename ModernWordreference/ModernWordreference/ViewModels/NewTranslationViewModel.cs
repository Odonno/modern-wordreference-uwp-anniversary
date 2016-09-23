using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using ModernWordreference.Constants;
using ModernWordreference.Messages;
using ModernWordreference.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;

namespace ModernWordreference.ViewModels
{
    public class NewTranslationViewModel : ViewModelBase
    {
        #region Fields

        private readonly ResourceLoader _resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        private INavigationService _navigationService;
        private IApiService _apiService;
        private IDictionaryService _dictionaryService;
        private IRoamingStorageService _roamingStorageService;
        private ILocalStorageService _localStorageService;
        private IToastNotificationService _toastNotificationService;
        private IAnalyticsService _analyticsService;

        #endregion

        #region Properties

        private Models.Dictionary _currentDictionary;
        public Models.Dictionary CurrentDictionary
        {
            get { return _currentDictionary; }
            set
            {
                _currentDictionary = value; RaisePropertyChanged();
            }
        }

        public bool NeedSpecificKeyboard
        {
            get
            {
                return (CurrentDictionary.From == "ru" ||
                    CurrentDictionary.From == "gr" ||
                    CurrentDictionary.From == "zh" ||
                    CurrentDictionary.From == "ja" ||
                    CurrentDictionary.From == "ko" ||
                    CurrentDictionary.From == "ar");
            }
        }

        private Models.TranslationResult _lastTranslation;
        public Models.TranslationResult LastTranslation
        {
            get { return _lastTranslation; }
            set
            {
                _lastTranslation = value; RaisePropertyChanged();
            }
        }

        private string _word;
        public string Word
        {
            get { return _word; }
            set
            {
                _word = value; RaisePropertyChanged();
                RaisePropertyChanged(nameof(CanExecuteSearch));
            }
        }

        public bool CanExecuteSearch { get { return (!string.IsNullOrWhiteSpace(Word) && Word.Length > 1); } }

        public ObservableCollection<Models.Suggestion> Suggestions { get; private set; } = new ObservableCollection<Models.Suggestion>();

        #endregion

        #region Constructor

        public NewTranslationViewModel(INavigationService navigationService, IApiService apiService, IDictionaryService dictionaryService, IRoamingStorageService roamingStorageService, ILocalStorageService localStorageService, IToastNotificationService toastNotificationService, IAnalyticsService analyticsService)
        {
            _navigationService = navigationService;
            _apiService = apiService;
            _dictionaryService = dictionaryService;
            _roamingStorageService = roamingStorageService;
            _localStorageService = localStorageService;
            _toastNotificationService = toastNotificationService;
            _analyticsService = analyticsService;

            InitializeAsync();

            Messenger.Default.Register<SelectDictionaryMessage>(this, (message) =>
            {
                CurrentDictionary = message.Dictionary;
            });
        }

        #endregion

        #region Private Methods

        private async void InitializeAsync()
        {
            // Retrieve current dictionary
            CurrentDictionary = _roamingStorageService.Read<Models.Dictionary>(StorageConstants.CurrentDictionary);
            if (CurrentDictionary == null)
            {
                CurrentDictionary = _dictionaryService.Get("en", "fr");
                _roamingStorageService.Save(StorageConstants.CurrentDictionary, CurrentDictionary);
            }

            // Retrieve last saved translation
            LastTranslation = await _roamingStorageService.ReadFileAsync<Models.TranslationResult>(StorageConstants.LastTranslation);
        }

        #endregion

        #region Methods

        public void SwitchDictionary()
        {
            CurrentDictionary = _dictionaryService.Get(CurrentDictionary.To, CurrentDictionary.From);
            RaisePropertyChanged(nameof(CurrentDictionary));

            // Send telemetry
            _analyticsService.TrackEvent("SwitchDictionary", new Dictionary<string, string> {
                { "From", CurrentDictionary.To },
                { "To", CurrentDictionary.From },
                { "NewFrom", CurrentDictionary.From },
                { "NewTo", CurrentDictionary.To }
            });
        }

        public void GoToSelectDictionaryPage()
        {
            _navigationService.NavigateTo("SelectDictionary");
        }

        public async void SearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                Suggestions.Clear();

                // Retrieve suggestions
                if (CanExecuteSearch)
                {
                    var suggestionsInverted = _localStorageService.Read(StorageConstants.InvertSuggestions, false);

                    var suggestions = await _apiService.RetrieveSuggestionsAsync(Word, CurrentDictionary);
                    foreach (var suggestion in suggestions)
                    {
                        if (suggestionsInverted)
                        {
                            Suggestions.Insert(0, suggestion);
                        }
                        else
                        {
                            Suggestions.Add(suggestion);
                        }
                    }
                }
            }
        }

        public async void SearchSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            // Select suggestion
            var suggestion = args.SelectedItem as Models.Suggestion;
            if (suggestion != null)
            {
                // Invert dictionary if the searched word is from another language
                if (suggestion.Language != CurrentDictionary.From)
                {
                    SwitchDictionary();
                }

                // Update text
                Word = suggestion.Word;

                await ExecuteSearchAsync();

                // Send telemetry
                _analyticsService.TrackEvent("SelectSuggestion", new Dictionary<string, string> {
                    { "From", LastTranslation.Dictionary.From },
                    { "To", LastTranslation.Dictionary.To },
                    { "Suggestion", Word }
                });
            }
        }

        public async void SearchQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                var suggestion = args.ChosenSuggestion as Models.Suggestion;
                Word = suggestion.Word;
            }
            else
            {
                await ExecuteSearchAsync();
            }
        }

        public async Task ExecuteSearchAsync()
        {
            // Do not execute search if we already searched this word
            if (LastTranslation != null
                && Word.Trim() == LastTranslation.OriginalWord.Trim()
                && LastTranslation.Dictionary.From == CurrentDictionary.From
                && LastTranslation.Dictionary.To == CurrentDictionary.To)
            {
                return;
            }

            // Execute search
            var searchResult = await _apiService.ExecuteSearchAsync(Word, CurrentDictionary);
            if (searchResult == null)
            {
                // Show toast notification that nothing was found
                _toastNotificationService.SendText(_resourceLoader.GetString("WordDoesNotExist"), "search");

                _analyticsService.TrackEvent("NotFound", new Dictionary<string, string> {
                    { "From", CurrentDictionary.From },
                    { "To", CurrentDictionary.To },
                    { "Word", Word }
                });

                return;
            }

            // Save last translation
            LastTranslation = searchResult;

            // Reset text search
            Word = string.Empty;

            // Remove suggestions list
            Suggestions.Clear();

            // Save result
            await _roamingStorageService.SaveFileAsync(StorageConstants.LastTranslation, LastTranslation);

            // Send result to other ViewModels
            Messenger.Default.Send<NewTranslationMessage>(new NewTranslationMessage
            {
                Translation = searchResult
            });

            // Send telemetry
            var properties = new Dictionary<string, string> {
                { "From", LastTranslation.Dictionary.From },
                { "To", LastTranslation.Dictionary.To },
                { "Word", LastTranslation.OriginalWord }
            };
            var metrics = new Dictionary<string, double> {
                { "PrincipalTranslations", LastTranslation.PrimaryTranslations.Count },
                { "AdditionalTranslations", LastTranslation.AdditionalTranslations.Count },
                { "CompoundForms", LastTranslation.CompoundForms.Count }
            };
            _analyticsService.TrackEvent("TranslationDone", properties, metrics);
        }

        #endregion
    }
}
