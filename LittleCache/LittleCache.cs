using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace LittleCache
{
    public static class LittleCache
    {
        private static StorageFolder LocalCacheFolder = ApplicationData.Current.LocalCacheFolder;
        private static HttpClient Httpclient;
        
        static LittleCache()
        {
            if (Httpclient == null)
                Httpclient = new HttpClient(new HttpClientHandler { MaxConnectionsPerServer = 100 });
        }

        public static async Task CreateCacheFolderAsync(string folder_key = "ImageCache")
        {
            if (string.IsNullOrEmpty(folder_key)) throw new LittleCacheExceptions("folder key parameter required");
            await LocalCacheFolder.CreateFolderAsync(folder_key, CreationCollisionOption.OpenIfExists);            
        }
        public static async Task CreateCacheFolderAsync(IList<string> folder_keys)
        {
            if (folder_keys.Count == 0) throw new LittleCacheExceptions("folder keys required");

            foreach (var key in folder_keys)
            {
                if (string.IsNullOrEmpty(key)) throw new LittleCacheExceptions("one or more keys listed is considered invalid");
                await LocalCacheFolder.CreateFolderAsync(key, CreationCollisionOption.OpenIfExists);
            }
        }
        public static async Task DeleteCacheFolderAsync(string filename = "ImageCache",bool permanent_delete = false)
        {
            var item = await LocalCacheFolder.TryGetItemAsync(filename).AsTask().ConfigureAwait(false);
            if (item != null && item.IsOfType(StorageItemTypes.Folder))
            {
                StorageDeleteOption option = permanent_delete ? StorageDeleteOption.PermanentDelete : StorageDeleteOption.Default;
                await item.DeleteAsync(option).AsTask().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// For Test use.
        /// </summary>
        private static async Task DeleteAllCacheFolderAsync(bool permanent_delete = false)
        {
            StorageDeleteOption option = permanent_delete ? StorageDeleteOption.PermanentDelete : StorageDeleteOption.Default;
            await LocalCacheFolder.DeleteAsync().AsTask().ConfigureAwait(false);
        }

        /// <summary>
        /// Request and cache the response inside the specified folder;
        /// </summary>
        /// <param name="url">Url needed</param>
        /// <param name="folder_key">to place the result in the specified folder</param>
        /// <returns>cached file</returns>
        public static async Task<StorageFile> GetFromCacheAsync(Uri url, string folder_key = "ImageCache")
        {
            return await GetFromCacheAsync(url.AbsoluteUri,folder_key);
        }
        public static async Task<StorageFile> GetFromCacheAsync(string url,string folder_key = "ImageCache")
        {
            StorageFile baseFile = null;
            StorageFolder folder = await LocalCacheFolder.GetFolderAsync(folder_key).AsTask().ConfigureAwait(false);

            var hash = GetCacheFileName(url);
            try
            {
                baseFile = await folder.TryGetItemAsync(hash).AsTask().ConfigureAwait(false) as StorageFile;
                if (baseFile != null)
                {
                    //Even if baseFile says that there's a file in that folder,
                    //You should check whether nor not the app is already cached.
                    var baseFileSize = await baseFile.GetBasicPropertiesAsync().AsTask().ConfigureAwait(false);
                    if (baseFileSize.Size != 0) return baseFile;
                }
                baseFile = await DownloadAsync(url, hash, folder_key);
                return baseFile;
            }
            finally
            {
                folder = null;
                baseFile = null;
                hash = string.Empty;
            }
        }

        /// <summary>
        /// Force HTTP GET request and replace the cached response.
        /// </summary>
        /// <param name="url">Url needed</param>
        /// <param name="folder_key">to place the result in the specified folder</param>
        /// <returns>newly cached file</returns>
        public static async Task<StorageFile> ForceDownloadAsync(Uri url, string folder_key = "ImageCache")
        {
            return await ForceDownloadAsync(url.AbsoluteUri, folder_key);
        }
        public static async Task<StorageFile> ForceDownloadAsync(string url,string folder_key = "ImageCache")
        {
            var hash = GetCacheFileName(url);
            return await DownloadAsync(url, hash, folder_key);
        }

        private static async Task<StorageFile> DownloadAsync(string url, string hash,string folder_key)
        {
            StorageFolder folder = await LocalCacheFolder.GetFolderAsync(folder_key).AsTask().ConfigureAwait(false);
            var file = await folder.CreateFileAsync(hash, CreationCollisionOption.ReplaceExisting).AsTask().ConfigureAwait(false);
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (var stream = await Httpclient.GetStreamAsync(url))
                    {
                        stream.CopyTo(ms);
                        ms.Flush();
                        ms.Position = 0;

                        using (var streamfile = await file.OpenStreamForWriteAsync())
                        {
                            ms.CopyTo(streamfile);
                            streamfile.Flush();
                        }
                    }
                }
                return file;
            }
            finally
            {
                folder = null;
                file = null;
            }
        }
        private static string GetCacheFileName(string uri)
        {
            return CreateHash64(uri).ToString();
        }
        private static ulong CreateHash64(string str)
        {
            byte[] utf8 = Encoding.UTF8.GetBytes(str);
            ulong value = (ulong)utf8.Length;
            for (int n = 0; n < utf8.Length; n++)
            {
                value += (ulong)utf8[n] << ((n * 5) % 56);
            }
            return value;
        }
    }
}
