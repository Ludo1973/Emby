﻿using MediaBrowser.Model.Entities;
using System;
using System.Collections.Generic;
using MediaBrowser.Model.Configuration;

namespace MediaBrowser.Controller.Entities
{
    public class InternalItemsQuery
    {
        public bool Recursive { get; set; }

        public int? StartIndex { get; set; }

        public int? Limit { get; set; }

        public string[] SortBy { get; set; }

        public SortOrder SortOrder { get; set; }

        public User User { get; set; }

        public Func<BaseItem, bool> Filter { get; set; }

        public bool? IsFolder { get; set; }
        public bool? IsFavorite { get; set; }
        public bool? IsFavoriteOrLiked { get; set; }
        public bool? IsLiked { get; set; }
        public bool? IsPlayed { get; set; }
        public bool? IsResumable { get; set; }
        public bool? IncludeItemsByName { get; set; }

        public string[] MediaTypes { get; set; }
        public string[] IncludeItemTypes { get; set; }
        public string[] ExcludeItemTypes { get; set; }
        public string[] ExcludeTags { get; set; }
        public string[] Genres { get; set; }

        public bool? IsMissing { get; set; }
        public bool? IsUnaired { get; set; }
        public bool? IsVirtualUnaired { get; set; }
        public bool? CollapseBoxSetItems { get; set; }

        public string NameStartsWithOrGreater { get; set; }
        public string NameStartsWith { get; set; }
        public string NameLessThan { get; set; }
        public string NameContains { get; set; }

        public string Path { get; set; }
        
        public string Person { get; set; }
        public string[] PersonIds { get; set; }
        public string[] ItemIds { get; set; }
        public string AdjacentTo { get; set; }
        public string[] PersonTypes { get; set; }

        public bool? Is3D { get; set; }
        public bool? IsHD { get; set; }
        public bool? IsInBoxSet { get; set; }
        public bool? IsLocked { get; set; }
        public bool? IsPlaceHolder { get; set; }
        public bool? IsYearMismatched { get; set; }

        public bool? HasImdbId { get; set; }
        public bool? HasOverview { get; set; }
        public bool? HasTmdbId { get; set; }
        public bool? HasOfficialRating { get; set; }
        public bool? HasTvdbId { get; set; }
        public bool? HasThemeSong { get; set; }
        public bool? HasThemeVideo { get; set; }
        public bool? HasSubtitles { get; set; }
        public bool? HasSpecialFeature { get; set; }
        public bool? HasTrailer { get; set; }
        public bool? HasParentalRating { get; set; }

        public string[] Studios { get; set; }
        public string[] StudioIds { get; set; }
        public string[] GenreIds { get; set; }
        public ImageType[] ImageTypes { get; set; }
        public VideoType[] VideoTypes { get; set; }
        public UnratedItem[] BlockUnratedItems { get; set; }
        public int[] Years { get; set; }
        public string[] Tags { get; set; }
        public string[] OfficialRatings { get; set; }

        public DateTime? MinPremiereDate { get; set; }
        public DateTime? MinStartDate { get; set; }
        public DateTime? MaxStartDate { get; set; }
        public DateTime? MinEndDate { get; set; }
        public DateTime? MaxEndDate { get; set; }
        public bool? IsAiring { get; set; }

        public bool? IsMovie { get; set; }
        public bool? IsSports { get; set; }
        public bool? IsKids { get; set; }

        public int? MinPlayers { get; set; }
        public int? MaxPlayers { get; set; }
        public int? MinIndexNumber { get; set; }
        public double? MinCriticRating { get; set; }
        public double? MinCommunityRating { get; set; }

        public string[] ChannelIds { get; set; }

        internal List<Guid> ItemIdsFromPersonFilters { get; set; }
        public int? MaxParentalRating { get; set; }

        public bool? IsCurrentSchema { get; set; }
        public bool? HasDeadParentId { get; set; }
        public bool? IsOffline { get; set; }
        public LocationType? LocationType { get; set; }

        public Guid? ParentId { get; set; }
        public string[] AncestorIds { get; set; }
        public string[] TopParentIds { get; set; }

        public LocationType[] ExcludeLocationTypes { get; set; }
        public string[] PresetViews { get; set; }

        public InternalItemsQuery()
        {
            BlockUnratedItems = new UnratedItem[] { };
            Tags = new string[] { };
            OfficialRatings = new string[] { };
            SortBy = new string[] { };
            MediaTypes = new string[] { };
            IncludeItemTypes = new string[] { };
            ExcludeItemTypes = new string[] { };
            Genres = new string[] { };
            Studios = new string[] { };
            StudioIds = new string[] { };
            GenreIds = new string[] { };
            ImageTypes = new ImageType[] { };
            VideoTypes = new VideoType[] { };
            Years = new int[] { };
            PersonTypes = new string[] { };
            PersonIds = new string[] { };
            ChannelIds = new string[] { };
            ItemIds = new string[] { };
            AncestorIds = new string[] { };
            TopParentIds = new string[] { };
            ExcludeTags = new string[] { };
            ExcludeLocationTypes = new LocationType[] { };
            PresetViews = new string[] { };
        }

        public InternalItemsQuery(User user)
            : this()
        {
            if (user != null)
            {
                var policy = user.Policy;
                MaxParentalRating = policy.MaxParentalRating;

                if (policy.MaxParentalRating.HasValue)
                {
                    BlockUnratedItems = policy.BlockUnratedItems;
                }

                ExcludeTags = policy.BlockedTags;

                User = user;
            }
        }
    }
}
