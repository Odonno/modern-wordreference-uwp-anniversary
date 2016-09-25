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
using Windows.UI.Xaml.Controls;

namespace ModernWordreference.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        #region Fields

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

        #endregion

        #region Constructor

        public SettingsViewModel(ILocalStorageService localStorageService, IRoamingStorageService roamingStorageService)
        {
            _localStorageService = localStorageService;
            _roamingStorageService = roamingStorageService;

            SelectedTheme = _localStorageService.Read(StorageConstants.SelectedTheme, "System (Dark/Light)");
            InvertSuggestions = _localStorageService.Read(StorageConstants.InvertSuggestions, false);
            ShowNewTranslationWidgetOnMainPage = _localStorageService.Read(StorageConstants.ShowNewTranslationWidgetOnMainPage, false);
            EnableDropShadow = _localStorageService.Read(StorageConstants.EnableDropShadow, false);
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

        public void RemoveHistory()
        {
            var translationSummaries = _roamingStorageService.Read<List<Models.TranslationSummary>>(StorageConstants.TranslationSummaries);
            if (translationSummaries != null)
            {
                var historyTranslations = translationSummaries.OrderByDescending(ts => ts.SearchedDate).Skip(1);
                foreach (var translationSummary in historyTranslations)
                {
                    translationSummary.Removed = true;
                }

                _roamingStorageService.Save(StorageConstants.TranslationSummaries, translationSummaries);

                Messenger.Default.Send(new HistoryRemovedMessage());
            }
        }

        #endregion
    }
}
