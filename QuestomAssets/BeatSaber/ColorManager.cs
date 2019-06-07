﻿using Newtonsoft.Json;
using QuestomAssets.AssetsChanger;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuestomAssets.BeatSaber
{
    public sealed class ColorManager : MonoBehaviourObject, INeedAssetsMetadata
    {
        public ISmartPtr<MonoBehaviourObject> PlayerModel { get; set; }
        public ISmartPtr<MonoBehaviourObject> ColorA { get; set; }
        public ISmartPtr<MonoBehaviourObject> ColorB { get; set; }

        public ColorManager(IObjectInfo<AssetsObject> objectInfo, AssetsReader reader) : base(objectInfo, reader)
        {
            Parse(reader);
        }

        public ColorManager(IObjectInfo<AssetsObject> objectInfo) : base(objectInfo)
        { }

        public ColorManager(AssetsFile assetsFile) : base(assetsFile, assetsFile.Manager.GetScriptObject("ColorManager"))
        { }

        protected override void Parse(AssetsReader reader)
        {
            base.Parse(reader);
            PlayerModel = SmartPtr<MonoBehaviourObject>.Read(ObjectInfo.ParentFile, this, reader);
            ColorA = SmartPtr<MonoBehaviourObject>.Read(ObjectInfo.ParentFile, this, reader);
            ColorB = SmartPtr<MonoBehaviourObject>.Read(ObjectInfo.ParentFile, this, reader);
        }

        public override void Write(AssetsWriter writer)
        {
            WriteBase(writer);
            PlayerModel.Write(writer);
            ColorA.Write(writer);
            ColorB.Write(writer);
        }
    }
}