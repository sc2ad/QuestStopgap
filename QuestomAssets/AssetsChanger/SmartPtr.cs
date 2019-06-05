﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace QuestomAssets.AssetsChanger
{
    public interface ISmartAction<out T> where T : AssetsObject
    {

    }
    public interface ISmartPtr<out T> where T : AssetsObject
    {
        IObjectInfo<AssetsObject> Owner { get; set; }
        Action UnsetOwnerProperty { get; set; }
        IObjectInfo<T> Target { get; }
        void Dispose();
        void WritePtr(AssetsWriter writer);
        bool Changes { get; set; }
    }

    public class SmartPtr<T> : ISmartPtr<T>, IDisposable where T : AssetsObject
    {
        public SmartPtr(IObjectInfo<T> target)
        {
            Target = target ?? throw new NullReferenceException("Target cannot be null");
            //TODO: not sure this is only ever called by new objects
            Changes = true;
            Target.ParentFile.AddPtrRef(this);
        }

        public static SmartPtr<T> Read(AssetsFile assetsFile, AssetsReader reader)
        {
            int fileID = reader.ReadInt32();
            Int64 pathID = reader.ReadInt64();
            if (fileID == 0 && pathID == 0)
                return null;
            
            SmartPtr<T> ptr = new SmartPtr<T>(assetsFile.GetObjectInfo<T>(fileID, pathID));
            //TODO: not sure this is only ever called by existing objects
            ptr.Changes = false;
            return ptr;
        }

        public bool Changes { get; set; }
        public Action UnsetOwnerProperty { get; set; }
        private IObjectInfo<AssetsObject> _owner;
        public IObjectInfo<AssetsObject> Owner
        {
            get
            {
                return _owner;
            }
            set
            {
                if (_owner != value)
                {
                    if (_owner != null)
                    {
                        _owner.ParentFile.RemovePtrRef(this);
                    }
                    if (value != null)
                    {
                        value.ParentFile.AddPtrRef(this);
                        value.ParentFile.GetOrAddExternalFileIDRef(Target.ParentFile);
                    }
                }
                _owner = value;
            }
        }

        public IObjectInfo<T> Target { get; private set; }


        //private int _fileID = 0;

        private int FileID
        {
            get
            {
                if (Owner == null)
                    throw new NotSupportedException("SmartPtr doesn't have an owner, getting FileID isn't supported!");
                if (Owner.ParentFile == Target.ParentFile)
                    return 0;

                return Owner.ParentFile.GetFileIDForFile(Target.ParentFile);
            }
        }

        //public ulong PathID
        //{
        //    get; set;
        //}

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (Owner != null)
                    {
                        Owner.ParentFile.RemovePtrRef(this);
                    }
                    if (Target != null)
                    {
                        Target.ParentFile.RemovePtrRef(this);
                    }
                    OwnerPropInfo = null;
                    Owner = null;
                    Target = null;
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

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

        public void WritePtr(AssetsWriter writer)
        {
            writer.Write(FileID);
            writer.Write(Owner.ObjectID);
        }



        //public ExternalFile File
        //{
        //    get
        //    {
        //        _object.ObjectInfo.Owner.Metadata.ExternalFiles
        //    }
        //}

        //public bool IsExternal
        //{
        //    get
        //    {

        //    }
        //}

        //private 

        //private T _object;


        //public T Object {
        //    get
        //    {
        //        return null;
        //    }
        //    set
        //    {

        //    }

        //}


    }

    public class UnreferencedExternalFile
    {
        public AssetsFile TargetFile { get; set; }
        public List<ISmartPtr<AssetsObject>> NeededBy { get; } = new List<ISmartPtr<AssetsObject>>();
    }
    public class SmartPtrException : Exception
    {
        public SmartPtrException(string message, Exception innerException = null) : base(message, innerException)
        { }
    }


}
