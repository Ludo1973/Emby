﻿using MediaBrowser.Common.Extensions;
using MediaBrowser.Common.IO;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Net;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Providers;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommonIO;
using MediaBrowser.Model.Serialization;

namespace MediaBrowser.Api
{
    [Route("/Items/{Id}/ExternalIdInfos", "GET", Summary = "Gets external id infos for an item")]
    [Authenticated(Roles = "Admin")]
    public class GetExternalIdInfos : IReturn<List<ExternalIdInfo>>
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        [ApiMember(Name = "Id", Description = "Item Id", IsRequired = true, DataType = "string", ParameterType = "path", Verb = "GET")]
        public string Id { get; set; }
    }

    [Route("/Items/RemoteSearch/Movie", "POST")]
    [Authenticated]
    public class GetMovieRemoteSearchResults : RemoteSearchQuery<MovieInfo>, IReturn<List<RemoteSearchResult>>
    {
    }

    [Route("/Items/RemoteSearch/AdultVideo", "POST")]
    [Authenticated]
    public class GetAdultVideoRemoteSearchResults : RemoteSearchQuery<ItemLookupInfo>, IReturn<List<RemoteSearchResult>>
    {
    }

    [Route("/Items/RemoteSearch/Series", "POST")]
    [Authenticated]
    public class GetSeriesRemoteSearchResults : RemoteSearchQuery<SeriesInfo>, IReturn<List<RemoteSearchResult>>
    {
    }

    [Route("/Items/RemoteSearch/Game", "POST")]
    [Authenticated]
    public class GetGameRemoteSearchResults : RemoteSearchQuery<GameInfo>, IReturn<List<RemoteSearchResult>>
    {
    }

    [Route("/Items/RemoteSearch/BoxSet", "POST")]
    [Authenticated]
    public class GetBoxSetRemoteSearchResults : RemoteSearchQuery<BoxSetInfo>, IReturn<List<RemoteSearchResult>>
    {
    }

    [Route("/Items/RemoteSearch/MusicArtist", "POST")]
    [Authenticated]
    public class GetMusicArtistRemoteSearchResults : RemoteSearchQuery<ArtistInfo>, IReturn<List<RemoteSearchResult>>
    {
    }

    [Route("/Items/RemoteSearch/MusicAlbum", "POST")]
    [Authenticated]
    public class GetMusicAlbumRemoteSearchResults : RemoteSearchQuery<AlbumInfo>, IReturn<List<RemoteSearchResult>>
    {
    }

    [Route("/Items/RemoteSearch/Person", "POST")]
    [Authenticated(Roles = "Admin")]
    public class GetPersonRemoteSearchResults : RemoteSearchQuery<PersonLookupInfo>, IReturn<List<RemoteSearchResult>>
    {
    }

    [Route("/Items/RemoteSearch/Image", "GET", Summary = "Gets a remote image")]
    public class GetRemoteSearchImage
    {
        [ApiMember(Name = "ImageUrl", Description = "The image url", IsRequired = true, DataType = "string", ParameterType = "query", Verb = "GET")]
        public string ImageUrl { get; set; }

        [ApiMember(Name = "ProviderName", IsRequired = true, DataType = "string", ParameterType = "query", Verb = "GET")]
        public string ProviderName { get; set; }
    }

    [Route("/Items/RemoteSearch/Apply/{Id}", "POST", Summary = "Applies search criteria to an item and refreshes metadata")]
    [Authenticated(Roles = "Admin")]
    public class ApplySearchCriteria : RemoteSearchResult, IReturnVoid
    {
        [ApiMember(Name = "Id", Description = "The item id", IsRequired = true, DataType = "string", ParameterType = "query", Verb = "POST")]
        public string Id { get; set; }

        [ApiMember(Name = "ReplaceAllImages", Description = "Whether or not to replace all images", IsRequired = false, DataType = "boolean", ParameterType = "query", Verb = "POST")]
        public bool ReplaceAllImages { get; set; }

        public ApplySearchCriteria()
        {
            ReplaceAllImages = true;
        }
    }

    public class ItemLookupService : BaseApiService
    {
        private readonly IProviderManager _providerManager;
        private readonly IServerApplicationPaths _appPaths;
        private readonly IFileSystem _fileSystem;
        private readonly ILibraryManager _libraryManager;
        private readonly IJsonSerializer _json;

        public ItemLookupService(IProviderManager providerManager, IServerApplicationPaths appPaths, IFileSystem fileSystem, ILibraryManager libraryManager, IJsonSerializer json)
        {
            _providerManager = providerManager;
            _appPaths = appPaths;
            _fileSystem = fileSystem;
            _libraryManager = libraryManager;
            _json = json;
        }

        public object Get(GetExternalIdInfos request)
        {
            var item = _libraryManager.GetItemById(request.Id);

            var infos = _providerManager.GetExternalIdInfos(item).ToList();

            return ToOptimizedResult(infos);
        }

        public object Post(GetMovieRemoteSearchResults request)
        {
            var result = _providerManager.GetRemoteSearchResults<Movie, MovieInfo>(request, CancellationToken.None).Result;

            return ToOptimizedResult(result);
        }

        public object Post(GetSeriesRemoteSearchResults request)
        {
            var result = _providerManager.GetRemoteSearchResults<Series, SeriesInfo>(request, CancellationToken.None).Result;

            return ToOptimizedResult(result);
        }

        public object Post(GetGameRemoteSearchResults request)
        {
            var result = _providerManager.GetRemoteSearchResults<Game, GameInfo>(request, CancellationToken.None).Result;

            return ToOptimizedResult(result);
        }

        public object Post(GetBoxSetRemoteSearchResults request)
        {
            var result = _providerManager.GetRemoteSearchResults<BoxSet, BoxSetInfo>(request, CancellationToken.None).Result;

            return ToOptimizedResult(result);
        }

        public object Post(GetPersonRemoteSearchResults request)
        {
            var result = _providerManager.GetRemoteSearchResults<Person, PersonLookupInfo>(request, CancellationToken.None).Result;

            return ToOptimizedResult(result);
        }

        public object Post(GetMusicAlbumRemoteSearchResults request)
        {
            var result = _providerManager.GetRemoteSearchResults<MusicAlbum, AlbumInfo>(request, CancellationToken.None).Result;

            return ToOptimizedResult(result);
        }

        public object Post(GetMusicArtistRemoteSearchResults request)
        {
            var result = _providerManager.GetRemoteSearchResults<MusicArtist, ArtistInfo>(request, CancellationToken.None).Result;

            return ToOptimizedResult(result);
        }

        public object Get(GetRemoteSearchImage request)
        {
            var result = GetRemoteImage(request).Result;

            return result;
        }

        public void Post(ApplySearchCriteria request)
        {
            var item = _libraryManager.GetItemById(new Guid(request.Id));

            //foreach (var key in request.ProviderIds)
            //{
            //    var value = key.Value;

            //    if (!string.IsNullOrWhiteSpace(value))
            //    {
            //        item.SetProviderId(key.Key, value);
            //    }
            //}
            Logger.Info("Setting provider id's to item {0}-{1}: {2}", item.Id, item.Name, _json.SerializeToString(request.ProviderIds));
            item.ProviderIds = request.ProviderIds;

			var task = _providerManager.RefreshFullItem(item, new MetadataRefreshOptions(_fileSystem)
            {
                MetadataRefreshMode = MetadataRefreshMode.FullRefresh,
                ImageRefreshMode = ImageRefreshMode.FullRefresh,
                ReplaceAllMetadata = true,
                ReplaceAllImages = request.ReplaceAllImages

            }, CancellationToken.None);
            Task.WaitAll(task);
        }

        /// <summary>
        /// Gets the remote image.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>Task{System.Object}.</returns>
        private async Task<object> GetRemoteImage(GetRemoteSearchImage request)
        {
            var urlHash = request.ImageUrl.GetMD5();
            var pointerCachePath = GetFullCachePath(urlHash.ToString());

            string contentPath;

            try
            {
                using (var reader = new StreamReader(pointerCachePath))
                {
                    contentPath = await reader.ReadToEndAsync().ConfigureAwait(false);
                }

				if (_fileSystem.FileExists(contentPath))
                {
                    return ToStaticFileResult(contentPath);
                }
            }
            catch (DirectoryNotFoundException)
            {
                // Means the file isn't cached yet
            }
            catch (FileNotFoundException)
            {
                // Means the file isn't cached yet
            }

            await DownloadImage(request.ProviderName, request.ImageUrl, urlHash, pointerCachePath).ConfigureAwait(false);

            // Read the pointer file again
            using (var reader = new StreamReader(pointerCachePath))
            {
                contentPath = await reader.ReadToEndAsync().ConfigureAwait(false);
            }

            return ToStaticFileResult(contentPath);
        }

        /// <summary>
        /// Downloads the image.
        /// </summary>
        /// <param name="providerName">Name of the provider.</param>
        /// <param name="url">The URL.</param>
        /// <param name="urlHash">The URL hash.</param>
        /// <param name="pointerCachePath">The pointer cache path.</param>
        /// <returns>Task.</returns>
        private async Task DownloadImage(string providerName, string url, Guid urlHash, string pointerCachePath)
        {
            var result = await _providerManager.GetSearchImage(providerName, url, CancellationToken.None).ConfigureAwait(false);

            var ext = result.ContentType.Split('/').Last();

            var fullCachePath = GetFullCachePath(urlHash + "." + ext);

			_fileSystem.CreateDirectory(Path.GetDirectoryName(fullCachePath));
            using (var stream = result.Content)
            {
                using (var filestream = _fileSystem.GetFileStream(fullCachePath, FileMode.Create, FileAccess.Write, FileShare.Read, true))
                {
                    await stream.CopyToAsync(filestream).ConfigureAwait(false);
                }
            }

			_fileSystem.CreateDirectory(Path.GetDirectoryName(pointerCachePath));
            using (var writer = new StreamWriter(pointerCachePath))
            {
                await writer.WriteAsync(fullCachePath).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Gets the full cache path.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns>System.String.</returns>
        private string GetFullCachePath(string filename)
        {
            return Path.Combine(_appPaths.CachePath, "remote-images", filename.Substring(0, 1), filename);
        }

    }
}
