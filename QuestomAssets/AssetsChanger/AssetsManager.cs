﻿using QuestomAssets.BeatSaber;
using QuestomAssets.Mods.Assets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace QuestomAssets.AssetsChanger
{
    public class AssetsManager : IDisposable
    {
        public Dictionary<string, Type> ClassNameToTypes { get; private set; } = new Dictionary<string, Type>();
        private IFileProvider _fileProvider;
        private string _assetsRootPath;

        public AssetsManager(IFileProvider fileProvider, string assetsRootPath, Dictionary<string, Type> classNameToTypes, string version)
        {
            _fileProvider = fileProvider;
            _assetsRootPath = assetsRootPath;
            LazyLoad = true;
            ClassNameToTypes = classNameToTypes;
            ForceLoadAllFiles = false;
            BeatSaberVersion = version;
        }

        public bool HasChanges
        {
            get
            {
                return _openAssetsFiles.Any(x=> x.Value.HasChanges);
            }
        }

        private Dictionary<string, AssetsFile> _openAssetsFiles = new Dictionary<string, AssetsFile>();
        public bool LazyLoad { get; private set; }
        public bool ForceLoadAllFiles { get; private set; }
        public List<AssetsFile> OpenFiles
        {
            get
            {
                return _openAssetsFiles.Values.ToList();
            }
        }
        public string BeatSaberVersion { get; }

        //if file ends in .split0 yes
        //if file ends in .assets yes
        //if file has no extension yes

        public List<string> FindAndLoadAllAssets()
        {
            List<string> loadedFiles = new List<string>();
            var foundFiles = _fileProvider.FindFiles(_assetsRootPath + "*");
            List<string> tryFiles = new List<string>();
            foreach (var foundFile in foundFiles)
            {
                var filename = foundFile.Substring(_assetsRootPath.Length);

                if (filename.Any(x => x == '/'))
                {
                    if (!filename.Substring(filename.LastIndexOf("/")).Contains("."))
                    {
                        tryFiles.Add(filename);
                        continue;
                    }
                }
                else if (!filename.Contains("."))
                {
                    tryFiles.Add(filename);
                    continue;
                }

                if (filename.ToLower().EndsWith(".split0"))
                {
                    filename = filename.Substring(0, filename.Length - ".split0".Length);
                    tryFiles.Add(filename);
                    continue;
                }

                if (filename.ToLower().EndsWith(".assets"))
                {
                    tryFiles.Add(filename);
                    continue;
                }
            }

            

            foreach (var tryFile in tryFiles)
            {
                if (_openAssetsFiles.ContainsKey(tryFile.ToLower()))
                {
                    loadedFiles.Add(tryFile);
                    continue;
                }
                AssetsFile file;
                if (TryGetAssetsFile(tryFile, out file))
                    loadedFiles.Add(tryFile);
            }
            return loadedFiles;
        }


        public AssetsFile GetAssetsFile(string assetsFilename)
        {
            if (assetsFilename.LastIndexOf("}}") > 2 + assetsFilename.LastIndexOf("{{"))
            {
                // Special case enum file
                LocatorEnum result;
                var enumVal = assetsFilename.Substring(assetsFilename.LastIndexOf("{{") + 2, assetsFilename.LastIndexOf("}}") - assetsFilename.LastIndexOf("{{") - 2);
                Log.LogMsg($"Found LocatorEnum with string: {enumVal}. Attempting to parse...");
                if (Enum.TryParse(enumVal, out result))
                {
                    // Replace the enum string
                    assetsFilename = assetsFilename.Replace("{{" + enumVal + "}}", LocatorEnumHelper.GetFile(result, BeatSaberVersion));
                }
                Log.LogMsg($"Parsed enumVal: {result} Result: {LocatorEnumHelper.GetFile(result, BeatSaberVersion)} File: {assetsFilename}");
            }
            lock (_openAssetsFiles)
            {
                if (_openAssetsFiles.ContainsKey(assetsFilename.ToLower()))
                    return _openAssetsFiles[assetsFilename.ToLower()];
                AssetsFile assetsFile = new AssetsFile(this, _fileProvider, _assetsRootPath, assetsFilename, false);
                _openAssetsFiles.Add(assetsFilename.ToLower(), assetsFile);
                assetsFile.LoadData();
                return assetsFile;
            }
        }

        public bool TryGetAssetsFile(string assetsFilename, out AssetsFile loadedFile)
        {
            lock (_openAssetsFiles)
            {
                if (_openAssetsFiles.ContainsKey(assetsFilename))
                {
                    loadedFile = _openAssetsFiles[assetsFilename];
                    return true;
                }
                AssetsFile assetsFile = null;
                try
                {
                    assetsFile = new AssetsFile(this, _fileProvider, _assetsRootPath, assetsFilename, false);
                }
                catch
                {
                    loadedFile = null;
                    return false;
                }
                _openAssetsFiles.Add(assetsFilename, assetsFile);
                assetsFile.LoadData();
                loadedFile = assetsFile;
                return true;
            }
        }

        public void WriteAllOpenAssets()
        {
            lock (_openAssetsFiles)
            {
                lock (this)
                {
                    foreach (var assetsFileName in _openAssetsFiles.Keys.ToList())
                    {
                        var assetsFile = _openAssetsFiles[assetsFileName];
                        if (assetsFile.HasChanges)
                        {
                            Log.LogMsg($"File {assetsFileName} has changed, writing new contents.");
                            try
                            {
                                assetsFile.Write();
                            }
                            catch (Exception ex)
                            {
                                Log.LogErr($"Exception writing assets file {assetsFileName}", ex);
                                throw;
                            }
                        }
                    }
                }
            }
        }

        public void CloseAllOpenAssets()
        {
            lock (this)
            {
                foreach (var assetsFileName in _openAssetsFiles.Keys.ToList())
                {
                    var assetsFile = _openAssetsFiles[assetsFileName];
                    _openAssetsFiles[assetsFileName].Dispose();
                    _openAssetsFiles.Remove(assetsFileName);
                }
            }
        }

        private Dictionary<string, MonoScriptObject> _classCache = new Dictionary<string, MonoScriptObject>();
        private Dictionary<Guid, MonoScriptObject> _hashClassCache = new Dictionary<Guid, MonoScriptObject>();
        public MonoScriptObject GetScriptObject(string className)
        {
            //TODO: change this logic over to like GetScriptObject
            if (_classCache.ContainsKey(className))
                return _classCache[className];
            var ggm = GetAssetsFile("globalgamemanagers.assets");
            var list = ggm.FindAssets<MonoScriptObject>(x => x.Object.Name == className);
            var classObj = list.FirstOrDefault();
            if (classObj == null)
                throw new Exception($"Unable to find a script with type name {className}!");
            _classCache.Add(className, classObj.Object);
            return classObj.Object;
        }
        public MonoScriptObject GetScriptObject(Guid propertiesHash)
        {
            if (_hashClassCache.ContainsKey(propertiesHash))
                return _hashClassCache[propertiesHash];

            IObjectInfo<MonoScriptObject> classObj = null;

            //check any open files
            foreach (var file in OpenFiles)
            {
                classObj = file.FindAsset<MonoScriptObject>(x => x.Object.PropertiesHash == propertiesHash);
                if (classObj != null)
                    break;
            }
            
            //TODO: decide if a mass search is in order (i.e. follow the tree of all external files)

            if (classObj == null)
            {
                Log.LogErr($"WARNING! Did not find the monoscript object for {propertiesHash}!");
                //throw new Exception($"Unable to find a script with type hash {propertiesHash}!");
                return null;
            }
            
            _hashClassCache.Add(propertiesHash, classObj.Object);
            return classObj.Object;
        }

        public IObjectInfo<T> MassFirstAsset<T>(Func<IObjectInfo<T>, bool> filter, bool deepSearch = true) where T : AssetsObject
        {
            return MassFindAssets(filter, deepSearch).First();
        }

        public IObjectInfo<T> MassFirstOrDefaultAsset<T>(Func<IObjectInfo<T>, bool> filter, bool deepSearch = true) where T : AssetsObject
        {
            return MassFindAssets(filter, deepSearch).FirstOrDefault();
        }

        private IEnumerable<IObjectInfo<T>> MassFindAssets<T>(AssetsFile file, Func<IObjectInfo<T>, bool> filter, bool deepSearch, List<AssetsFile> searched, List<AssetsFile> deepSearched) where T : AssetsObject
        {
            if (!searched.Contains(file))
            {
                searched.Add(file);
                foreach (var res in file.FindAssets(filter))
                    yield return res;
            }
            if (deepSearch)
            {
                foreach (var extFile in file.Metadata.ExternalFiles)
                {
                    var ext = GetAssetsFile(extFile.FileName);
                    if (!deepSearched.Contains(ext))
                    {
                        deepSearched.Add(ext);
                        foreach (var res in MassFindAssets(ext, filter, deepSearch, searched, deepSearched))
                            yield return res;
                    }

                }
            }
            yield break;
        }
        public IEnumerable<IObjectInfo<T>> MassFindAssets<T>(Func<IObjectInfo<T>, bool> filter, bool deepSearch = true) where T : AssetsObject
        {
            List<AssetsFile> searched = new List<AssetsFile>();
            List<AssetsFile> deepSearched = new List<AssetsFile>();
            //do a quick pass on the open assts files so that if we find one and stop at that, we don't iterate them all
            foreach (var file in _openAssetsFiles.Values.ToList())
            {
                if (!searched.Contains(file))
                {
                    searched.Add(file);
                    foreach (var res in file.FindAssets(filter))
                    {
                        yield return res;
                    }
                }
            }

            if (deepSearch)
            {
                //now do a deep search
                foreach (var file in _openAssetsFiles.Values.ToList())
                    foreach (var res in MassFindAssets(file, filter, true, searched, deepSearched))
                        yield return res;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.CloseAllOpenAssets();
                }

                disposedValue = true;
            }
        }


        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
