using GalaSoft.MvvmLight.Ioc;
using ModernWordreference.Services;
using ModernWordreference.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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

        #endregion

        #region Constructor

        public NewTranslationControl()
        {
            this.InitializeComponent();

            Loaded += NewTranslationControl_Loaded;

            SimpleIoc.Default.GetInstance<IReactiveService>().ShowNewTranslationControlDone.Subscribe(_ =>
            {
                WordTextBox.Focus(FocusState.Programmatic);
            });
        }

        #endregion

        #region Events

        private void NewTranslationControl_Loaded(object sender, RoutedEventArgs e)
        {
            WordTextBox.Focus(FocusState.Programmatic);
        }

        #endregion
    }
}
