using Microsoft.Practices.ServiceLocation;
using Microsoft.Toolkit.Uwp.UI.Animations;
using ModernWordreference.Constants;
using ModernWordreference.Infrastructure;
using ModernWordreference.Services;
using ModernWordreference.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ModernWordreference.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        #region Fields

        private bool _initialized = false;

        #endregion

        #region Properties

        public MainViewModel ViewModel { get { return (MainViewModel)DataContext; } }

        #endregion

        #region Constructor

        public MainPage()
        {
            this.InitializeComponent();

            // Set navigation system
            var systemNavigationManager = SystemNavigationManager.GetForCurrentView();
            systemNavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            systemNavigationManager.BackRequested += (object sender, BackRequestedEventArgs e) =>
            {
                if (Frame.CanGoBack)
                {
                    e.Handled = true;
                    Frame.GoBack();
                }
            };

            // Handle events
            ViewModel.PropertyChanged += OnPropertyChanged;
            Loaded += (object sender, RoutedEventArgs e) =>
            {
                if (!_initialized)
                {
                    var localStorageService = ServiceLocator.Current.GetInstance<ILocalStorageService>();
                    bool enableDropShadow = localStorageService.Read(StorageConstants.EnableDropShadow, false);
                    if (enableDropShadow)
                    {
                        // Add animations
                        CreateDropShadowOnTranslationInfosGrid();
                    }
                }

                _initialized = true;
            };
        }

        #endregion

        #region Events

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var systemNavigationManager = SystemNavigationManager.GetForCurrentView();
            systemNavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;

            base.OnNavigatedTo(e);
        }

        private async void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.ShowNewTranslationControl))
            {
                if (ViewModel.ShowNewTranslationControl)
                {
                    await ShowNewTranslationControlAsync();
                }
                else
                {
                    await HideNewTranslationControlAsync();
                }
            }
        }

        #endregion

        #region Animation

        private async Task ShowNewTranslationControlAsync()
        {
            await ContentGrid.Blur(10f).StartAsync();
            await NewTranslationControl.Fade(1).StartAsync();
        }

        private async Task HideNewTranslationControlAsync()
        {
            await ContentGrid.Blur(0f).StartAsync();
            await NewTranslationControl.Fade().StartAsync();
        }

        private void CreateDropShadowOnTranslationInfosGrid()
        {
            var hostVisual = ElementCompositionPreview.GetElementVisual(TranslationInfosSubGrid);
            var compositor = hostVisual.Compositor;
            SolidColorBrush dropShadowThemeBrush = null;

            // Create a drop shadow
            var dropShadow = compositor.CreateDropShadow();
            dropShadow.BlurRadius = 15f;
            dropShadow.Offset = new Vector3(0f, 2.5f, 0f);

            // Detect theme color change
            var dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += async (sender, e) =>
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher
                       .RunAsync(CoreDispatcherPriority.High, () =>
                       {
                           dropShadowThemeBrush = Application.Current.Resources["TranslationInfosDropShadowColorBrush"] as SolidColorBrush;
                           if (dropShadow.Color != dropShadowThemeBrush.Color)
                           {
                               dropShadow.Color = dropShadowThemeBrush.Color;
                           }
                       });
            };
            dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            dispatcherTimer.Start();

            // Set color by default (first start)
            dropShadowThemeBrush = Application.Current.Resources["TranslationInfosDropShadowColorBrush"] as SolidColorBrush;
            if (dropShadow.Color != dropShadowThemeBrush.Color)
            {
                dropShadow.Color = dropShadowThemeBrush.Color;
            }

            // Associate the shape of the shadow with the shape of the target element
            dropShadow.Mask = TranslationInfosShape.GetAlphaMask();

            // Create a Visual to hold the shadow
            var shadowVisual = compositor.CreateSpriteVisual();
            shadowVisual.Shadow = dropShadow;

            // Add the shadow as a child of the host in the visual tree
            ElementCompositionPreview.SetElementChildVisual(TranslationInfosSubGrid, shadowVisual);

            // Make sure size of shadow host and shadow visual always stay in sync
            var bindSizeAnimation = compositor.CreateExpressionAnimation("hostVisual.Size");
            bindSizeAnimation.SetReferenceParameter("hostVisual", hostVisual);

            shadowVisual.StartAnimation("Size", bindSizeAnimation);
        }

        #endregion
    }
}
