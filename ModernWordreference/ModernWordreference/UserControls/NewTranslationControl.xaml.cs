using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Practices.ServiceLocation;
using ModernWordreference.Constants;
using ModernWordreference.Infrastructure;
using ModernWordreference.Messages;
using ModernWordreference.Services;
using ModernWordreference.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ModernWordreference.UserControls
{
    public sealed partial class NewTranslationControl : UserControl
    {
        #region Properties

        public NewTranslationViewModel ViewModel { get { return (NewTranslationViewModel)DataContext; } }
        public MainViewModel MainViewModel { get { return ViewModelLocator.Main; } }

        #endregion

        #region Dependency Properties

        public bool ShowBackground
        {
            get { return (bool)GetValue(ShowBackgroundProperty); }
            set { SetValue(ShowBackgroundProperty, value); }
        }
        public static readonly DependencyProperty ShowBackgroundProperty =
            DependencyProperty.Register(nameof(ShowBackground), typeof(bool), typeof(NewTranslationControl), new PropertyMetadata(true));

        public bool ShowBorderBrush
        {
            get { return (bool)GetValue(ShowBorderBrushProperty); }
            set { SetValue(ShowBorderBrushProperty, value); }
        }
        public static readonly DependencyProperty ShowBorderBrushProperty =
            DependencyProperty.Register(nameof(ShowBorderBrush), typeof(bool), typeof(NewTranslationControl), new PropertyMetadata(true));

        public bool ShowHideButton
        {
            get { return (bool)GetValue(ShowHideButtonProperty); }
            set { SetValue(ShowHideButtonProperty, value); }
        }
        public static readonly DependencyProperty ShowHideButtonProperty =
            DependencyProperty.Register(nameof(ShowHideButton), typeof(bool), typeof(NewTranslationControl), new PropertyMetadata(true));

        #endregion

        #region Constructor

        public NewTranslationControl()
        {
            this.InitializeComponent();

            Loaded += NewTranslationControl_Loaded;

            Messenger.Default.Register<ShowNewTranslationControlMessage>(this, async _ =>
            {
                var localStorageService = ServiceLocator.Current.GetInstance<ILocalStorageService>();
                bool showNewTranslationWidgetOnMainPage = localStorageService.Read(StorageConstants.ShowNewTranslationWidgetOnMainPage, false);
                if (showNewTranslationWidgetOnMainPage)
                {
                    await Task.Delay(500);
                }

                WordTextBox.Focus(FocusState.Programmatic);
            });
        }

        #endregion

        #region Events

        private void NewTranslationControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!ShowBorderBrush)
            {
                RootGrid.BorderThickness = new Thickness(0);
            }

            if (!ShowHideButton)
            {
                HideButton.Visibility = Visibility.Collapsed;
            }

            if (!ShowBackground)
            {
                RootGrid.Background = new SolidColorBrush(Colors.Transparent);
            }

            WordTextBox.Focus(FocusState.Programmatic);
        }

        #endregion
    }
}
