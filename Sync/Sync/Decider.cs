using System;
using System.Collections.Generic;
using System.Linq;

namespace Sync
{
    /// <summary>
    /// Make "decisions" on when to move files.  ABC with derived classes implementing
    /// different polices.
    /// </summary>
    abstract class Decision
    {
        protected Move mover { get; set; }
        public Decision(Move mover)
        {
            this.mover = mover;
        }
        /// <summary>
        /// Take decision in case both localFile and extrnalFile already exist
        /// </summary>
        public abstract void ProcessFile(SyncFileInfo localFile, SyncFileInfo externalFile);
        /// <summary>
        /// Take decision in case only localFile exists
        /// </summary>
        public abstract void ProcessFile(SyncFileInfo localFile, SyncFileDirName externalFile);
        /// <summary>
        /// Take decision in case only externalFile exists
        /// </summary>
        public abstract void ProcessFile(SyncFileDirName localFile, SyncFileInfo externalFile);
        /// <summary>
        /// Take decision about sub-directory when local exists but external does not.
        /// </summary>
        public abstract void ProcessDir(SyncDirectoryInfo localDir, SyncFileDirName externalDir);
        /// <summary>
        /// Take decision about sub-directory when external exists but local does not.
        /// </summary>
        public abstract void ProcessDir(SyncFileDirName localDir, SyncDirectoryInfo externalDir);
        /// <summary>
        /// A problem with using NTFS and FAT32 filesystems is that date/time resolution differs: you can
        /// copy a file from a NTFS to FAT32 system, and the two files will have a difference of a second
        /// or so in time.  As a bit of a hack, this function considers times to be the same if they differ
        /// by 2 seconds or less.
        /// </summary>
        protected int CompareTimes(DateTime one, DateTime two)
        {
            TimeSpan difference = one.Subtract(two);
            if ( -2 <= difference.TotalSeconds && difference.TotalSeconds <= 2 )
            {
                return 0;
            }
            return one.CompareTo(two);
        }
    }

    /// <summary>
    /// Subclass of Decision which copies from the local source to the external source
    /// </summary>
    class LocalToExternalDecider : Decision
    {
        public LocalToExternalDecider(Move mover) : base(mover) { }

        /// <summary>
        /// If local and external have different times, then copy local to external
        /// </summary>
        public override void ProcessFile(SyncFileInfo localFile, SyncFileInfo externalFile)
        {
            if ( CompareTimes(localFile.LastWriteTime, externalFile.LastWriteTime) != 0 )
            {
                mover.Replace(from: localFile, to: externalFile);
            }
        }
        /// <summary>
        /// localFile doesn't exist, so deletes external file.
        /// </summary>
        public override void ProcessFile(SyncFileDirName localFile, SyncFileInfo externalFile)
        {
            mover.Remove(externalFile);
        }
        /// <summary>
        /// External file doesn't exist so copy.
        /// </summary>
        public override void ProcessFile(SyncFileInfo localFile, SyncFileDirName externalFile)
        {
            mover.Copy(from: localFile, toFullName: externalFile);
        }
        /// <summary>
        /// Local directory exists but external does not, so copy
        /// </summary>
        public override void ProcessDir(SyncDirectoryInfo localDir, SyncFileDirName externalDir)
        {
 	        mover.CopyTree(from: localDir, toFullPath: externalDir);
        }
        /// <summary>
        /// Local directory does not exist, so delete external
        /// </summary>
        public override void ProcessDir(SyncFileDirName localDir, SyncDirectoryInfo externalDir)
        {
 	        mover.DeleteTree(externalDir);
        }
    }

    /// <summary>
    /// Subclass of Decision which copies from the external source to the local source
    /// </summary>
    class ExternalToLocalDecider : Decision
    {
        public ExternalToLocalDecider(Move mover) : base(mover) { }

        /// <summary>
        /// If local and external have different times, then copy external to local.
        /// </summary>
        public override void ProcessFile(SyncFileInfo localFile, SyncFileInfo externalFile)
        {
            if ( CompareTimes(localFile.LastWriteTime, externalFile.LastWriteTime) != 0 )
            {
                mover.Replace(from: externalFile, to: localFile);
            }
        }
        /// <summary>
        /// localFile doesn't exist, so copy.
        /// </summary>
        public override void ProcessFile(SyncFileDirName localFile, SyncFileInfo externalFile)
        {
            mover.Copy(from: externalFile, toFullName: localFile);
        }
        /// <summary>
        /// External file doesn't exist, so delete local file.
        /// </summary>
        public override void ProcessFile(SyncFileInfo localFile, SyncFileDirName externalFile)
        {
            mover.Remove(localFile);
        }
        /// <summary>
        /// Local directory exists but external does not, so delete local
        /// </summary>
        public override void ProcessDir(SyncDirectoryInfo localDir, SyncFileDirName externalDir)
        {
            mover.DeleteTree(localDir);
        }
        /// <summary>
        /// Local directory does not exist, so copy
        /// </summary>
        public override void ProcessDir(SyncFileDirName localDir, SyncDirectoryInfo externalDir)
        {
            mover.CopyTree(from: externalDir, toFullPath: localDir);
        }
    }

    /// <summary>
    /// Subclass of Decision which tries to update in both directions, picking newest files
    /// </summary>
    class MatchDecider : Decision
    {
        public MatchDecider(Move mover) : base(mover) { }

        /// <summary>
        /// If local and external have different times, then copy newest to replace older version.
        /// </summary>
        public override void ProcessFile(SyncFileInfo localFile, SyncFileInfo externalFile)
        {
            if ( CompareTimes(localFile.LastWriteTime, externalFile.LastWriteTime) > 0 )
            {
                mover.Replace(from: localFile, to: externalFile);
            }
            if ( CompareTimes(localFile.LastWriteTime, externalFile.LastWriteTime) < 0 )
            {
                mover.Replace(from: externalFile, to: localFile);
            }
        }
        /// <summary>
        /// localFile doesn't exist, so copy.
        /// </summary>
        public override void ProcessFile(SyncFileDirName localFile, SyncFileInfo externalFile)
        {
            mover.Copy(from: externalFile, toFullName: localFile);
        }
        /// <summary>
        /// External file doesn't exist, so copy.
        /// </summary>
        public override void ProcessFile(SyncFileInfo localFile, SyncFileDirName externalFile)
        {
            mover.Copy(from: localFile, toFullName: externalFile);
        }
        /// <summary>
        /// Local directory exists but external does not, so copy
        /// </summary>
        public override void ProcessDir(SyncDirectoryInfo localDir, SyncFileDirName externalDir)
        {
            mover.CopyTree(from: localDir, toFullPath: externalDir);
        }
        /// <summary>
        /// Local directory does not exist, so copy
        /// </summary>
        public override void ProcessDir(SyncFileDirName localDir, SyncDirectoryInfo externalDir)
        {
            mover.CopyTree(from: externalDir, toFullPath: localDir);
        }
    }
}
