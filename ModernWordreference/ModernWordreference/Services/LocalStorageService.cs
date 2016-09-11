using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ModernWordreference.Services
{
    public interface ILocalStorageService : IStorageService
    {
    }

    public class LocalStorageService : BaseStorageService, ILocalStorageService
    {
        public LocalStorageService()
        {
            Settings = ApplicationData.Current.LocalSettings;
            Folder = ApplicationData.Current.LocalFolder;
        }
    }
}
