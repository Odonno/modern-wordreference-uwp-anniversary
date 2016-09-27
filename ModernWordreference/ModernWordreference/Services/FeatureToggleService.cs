using Microsoft.Services.Store.Engagement;
using ModernWordreference.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System.Profile;

namespace ModernWordreference.Services
{
    public interface IFeatureToggleService
    {
        bool IsNotificationBackgroundTasksEnabled();
        bool UseFeedbackHubApp();
        bool IsRunningWindowsMobile();
        bool LoadAllTranslations();
        bool ShowNewTranslationWidgetOnMainPage();
    }

    public class FeatureToggleService : IFeatureToggleService
    {
        private ILocalStorageService _localStorageService;
        private IRoamingStorageService _roamingStorageService;

        public FeatureToggleService(ILocalStorageService localStorageService, IRoamingStorageService roamingStorageService)
        {
            _localStorageService = localStorageService;
            _roamingStorageService = roamingStorageService;
        }

        // Deployment Toggles
        public bool IsNotificationBackgroundTasksEnabled()
        {
            return true;
        }

        // Runtime Toggles
        public bool UseFeedbackHubApp()
        {
            return StoreServicesFeedbackLauncher.IsSupported();
        }

        public bool IsRunningWindowsMobile()
        {
            return AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile";
        }

        public bool LoadAllTranslations()
        {
            return false;
        }

        // Permission Toggles

        // User settings Toggles
        public bool ShowNewTranslationWidgetOnMainPage()
        {
            bool showNewTranslationWidgetOnMainPage = _localStorageService.Read(StorageConstants.ShowNewTranslationWidgetOnMainPage, false);

            var translationSummaries = _roamingStorageService.Read<List<Models.TranslationSummary>>(StorageConstants.TranslationSummaries);
            bool hasNoTranslation = translationSummaries == null;

            return showNewTranslationWidgetOnMainPage && !hasNoTranslation;
        }

        // Experiment Toggles
    }
}
