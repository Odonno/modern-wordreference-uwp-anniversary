using GalaSoft.MvvmLight;
using ModernWordreference.Constants;
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

        #endregion

        #region Constructor

        public SettingsViewModel(ILocalStorageService localStorageService)
        {
            _localStorageService = localStorageService;

            SelectedTheme = _localStorageService.Read(StorageConstants.SelectedTheme, "System (Dark/Light)");
        }

        #endregion

        #region Private methods

        public void SaveSelectedTheme(object sender, SelectionChangedEventArgs e)
        {
            var selection = e.AddedItems[0] as string;
            if (selection != SelectedTheme)
            {
                SelectedTheme = selection;
                _localStorageService.Save(StorageConstants.SelectedTheme, SelectedTheme);
            }
        }

        #endregion
    }
}
