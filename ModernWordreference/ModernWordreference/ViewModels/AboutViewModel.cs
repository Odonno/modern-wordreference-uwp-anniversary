using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace ModernWordreference.ViewModels
{
    public class AboutViewModel : ViewModelBase
    {
        #region Properties

        public string AppVersion { get; set; }

        #endregion

        #region Constructor

        public AboutViewModel()
        {
            // Retrieve current package app version
            var currentPackage = Package.Current;
            var packageVersion = currentPackage.Id.Version;
            AppVersion = $"v{packageVersion.Major}.{packageVersion.Minor}.{packageVersion.Build}";
        }

        #endregion
    }
}
