﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


    public class DifficultyBeatmapSO
    {

        public Difficulty _difficulty;
        public int _difficultyRank;
        public float _noteJumpMovementSpeed;
        public int _noteJumpStartBeatOffset;

        public BeatmapDataSO _beatmapData;

        public void Write(AlignedStream s)
        {
            s.Write((int)_difficulty);
            s.Write(_difficultyRank);
            s.Write(_noteJumpMovementSpeed);
            s.Write(_noteJumpStartBeatOffset);
            s.Write(_beatmapData.Ptr);
        }
    }

