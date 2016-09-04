using Microsoft.Toolkit.Uwp.UI.Animations;
using ModernWordreference.Infrastructure;
using ModernWordreference.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
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
            
            ViewModel.PropertyChanged += OnPropertyChanged;
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

        #endregion
    }
}
