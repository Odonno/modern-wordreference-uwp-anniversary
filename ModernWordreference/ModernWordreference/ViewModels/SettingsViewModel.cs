using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using ModernWordreference.Constants;
using ModernWordreference.Messages;
using ModernWordreference.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace ModernWordreference.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        #region Fields

        private readonly ResourceLoader _resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        private ILocalStorageService _localStorageService;
        private IRoamingStorageService _roamingStorageService;

        #endregion

        #region Properties

        public List<string> Themes { get; } = new List<string>
        {
            "System (Dark/Light)",
            "Dark",
            "Light",
            "Original"
        };

        private string _selectedTheme;
        public string SelectedTheme
        {
            get { return _selectedTheme; }
            set { _selectedTheme = value; }
        }

        private bool _invertSuggestions;
        public bool InvertSuggestions
        {
            get { return _invertSuggestions; }
            set { _invertSuggestions = value; SaveInvertSuggestions(); }
        }

        private bool _showNewTranslationWidgetOnMainPage;
        public bool ShowNewTranslationWidgetOnMainPage
        {
            get { return _showNewTranslationWidgetOnMainPage; }
            set { _showNewTranslationWidgetOnMainPage = value; SaveShowNewTranslationWidgetOnMainPage(); }
        }

        private bool _enableDropShadow;
        public bool EnableDropShadow
        {
            get { return _enableDropShadow; }
            set { _enableDropShadow = value; SaveEnableDropShadow(); }
        }

        private bool _historyShowed;
        public bool HistoryShowed
        {
            get { return _historyShowed; }
            set { _historyShowed = value; SaveHistoryShowed(); }
        }

        private bool _instantTranslation;
        public bool InstantTranslation
        {
            get { return _instantTranslation; }
            set { _instantTranslation = value; SaveInstantTranslation(); }
        }      

        #endregion

        #region Constructor

        public SettingsViewModel(ILocalStorageService localStorageService, IRoamingStorageService roamingStorageService)
        {
            _localStorageService = localStorageService;
            _roamingStorageService = roamingStorageService;

            SelectedTheme = _localStorageService.Read(StorageConstants.SelectedTheme, Themes.First());
            InvertSuggestions = _localStorageService.Read(StorageConstants.InvertSuggestions, false);
            ShowNewTranslationWidgetOnMainPage = _localStorageService.Read(StorageConstants.ShowNewTranslationWidgetOnMainPage, false);
            EnableDropShadow = _localStorageService.Read(StorageConstants.EnableDropShadow, false);
            HistoryShowed = _localStorageService.Read(StorageConstants.HistoryShowed, true);
            InstantTranslation = _localStorageService.Read(StorageConstants.InstantTranslation, true);
        }

        #endregion

        #region Private methods

        private void SaveInvertSuggestions()
        {
            _localStorageService.Save(StorageConstants.InvertSuggestions, InvertSuggestions);
        }

        private void SaveShowNewTranslationWidgetOnMainPage()
        {
            _localStorageService.Save(StorageConstants.ShowNewTranslationWidgetOnMainPage, ShowNewTranslationWidgetOnMainPage);
        }

        private void SaveEnableDropShadow()
        {
            _localStorageService.Save(StorageConstants.EnableDropShadow, EnableDropShadow);
        }

        private void SaveHistoryShowed()
        {
            _localStorageService.Save(StorageConstants.HistoryShowed, HistoryShowed);
            Messenger.Default.Send(new HistoryToggleMessage());
        }

        private void SaveInstantTranslation()
        {
            _localStorageService.Save(StorageConstants.InstantTranslation, InstantTranslation);
        }

        #endregion

        #region Public methods

        public void SaveSelectedTheme(object sender, SelectionChangedEventArgs e)
        {
            var selection = e.AddedItems[0] as string;
            if (selection != SelectedTheme)
            {
                SelectedTheme = selection;
                _localStorageService.Save(StorageConstants.SelectedTheme, SelectedTheme);
            }
        }

        public async Task RemoveHistoryAsync()
        {
            var translationSummaries = _roamingStorageService.Read<List<Models.TranslationSummary>>(StorageConstants.TranslationSummaries);
            if (translationSummaries != null)
            {
                // Set "Removed" property on each translation summary
                var historyTranslations = translationSummaries.OrderByDescending(ts => ts.SearchedDate).Skip(1);
                foreach (var translationSummary in historyTranslations)
                {
                    translationSummary.Removed = true;
                }

                _roamingStorageService.Save(StorageConstants.TranslationSummaries, translationSummaries);

                Messenger.Default.Send(new HistoryRemovedMessage());

                // Show message
                var dialog = new MessageDialog(_resourceLoader.GetString("HistoryHasBeenRemoved"));
                dialog.Commands.Add(new UICommand(_resourceLoader.GetString("Ok")) { Id = 0 });
                await dialog.ShowAsync();
            }
        }

        #endregion
    }
}
