using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ModernWordreference.Services
{
    public interface IStorageService
    {
        /// <summary>
        /// Retrieve single item by its key
        /// </summary>
        /// <typeparam name="T">Type of object retrieved</typeparam>
        /// <param name="key">Key of the object</param>
        /// <param name="default">Default value of the object</param>
        /// <returns>The T object</returns>
        T Read<T>(string key, T @default = default(T));

        /// <summary>
        /// Save single item by its key
        /// </summary>
        /// <typeparam name="T">Type of object saved</typeparam>
        /// <param name="key">Key of the value saved</param>
        /// <param name="value">Object to save</param>
        void Save<T>(string key, T value);

        /// <summary>
        /// Retrieve object from file
        /// </summary>
        /// <typeparam name="T">Type of object retrieved</typeparam>
        /// <param name="filePath">Path to the file that contains the object</param>
        /// <param name="default">Default value of the object</param>
        /// <returns>Waiting task until completion with the object in the file</returns>
        Task<T> ReadFileAsync<T>(string filePath, T @default = default(T));

        /// <summary>
        /// Save object inside file
        /// </summary>
        /// <typeparam name="T">Type of object saved</typeparam>
        /// <param name="filePath">Path to the file that will contain the object</param>
        /// <param name="value">Object to save</param>
        /// <returns>Waiting task until completion</returns>
        Task SaveFileAsync<T>(string filePath, T value);
    }

    public class BaseStorageService : IStorageService
    {
        #region Fields

        private JsonSerializer serializer = new JsonSerializer();

        protected ApplicationDataContainer Settings { get; set; }
        protected StorageFolder Folder { get; set; }

        #endregion

        #region Methods

        public T Read<T>(string key, T @default = default(T))
        {
            string value = (string)Settings.Values[key];
            if (value != null)
            {
                return JsonConvert.DeserializeObject<T>(value);
            }

            return @default;
        }

        public void Save<T>(string key, T value)
        {
            Settings.Values[key] = JsonConvert.SerializeObject(value);
        }

        public async Task<T> ReadFileAsync<T>(string filePath, T @default = default(T))
        {
            var file = await Folder.CreateFileAsync(filePath, CreationCollisionOption.OpenIfExists);
            if (file != null)
            {
                string value = await FileIO.ReadTextAsync(file);
                if (value != null)
                {
                    return JsonConvert.DeserializeObject<T>(value);
                }
            }

            return @default;
        }

        public async Task SaveFileAsync<T>(string filePath, T value)
        {
            var file = await Folder.CreateFileAsync(filePath, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, JsonConvert.SerializeObject(value));
        }

        #endregion
    }
}
