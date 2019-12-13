﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuestomAssets.AssetsChanger;

namespace QuestomAssets.BeatSaber
{
    public sealed class BeatmapLevelPackObject : MonoBehaviourObject, INeedAssetsMetadata
    {

        public BeatmapLevelPackObject(AssetsFile assetsFile) : base(assetsFile, assetsFile.Manager.GetScriptObject("BeatmapLevelPackSO"))
        { }

        public BeatmapLevelPackObject(IObjectInfo<AssetsObject> objectInfo, AssetsReader reader) : base(objectInfo)
        {
            Parse(reader);
        }

        public string PackID { get; set; }

        public string PackName { get; set; }

        public string ShortPackName { get; set; }

        public ISmartPtr<SpriteObject> CoverImage { get; set; }

        public ISmartPtr<BeatmapLevelCollectionObject> BeatmapLevelCollection { get; set; }

        protected override void WriteObject(AssetsWriter writer)
        {
            base.WriteBase(writer);
            writer.Write(PackID);
            writer.Write(PackName);
            writer.Write(ShortPackName);
            CoverImage.Write(writer);
            BeatmapLevelCollection.Write(writer);
        }

        public override void Parse(AssetsReader reader)
        {
            base.ParseBase(reader);
            PackID = reader.ReadString();
            PackName = reader.ReadString();
            ShortPackName = reader.ReadString();
            CoverImage = SmartPtr<SpriteObject>.Read(ObjectInfo.ParentFile, this, reader);
            BeatmapLevelCollection = SmartPtr<BeatmapLevelCollectionObject>.Read(ObjectInfo.ParentFile, this, reader);
        }

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
