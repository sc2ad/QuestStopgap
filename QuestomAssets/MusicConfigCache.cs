﻿using QuestomAssets.BeatSaber;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Collections.Specialized;

namespace QuestomAssets
{
    public class MusicConfigCache
    {
        //keyed on PlaylistID (aka PackID)
        public Dictionary<string, PlaylistAndSongs> PlaylistCache { get; } = new Dictionary<string, PlaylistAndSongs>();

        //keyed on SongID (aka LevelID)
        public Dictionary<string, SongAndPlaylist> SongCache { get; } = new Dictionary<string, SongAndPlaylist>();

        //we will see if this cache is enough of a performance boost to warrant the extra hassle of keeping it up to date
        // Collection is the pack collection we want to add our custom songs to
        public MusicConfigCache(BeatmapLevelPackCollection collection)
        {
            Log.LogMsg("Building cache...");
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                PlaylistCache.Clear();
                SongCache.Clear();
                int plCtr = 0;
                
                
                foreach (var x in collection.BeatmapLevelPacks)
                {
                    if (PlaylistCache.ContainsKey(x.Object.PackID))
                    {
                        Log.LogErr($"Cache building: playlist ID {x.Object.PackID} exists multiple times in the {collection.Name} list!  Skipping redundant copies...");
                    }
                    else
                    {
                        var pns = new PlaylistAndSongs() { Playlist = x.Object, Order = plCtr };
                        int ctr = 0;
                        foreach (var y in x.Object.BeatmapLevelCollection.Object.BeatmapLevels)
                        {
                            if (pns.Songs.ContainsKey(y.Object.LevelID))
                            {
                                Log.LogErr($"Cache building: song ID {y.Object.LevelID} exists multiple times in playlist {x.Object.PackID}!");
                            }
                            else
                            {
                                pns.Songs.Add(y.Object.LevelID, new OrderedSong() { Song = y.Object, Order = ctr });
                            }
                            if (SongCache.ContainsKey(y.Object.LevelID))
                            {
                                Log.LogErr($"Cache building: cannot add song ID {y.Object.LevelID} in playlist ID {x.Object.PackID} because it already exists in {SongCache[y.Object.LevelID].Playlist.PackID}!");
                            }
                            else
                            {
                                SongCache.Add(y.Object.LevelID, new SongAndPlaylist() { Song = y.Object, Playlist = x.Object });
                            }
                            ctr++;
                        }
                        PlaylistCache.Add(x.Object.PackID, pns);
                        plCtr++;
                    }
                }
                sw.Stop();
                Log.LogMsg($"Building cache took {sw.ElapsedMilliseconds}ms");
            }
            catch (Exception ex)
            {
                Log.LogErr("Exception building cache!", ex);
                throw;
            }
        }

        public class PlaylistAndSongs
        {
            public int Order { get; set; }
            public BeatmapLevelPackObject Playlist { get; set; }
            public Dictionary<string, OrderedSong> Songs = new Dictionary<string, OrderedSong>();
        }

        public class SongAndPlaylist
        {
            public BeatmapLevelPackObject Playlist { get; set; }
            public BeatmapLevelDataObject Song { get; set; }
        }

        public class OrderedSong
        {
            public int Order { get; set; }
            public BeatmapLevelDataObject Song { get; set; }
        }
    }
}
