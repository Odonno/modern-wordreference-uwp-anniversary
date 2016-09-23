using GalaSoft.MvvmLight.Ioc;
using Microsoft.ApplicationInsights;
using ModernWordreference.Constants;
using ModernWordreference.Infrastructure;
using ModernWordreference.Services;
using ModernWordreference.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace ModernWordreference
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        #region Constructor

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            WindowsAppInitializer.InitializeAsync();

            this.InitializeComponent();
            this.Suspending += OnSuspending;

            // Set screen size
            SetScreenSize();

            // Load theme
            LoadTheme(false);
        }

        #endregion

        #region Events

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            // Load theme
            LoadTheme(true);

#if DEBUG
            if (Debugger.IsAttached)
            {
                DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            #region Page navigation

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }

            #endregion

            await InstallVoiceCommandDefinitionsAsync();
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Entry point for an application activated by some means other than normal launching.
        /// This includes voice commands, URI, share target from another app, and so on.
        /// </summary>
        /// <param name="args">Details about the activation method.</param>
        protected override async void OnActivated(IActivatedEventArgs args)
        {
            base.OnActivated(args);

            #region Page navigation

            // Repeat the same basic initialization as OnLaunched() above, taking into account whether or not the app is already active.
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content, just ensure that the window is active.
            if (rootFrame == null)
            {
                // Create a frame to act as the navigation context and navigate to the first page.
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;

                // Place the frame in the current window.
                Window.Current.Content = rootFrame;
            }

            // Since we're expecting to always show a details page, navigate even if a content frame is in place (unlike OnLaunched).
            // Navigate to either the main trip list page, or if a valid voice command was provided, to the details page for that trip.
            rootFrame.Navigate(typeof(MainPage), args);

            // Ensure the current window is active
            Window.Current.Activate();

            #endregion

            #region Voice handle

            // Voice command activation
            if (args.Kind == ActivationKind.VoiceCommand)
            {
                // Event args can represent many different activation types
                var commandArgs = args as VoiceCommandActivatedEventArgs;
                var speechRecognitionResult = commandArgs.Result;

                // Get the name of the voice command and the text spoken
                string voiceCommandName = speechRecognitionResult.RulePath[0];
                string textSpoken = speechRecognitionResult.Text;

                // commandMode indicates whether the command was entered using speech or text
                // Apps should respect text mode by providing silent (text) feedback
                string commandMode = speechRecognitionResult.SemanticInterpretation.Properties["commandMode"].FirstOrDefault();

                switch (voiceCommandName)
                {
                    case "startNewTranslation":
                        // Start new translation from main page
                        await ViewModelLocator.Main.StartNewTranslationAsync();
                        break;
                    default:
                        // If we can't determine what page to launch, go to the default entry point.
                        break;
                }
            }

            #endregion
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        #endregion

        #region Methods

        private void SetScreenSize()
        {
            ApplicationView.PreferredLaunchViewSize = new Size(500, 800);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
        }

        private void SetStatusBar()
        {
            if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                var statusBar = StatusBar.GetForCurrentView();
                statusBar.BackgroundOpacity = 1;
                statusBar.BackgroundColor = Color.FromArgb(255, 94, 88, 198);
            }
        }

        private void SetTitleBar()
        {
            // Set title bar colors
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            var backgroundColor = Color.FromArgb(255, 94, 88, 198);
            var foregroundColor = Color.FromArgb(255, 255, 255, 255);

            // Title bar
            titleBar.ForegroundColor = foregroundColor;
            titleBar.BackgroundColor = backgroundColor;

            // Title bar buttons
            titleBar.ButtonForegroundColor = foregroundColor;
            titleBar.ButtonBackgroundColor = backgroundColor;

            titleBar.ButtonHoverForegroundColor = backgroundColor;
            titleBar.ButtonHoverBackgroundColor = foregroundColor;

            titleBar.ButtonPressedForegroundColor = foregroundColor;
            titleBar.ButtonPressedBackgroundColor = backgroundColor;
        }

        private void LoadTheme(bool appLoaded)
        {
            string selectedTheme = new LocalStorageService().Read<string>(StorageConstants.SelectedTheme);

            if (selectedTheme == "Original")
            {
                // Original theme
                if (!appLoaded)
                    RequestedTheme = ApplicationTheme.Dark;

                if (appLoaded)
                {
                    Resources.ThemeDictionaries.Add("Default", new ResourceDictionary { Source = new Uri("ms-appx:///Styles/OriginalThemeDictionary.xaml") });
                    Resources.ThemeDictionaries.Remove("Dark");
                    Resources.ThemeDictionaries.Remove("Light");

                    SetTitleBar();
                    SetStatusBar();
                }
            }
            else if (selectedTheme == "Light")
            {
                // Light theme
                if (!appLoaded)
                    RequestedTheme = ApplicationTheme.Light;
            }
            else if (selectedTheme == "Dark")
            {
                // Dark theme
                if (!appLoaded)
                    RequestedTheme = ApplicationTheme.Dark;
            }
            else
            {
                // System theme
            }
        }

        private async Task InstallVoiceCommandDefinitionsAsync()
        {
            try
            {
                // Install the main VCD
                var vcdStorageFile = await Package.Current.InstalledLocation.GetFileAsync(@"ModernWordReferenceCommands.xml");
                await VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(vcdStorageFile);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Installing Voice Commands Failed: " + ex.ToString());
            }
        }

        #endregion
    }
}
