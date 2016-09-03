using ModernWordreference.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ModernWordreference.Templates
{
    public class TranslationCardsTemplateSelector : DataTemplateSelector
    {
        #region Properties

        public DataTemplate NewTranslationCardTemplate { get; set; }
        public DataTemplate ExistingTranslationCardTemplate { get; set; }

        #endregion

        #region Methods

        protected override DataTemplate SelectTemplateCore(object item)
        {
            return (item is TranslationResult) ? ExistingTranslationCardTemplate : NewTranslationCardTemplate;
        }

        #endregion
    }
}
