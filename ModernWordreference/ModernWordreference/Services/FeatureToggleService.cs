using Microsoft.Services.Store.Engagement;
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
        bool UseColorTitleBar();
    }

    public class FeatureToggleService : IFeatureToggleService
    {
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

        public bool UseColorTitleBar()
        {
            return false;
        }

        // Permission Toggles

        // User settings Toggles

        // Experiment Toggles
    }
}
