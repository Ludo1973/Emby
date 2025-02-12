﻿using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Users;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommonIO;
using MediaBrowser.Common.IO;

namespace MediaBrowser.Controller.Entities.Movies
{
    /// <summary>
    /// Class Movie
    /// </summary>
    public class Movie : Video, IHasCriticRating, IHasSpecialFeatures, IHasProductionLocations, IHasBudget, IHasKeywords, IHasTrailers, IHasThemeMedia, IHasTaglines, IHasAwards, IHasMetascore, IHasLookupInfo<MovieInfo>, ISupportsBoxSetGrouping, IHasOriginalTitle
    {
        public List<Guid> SpecialFeatureIds { get; set; }

        public string OriginalTitle { get; set; }

        public List<Guid> ThemeSongIds { get; set; }
        public List<Guid> ThemeVideoIds { get; set; }
        public List<string> ProductionLocations { get; set; }

        public Movie()
        {
            SpecialFeatureIds = new List<Guid>();
            RemoteTrailers = new List<MediaUrl>();
            LocalTrailerIds = new List<Guid>();
            RemoteTrailerIds = new List<Guid>();
            ThemeSongIds = new List<Guid>();
            ThemeVideoIds = new List<Guid>();
            Taglines = new List<string>();
            Keywords = new List<string>();
            ProductionLocations = new List<string>();
        }

        public string AwardSummary { get; set; }

        public float? Metascore { get; set; }

        public List<Guid> LocalTrailerIds { get; set; }
        public List<Guid> RemoteTrailerIds { get; set; }
        public List<string> Keywords { get; set; }

        public List<MediaUrl> RemoteTrailers { get; set; }

        /// <summary>
        /// Gets or sets the taglines.
        /// </summary>
        /// <value>The taglines.</value>
        public List<string> Taglines { get; set; }

        /// <summary>
        /// Gets or sets the budget.
        /// </summary>
        /// <value>The budget.</value>
        public double? Budget { get; set; }

        /// <summary>
        /// Gets or sets the revenue.
        /// </summary>
        /// <value>The revenue.</value>
        public double? Revenue { get; set; }

        /// <summary>
        /// Gets or sets the critic rating.
        /// </summary>
        /// <value>The critic rating.</value>
        public float? CriticRating { get; set; }

        /// <summary>
        /// Gets or sets the critic rating summary.
        /// </summary>
        /// <value>The critic rating summary.</value>
        public string CriticRatingSummary { get; set; }

        /// <summary>
        /// Gets or sets the name of the TMDB collection.
        /// </summary>
        /// <value>The name of the TMDB collection.</value>
        public string TmdbCollectionName { get; set; }

        /// <summary>
        /// Gets the trailer ids.
        /// </summary>
        /// <returns>List&lt;Guid&gt;.</returns>
        public List<Guid> GetTrailerIds()
        {
            var list = LocalTrailerIds.ToList();
            list.AddRange(RemoteTrailerIds);
            return list;
        }

        /// <summary>
        /// Gets the user data key.
        /// </summary>
        /// <returns>System.String.</returns>
        protected override string CreateUserDataKey()
        {
            var key = GetMovieUserDataKey(this);

            if (string.IsNullOrWhiteSpace(key))
            {
                key = base.CreateUserDataKey();
            }

            return key;
        }

        public static string GetMovieUserDataKey(BaseItem movie)
        {
            var key = movie.GetProviderId(MetadataProviders.Tmdb);

            if (string.IsNullOrWhiteSpace(key))
            {
                key = movie.GetProviderId(MetadataProviders.Imdb);
            }

            return key;
        }

        protected override async Task<bool> RefreshedOwnedItems(MetadataRefreshOptions options, List<FileSystemMetadata> fileSystemChildren, CancellationToken cancellationToken)
        {
            var hasChanges = await base.RefreshedOwnedItems(options, fileSystemChildren, cancellationToken).ConfigureAwait(false);

            // Must have a parent to have special features
            // In other words, it must be part of the Parent/Child tree
            if (LocationType == LocationType.FileSystem && GetParent() != null && !IsInMixedFolder)
            {
                var specialFeaturesChanged = await RefreshSpecialFeatures(options, fileSystemChildren, cancellationToken).ConfigureAwait(false);

                if (specialFeaturesChanged)
                {
                    hasChanges = true;
                }
            }

            return hasChanges;
        }

        private async Task<bool> RefreshSpecialFeatures(MetadataRefreshOptions options, List<FileSystemMetadata> fileSystemChildren, CancellationToken cancellationToken)
        {
            var newItems = LibraryManager.FindExtras(this, fileSystemChildren, options.DirectoryService).ToList();
            var newItemIds = newItems.Select(i => i.Id).ToList();

            var itemsChanged = !SpecialFeatureIds.SequenceEqual(newItemIds);

            var tasks = newItems.Select(i => i.RefreshMetadata(options, cancellationToken));

            await Task.WhenAll(tasks).ConfigureAwait(false);

            SpecialFeatureIds = newItemIds;

            return itemsChanged;
        }

        public override UnratedItem GetBlockUnratedType()
        {
            return UnratedItem.Movie;
        }

        public MovieInfo GetLookupInfo()
        {
            var info = GetItemLookupInfo<MovieInfo>();

            if (!IsInMixedFolder)
            {
                info.Name = System.IO.Path.GetFileName(ContainingFolderPath);
            }

            return info;
        }

        public override bool BeforeMetadataRefresh()
        {
            var hasChanges = base.BeforeMetadataRefresh();

            if (!ProductionYear.HasValue)
            {
                var info = LibraryManager.ParseName(Name);

                var yearInName = info.Year;

                if (yearInName.HasValue)
                {
                    ProductionYear = yearInName;
                    hasChanges = true;
                }
                else
                {
                    // Try to get the year from the folder name
                    if (!IsInMixedFolder)
                    {
                        info = LibraryManager.ParseName(System.IO.Path.GetFileName(ContainingFolderPath));

                        yearInName = info.Year;

                        if (yearInName.HasValue)
                        {
                            ProductionYear = yearInName;
                            hasChanges = true;
                        }
                    }
                }
            }

            return hasChanges;
        }
    }
}
