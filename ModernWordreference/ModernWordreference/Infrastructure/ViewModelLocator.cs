using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using Microsoft.Practices.ServiceLocation;
using ModernWordreference.Services;
using ModernWordreference.ViewModels;
using ModernWordreference.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModernWordreference.Infrastructure
{
    public class ViewModelLocator
    {
        #region Constructor

        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            // Initialize Navigation
            if (!SimpleIoc.Default.IsRegistered<INavigationService>())
            {
                var navigationService = CreateNavigationService();
                SimpleIoc.Default.Register<INavigationService>(() => navigationService);
            }

            // Register Services
            if (!SimpleIoc.Default.IsRegistered<IAnalyticsService>())
            {
                var analyticsService = new AnalyticsService();
                SimpleIoc.Default.Register<IAnalyticsService>(() => analyticsService);
            }

            if (!SimpleIoc.Default.IsRegistered<IApiService>())
            {
                SimpleIoc.Default.Register<IApiService, ApiService>();
            }

            if (!SimpleIoc.Default.IsRegistered<IDictionaryService>())
            {
                var dictionaryService = new DictionaryService();
                dictionaryService.Initialize();
                SimpleIoc.Default.Register<IDictionaryService>(() => dictionaryService);
            }

            if (!SimpleIoc.Default.IsRegistered<IFeatureToggleService>())
            {
                SimpleIoc.Default.Register<IFeatureToggleService, FeatureToggleService>();
            }

            if (!SimpleIoc.Default.IsRegistered<INetworkService>())
            {
                SimpleIoc.Default.Register<INetworkService, NetworkService>();
            }

            if (!SimpleIoc.Default.IsRegistered<IReactiveService>())
            {
                SimpleIoc.Default.Register<IReactiveService, ReactiveService>();
            }

            if (!SimpleIoc.Default.IsRegistered<IRoamingStorageService>())
            {
                SimpleIoc.Default.Register<IRoamingStorageService, RoamingStorageService>();
            }

            if (!SimpleIoc.Default.IsRegistered<IToastNotificationService>())
            {
                SimpleIoc.Default.Register<IToastNotificationService, ToastNotificationService>();
            }

            // Register ViewModels
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<AboutViewModel>();
            SimpleIoc.Default.Register<LoveViewModel>();
            SimpleIoc.Default.Register<NewTranslationViewModel>();
            SimpleIoc.Default.Register<SelectDictionaryViewModel>();
        }

        #endregion

        #region Methods

        private INavigationService CreateNavigationService()
        {
            var navigationService = new NavigationService();

            navigationService.Configure("Main", typeof(MainPage));
            navigationService.Configure("About", typeof(AboutPage));
            navigationService.Configure("Love", typeof(LovePage));
            navigationService.Configure("SelectDictionary", typeof(SelectDictionaryPage));

            return navigationService;
        }

        #endregion

        #region ViewModels
        
        public static MainViewModel Main
        {
            get { return ServiceLocator.Current.GetInstance<MainViewModel>(); }
        }

        public static AboutViewModel About
        {
            get { return ServiceLocator.Current.GetInstance<AboutViewModel>(); }
        }

        public static LoveViewModel Love
        {
            get { return ServiceLocator.Current.GetInstance<LoveViewModel>(); }
        }

        public static NewTranslationViewModel NewTranslation
        {
            get { return ServiceLocator.Current.GetInstance<NewTranslationViewModel>(); }
        }

        public static SelectDictionaryViewModel SelectDictionary
        {
            get { return ServiceLocator.Current.GetInstance<SelectDictionaryViewModel>(); }
        }

        #endregion
    }
}
