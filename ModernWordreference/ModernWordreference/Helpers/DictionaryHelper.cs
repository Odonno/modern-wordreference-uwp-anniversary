using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace ModernWordreference.Helpers
{
    public static class DictionaryHelper
    {
        private static readonly ResourceLoader _resourceLoader = ResourceLoader.GetForCurrentView("LanguageResources");

        public static string GetLanguage(string countryCode)
        {
            switch (countryCode)
            {
                case "en":
                    return _resourceLoader.GetString("english");
                case "fr":
                    return _resourceLoader.GetString("french");
                case "es":
                    return _resourceLoader.GetString("spanish");
                case "it":
                    return _resourceLoader.GetString("italian");
                case "de":
                    return _resourceLoader.GetString("german");
                case "nl":
                    return _resourceLoader.GetString("dutch");
                case "sv":
                    return _resourceLoader.GetString("swedish");
                case "ru":
                    return _resourceLoader.GetString("russian");
                case "pt":
                    return _resourceLoader.GetString("portuguese");
                case "pl":
                    return _resourceLoader.GetString("polish");
                case "ro":
                    return _resourceLoader.GetString("romanian");
                case "cz":
                    return _resourceLoader.GetString("czech");
                case "gr":
                    return _resourceLoader.GetString("greek");
                case "tr":
                    return _resourceLoader.GetString("turkish");
                case "zh":
                    return _resourceLoader.GetString("chinese");
                case "ja":
                    return _resourceLoader.GetString("japanese");
                case "ko":
                    return _resourceLoader.GetString("korean");
                case "ar":
                    return _resourceLoader.GetString("arabic");
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
