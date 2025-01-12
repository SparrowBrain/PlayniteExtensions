﻿using Playnite.SDK;
using Playnite.SDK.Models;
using PlayniteExtensions.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Rawg.Common
{
    public static class RawgMetadataHelper
    {
        private static Regex yearRegex = new Regex(@" \([0-9]{4}\)$", RegexOptions.Compiled);

        public static string StripYear(string gameName)
        {
            return yearRegex.Replace(gameName, string.Empty);
        }

        public static string NormalizeNameForComparison(string gameName)
        {
            return StripYear(gameName).Deflate();
        }

        public static MetadataProperty GetPlatform(RawgPlatform platform)
        {
            switch (platform.Platform.Slug)
            {
                case "pc":
                    return new MetadataSpecProperty("pc_windows"); //assumption that doesn't work for dos games, but for those there's often no data to extrapolate a proper specid
                case "linux":
                    return new MetadataSpecProperty("pc_linux");

                case "xbox-old":
                    return new MetadataSpecProperty("xbox");
                case "xbox360":
                    return new MetadataSpecProperty("xbox360");
                case "xbox-one":
                    return new MetadataSpecProperty("xbox_one");
                case "xbox-series-x":
                    return new MetadataSpecProperty("xbox_series");

                case "playstation1":
                case "playstation2":
                case "playstation3":
                case "playstation4":
                case "playstation5":
                case "psp":
                    return new MetadataSpecProperty($"sony_" + platform.Platform.Slug);
                case "ps-vita":
                    return new MetadataSpecProperty("sony_vita");

                case "nes":
                    return new MetadataSpecProperty("nintendo_nes");
                case "snes":
                    return new MetadataSpecProperty("nintendo_super_nes");
                case "nintendo-ds":
                    return new MetadataSpecProperty("nintendo_ds");
                case "nintendo-3ds":
                    return new MetadataSpecProperty("nintendo_3ds");
                case "nintendo-switch":
                    return new MetadataSpecProperty("nintendo_switch");
                case "nintendo-64":
                    return new MetadataSpecProperty("nintendo_64");
                case "gamecube":
                    return new MetadataSpecProperty("nintendo_gamecube");
                case "wii":
                    return new MetadataSpecProperty("nintendo_wii");
                case "wii-u":
                    return new MetadataSpecProperty("nintendo_wiiu");
                case "game-boy":
                case "game-boy-color":
                case "game-boy-advance":
                    return new MetadataSpecProperty("nintendo_" + platform.Platform.Slug.Replace("-", ""));
                case "macintosh":
                    return new MetadataSpecProperty(platform.Platform.Slug);
                case "apple-ii":
                    return new MetadataSpecProperty("apple_2");

                case "jaguar":
                    return new MetadataSpecProperty("atari_jaguar");
                case "commodore-amiga":
                case "atari-2600":
                case "atari-5200":
                case "atari-7800":
                case "atari-8-bit":
                case "atari-st":
                case "atari-lynx":
                case "sega-saturn":
                case "sega-cd":
                case "sega-32x":
                    return new MetadataSpecProperty(platform.Platform.Slug.Replace("-", "_"));

                case "genesis":
                    return new MetadataSpecProperty("sega_genesis");
                case "sega-master-system":
                    return new MetadataSpecProperty("sega_mastersystem");
                case "dreamcast":
                    return new MetadataSpecProperty("sega_dreamcast");
                case "game-gear":
                    return new MetadataSpecProperty("sega_gamegear");
                case "3do":
                    return new MetadataSpecProperty("3do");

                case "atari-xegs":
                case "atari-flashback":
                case "ios":
                case "android":
                case "macos":
                case "neogeo":
                case "nintendo-dsi":
                default:
                    return new MetadataNameProperty(platform.Platform.Name);
            }
        }

        public static ReleaseDate? ParseReleaseDate(RawgGameBase data, ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(data.Released))
                return null;

            var segments = data.Released.Split('-');
            List<int> numberSegments;
            try
            {
                numberSegments = segments.Select(int.Parse).ToList();
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"Could not parse release date <{data.Released}> for {data.Name}");
                return null;
            }

            switch (numberSegments.Count)
            {
                case 1:
                    return new ReleaseDate(numberSegments[0]);
                case 2:
                    return new ReleaseDate(numberSegments[0], numberSegments[1]);
                case 3:
                    return new ReleaseDate(numberSegments[0], numberSegments[1], numberSegments[2]);
                default:
                    logger.Warn($"Could not parse release date <{data.Released}> for {data.Name}");
                    return null;
            }
        }

        public static int? ParseUserScore(float? userScore)
        {
            if (userScore == null || userScore == 0)
                return null;

            return Convert.ToInt32(userScore.Value * 20);
        }

        public static Link GetRawgLink(RawgGameBase data)
        {
            return new Link("RAWG", $"https://rawg.io/games/{data.Id}");
        }

        public static List<Link> GetLinks(RawgGameDetails data)
        {
            var links = new List<Link>();
            links.Add(GetRawgLink(data));

            if (!string.IsNullOrWhiteSpace(data.Website))
                links.Add(new Link("Website", data.Website));

            if (!string.IsNullOrWhiteSpace(data.RedditUrl))
                links.Add(new Link("Reddit", data.RedditUrl));

            return links;
        }

        public static RawgGameBase GetExactTitleMatch(Game game, RawgApiClient client)
        {
            if (string.IsNullOrWhiteSpace(game?.Name))
                return null;

            string searchString;
            if (game.ReleaseYear.HasValue)
                searchString = $"{game.Name} {game.ReleaseYear}";
            else
                searchString = game.Name;
            var result = client.SearchGames(searchString);
            if (result?.Results == null)
                return null;

            var gameName = NormalizeNameForComparison(game.Name);
            foreach (var g in result.Results)
            {
                var resultName = NormalizeNameForComparison(g.Name);
                if (gameName.Equals(resultName, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (game.Links == null)
                        game.Links = new System.Collections.ObjectModel.ObservableCollection<Link>();
                    else
                        game.Links = new System.Collections.ObjectModel.ObservableCollection<Link>(game.Links);

                    game.Links.Add(GetRawgLink(g));
                    return g;
                }
            }
            return null;
        }
    }
}
