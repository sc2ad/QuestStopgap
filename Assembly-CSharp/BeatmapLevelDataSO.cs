﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.IO;

public class BeatmapLevelDataSO
{
    public BeatmapLevelDataSO()
    {
        _difficultyBeatmapSets = new List<DifficultyBeatmapSetSO>();
    }
    public string _levelID
    {
        get
        {
            //this probably isn't safe to use as a songID
            return new string(_songName.Where(c => char.IsLetter(c)).ToArray());
        }
    }

    public string _songName;
    public string _songSubName;
    public string _songAuthorName;
    public string _levelAuthorName;
    [JsonIgnore]
    public UPtr _audioClip;
    public float _beatsPerMinute;
    public float _songTimeOffset;
    public float _shuffle;
    public float _shufflePeriod;
    public float _previewStartTime;
    public float _previewDuration;
    [JsonIgnore]
    public UPtr _coverImageTexture2D;
    [JsonIgnore]
    public UPtr _environmentSceneInfo;
    public List<DifficultyBeatmapSetSO> _difficultyBeatmapSets { get; private set; }


    //json properties
    public string _songFilename;
    public string _coverImageFilename;
    public string _environmentName;

    public void Write(AlignedStream s)
    {
        s.Write(_levelID);
        s.Write(_songName);
        s.Write(_songSubName);
        s.Write(_songAuthorName);
        s.Write(_levelAuthorName);
        _audioClip.Write(s);
        s.Write(_beatsPerMinute);
        s.Write(_songTimeOffset);
        s.Write(_shuffle);
        s.Write(_shufflePeriod);
        s.Write(_previewStartTime);
        s.Write(_previewDuration);
        _coverImageTexture2D.Write(s);
        _environmentSceneInfo.Write(s);
        s.Write(_difficultyBeatmapSets.Count);
        foreach (var dbs in _difficultyBeatmapSets)
        {
            dbs.Write(s);
        }
    }

    

}



