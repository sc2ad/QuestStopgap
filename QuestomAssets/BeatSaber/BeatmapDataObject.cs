﻿using QuestomAssets.AssetsChanger;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Newtonsoft.Json.Linq;

namespace QuestomAssets.BeatSaber
{
    public sealed class BeatmapDataObject: MonoBehaviourObject
    {
        public BeatmapDataObject(IObjectInfo<AssetsObject> objectInfo, AssetsReader reader) : base(objectInfo)
        {
            Parse(reader);
        }

        public BeatmapDataObject(AssetsFile assetsFile) : base(assetsFile, assetsFile.Manager.GetScriptObject("BeatmapDataSO"))
        { }

        public override void Parse(AssetsReader reader)
        {
            base.ParseBase(reader);
            JsonData = reader.ReadString();
            reader.AlignTo(4);
        }

        protected override void WriteObject(AssetsWriter writer)
        {
            base.WriteBase(writer);
            writer.Write(JsonData);
            writer.AlignTo(4);
        }
        private string _jsonData;
        [JsonProperty("_jsonData")]
        public string JsonData
        {
            get
            {
                return _jsonData;
            }
            set
            {
                var json = value;
                _jsonData = value;
            }
        }

        [JsonProperty("_signatureBytes")]
        public byte[] SignatureBytes { get; set; } = new byte[128];

        [JsonProperty("_projectedData")]
        public byte[] ProjectedData { get; set; }

        [JsonProperty("_beatsPerMinute")]
        public Single BeatsPerMinute { get; set; }

        [JsonProperty("_shuffle")]
        public Single Shuffle { get; set; }

        [JsonProperty("_shufflePeriod")]
        public Single ShufflePeriod { get; set; }

        [JsonProperty("_hasRequiredDataForLoad")]
        public bool HasRequiredDataForLoad { get; set; }

        [System.ComponentModel.Browsable(false)]
        [Newtonsoft.Json.JsonIgnore]
        public override byte[] ScriptParametersData
        {
            get
            {
                throw new InvalidOperationException("Cannot access parameters data from this object.");
            }
            set
            {
                throw new InvalidOperationException("Cannot access parameters data from this object.");
            }
        }

    }
}

