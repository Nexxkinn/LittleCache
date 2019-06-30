# LittleCache

A simple UWP cache management library. [Available at nuget](https://www.nuget.org/packages/LittleCache/)

The current [HttpClient](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient) implementation doesn't have a proper implementation for developers to control the cache location. This leads to an occasional case of undisposable cache resided in an unreachable directory and require either reset the app or reinstall the app.

This library can helps you to cache your `url` response in a specified `folder_key` folder inside the local cache folder without worrying about creating a duplicated cache made by httpclient simply by calling
```
await LittleCache.CreateCacheFolderAsync(<folder_key>) // only need to be called once
await LittleCache.GetFromCacheAsync(<url>,<folder_key>)
```
Meanwhile, you have an ability to clear the whole cache folder simply by calling 
```
await LittleCache.DeleteCacheFolderAsync(<folder_key>)
```
### Limitation

The library only able to perform a simple HTTP GET request since It's meant to be used to reduce network usage by opening the cached file instead of requesting the same url.

### Warning

This library is only intended to store reusable content such as image, videos, text, etc. Any other kind such as authentication, sensitive information, cookie storing, and similar should avoid using this library as it does not perform any kind of encryption for performance improvement.