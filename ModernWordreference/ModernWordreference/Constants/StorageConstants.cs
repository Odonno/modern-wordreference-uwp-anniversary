using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModernWordreference.Constants
{
    public static class StorageConstants
    {
        /// <summary>
        /// Save / Retrieve current dictionary selected
        /// </summary>
        public static string CurrentDictionary = "currentDictionary";

        /// <summary>
        /// Save / Retrieve last executed translation
        /// </summary>
        public static string LastTranslation = "lastTranslation.txt";

        /// <summary>
        /// Save / Retrieve count of IAP buyed
        /// </summary>
        public static string IapBuyed = "iapBuyed";

        /// <summary>
        /// Save / Retrieve list of all translations (summarized by searched date and file name)
        /// </summary>
        public static string TranslationSummaries = "allTranslationsList.txt";

        /// <summary>
        /// Save / Retrieve selected theme
        /// </summary>
        public static string SelectedTheme = "selectedTheme";

        /// <summary>
        /// Save / Retrieve if user invert suggestions
        /// </summary>
        public static string InvertSuggestions = "invertSuggestions";

        /// <summary>
        /// Save / Retrieve if drop shadow is enabled or disabled
        /// </summary>
        public static string EnableDropShadow = "enableDropShadow";
    }
}
