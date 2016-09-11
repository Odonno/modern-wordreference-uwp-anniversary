using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ModernWordreference.Services
{
    public interface IRoamingStorageService : IStorageService
    {
    }

    public class RoamingStorageService : BaseStorageService, IRoamingStorageService
    {
        public RoamingStorageService()
        {
            Settings = ApplicationData.Current.RoamingSettings;
            Folder = ApplicationData.Current.RoamingFolder;
        }
    }
}
