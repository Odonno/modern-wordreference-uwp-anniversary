using GalaSoft.MvvmLight;
using ModernWordreference.Constants;
using ModernWordreference.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Store;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ModernWordreference.ViewModels
{
    public class LoveViewModel : ViewModelBase
    {
        #region Fields

        private readonly ResourceLoader _resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        private IToastNotificationService _toastNotificationService;
        private IRoamingStorageService _storageService;
        private IAnalyticsService _analyticsService;

        #endregion

        #region Properties

        private Models.IapBuyed _iapBuyed;
        public Models.IapBuyed IapBuyed
        {
            get { return _iapBuyed; }
            set { _iapBuyed = value; RaisePropertyChanged(); }
        }

        private string _thankYouText;
        public string ThankYouText
        {
            get { return _thankYouText; }
            set { _thankYouText = value; RaisePropertyChanged(); }
        }

        #endregion

        #region Contructor

        public LoveViewModel(IToastNotificationService toastNotificationService, IRoamingStorageService storageService, IAnalyticsService analyticsService)
        {
            _toastNotificationService = toastNotificationService;
            _storageService = storageService;
            _analyticsService = analyticsService;

            Initialize();
        }

        #endregion

        #region Private Methods

        private void Initialize()
        {
            // Retrieve already purchased items
            IapBuyed = _storageService.Retrieve<Models.IapBuyed>(StorageConstants.IapBuyed);
            if (IapBuyed == null)
            {
                IapBuyed = new Models.IapBuyed();
            }

            UpdateUI();
        }

        private void UpdateUI()
        {
            if (IapBuyed.Word > 0 || IapBuyed.Page > 0 || IapBuyed.Book > 0)
            {
                var sb = new StringBuilder();

                sb.Append(_resourceLoader.GetString("YouBought"));

                if (IapBuyed.Word > 0)
                {
                    sb.Append($" {IapBuyed.Word} word");
                    if (IapBuyed.Word > 1)
                        sb.Append("s");
                    if (IapBuyed.Page > 0)
                        sb.Append(",");
                }

                if (IapBuyed.Page > 0)
                {
                    sb.Append($" {IapBuyed.Page} page");
                    if (IapBuyed.Page > 1)
                        sb.Append("s");
                    if (IapBuyed.Book > 0)
                        sb.Append(",");
                }

                if (IapBuyed.Book > 0)
                {
                    sb.Append($" {IapBuyed.Book} book");
                    if (IapBuyed.Book > 1)
                        sb.Append("s");
                }

                sb.Append(" ! ");
                sb.Append(_resourceLoader.GetString("ThankYouVeryMuch"));

                ThankYouText = sb.ToString();
            }
            else
            {
                ThankYouText = string.Empty;
            }
        }

        #endregion

        #region Methods

        public async Task RateAppAsync()
        {
            // Rate the app
            var uriToLaunch = $"ms-windows-store:REVIEW?PFN={Package.Current.Id.FamilyName}";
            var uri = new Uri(uriToLaunch);
            bool result = await Launcher.LaunchUriAsync(uri);

            if (result)
            {
                // Send telemetry
                _analyticsService.TrackEvent("RateApp");
            }
        }

        public async Task BuyIapAsync(object sender, RoutedEventArgs e)
        {
            string name = (sender as FrameworkElement).Name;

            // Send telemetry
            _analyticsService.TrackEvent("StartBuyIAP", new Dictionary<string, string> { { "iapName", name } });

#if DEBUG
            // Use simulator info
            var licenseInformation = CurrentAppSimulator.LicenseInformation;

            // Buy IAP
            var requestPurchase = await CurrentAppSimulator.RequestProductPurchaseAsync(name);
#else
            // Use release licensing info
            var licenseInformation = CurrentApp.LicenseInformation;

            // Buy IAP
            var requestPurchase = await CurrentApp.RequestProductPurchaseAsync(name);
#endif

            var productLicense = licenseInformation.ProductLicenses[name];
            if (productLicense != null && productLicense.IsActive)
            {
                // Show toast notification (thanks to support us)
                _toastNotificationService.SendTitleAndText(_resourceLoader.GetString("ThankYou"), _resourceLoader.GetString("ThankYouToSupportUs"), "purchase");

                // Save buyed IAP
                if (name == "theWord")
                {
                    IapBuyed.Word++;
                }
                if (name == "thePage")
                {
                    IapBuyed.Page++;
                }
                if (name == "theBook")
                {
                    IapBuyed.Book++;
                }

                UpdateUI();

                _storageService.Save(StorageConstants.IapBuyed, IapBuyed);

                // Send telemetry
                _analyticsService.TrackEvent("SuccessBuyIAP", new Dictionary<string, string> { { "iapName", name } });
            }
            else
            {
                // Show toast notification (something going wrong)
                _toastNotificationService.SendTitleAndText(_resourceLoader.GetString("PurchaseFailed"), _resourceLoader.GetString("PurchaseFailedExtended"), "purchase");
            }
        }

        #endregion
    }
}
