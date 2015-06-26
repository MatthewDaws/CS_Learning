using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

// TODO:
// There is a lot of code duplication below.
// Maybe _all_ the "action" code should be moved to Actioner (including DeleteTree etc.)
// and leave it up to Actioner to decide on "simulate" or not.

namespace Sync
{
    /// <summary>
    /// Executes commands to move file/directories (or simulate such a move).
    /// ABC with some partial implementations (e.g. TreeMove command)
    /// </summary>
    abstract class Move
    {
        public ActionWithLog Actioner { get; private set; }
        public GetResponse Responder { get; private set; }
        public Move(ActionWithLog actioner, GetResponse responder)
        {
            Actioner = actioner; Responder = responder;
        }

        public abstract void Replace(SyncFileInfo from, SyncFileInfo to);
        public abstract void Copy(SyncFileInfo from, SyncFileDirName toFullName);
        public abstract void Remove(SyncFileInfo file);
        public abstract void DeleteTree(SyncDirectoryInfo dir);
        public abstract void CopyTree(SyncDirectoryInfo from, SyncFileDirName toFullPath);
        // Useful helper functions
        /// <summary>
        /// Unconditionally delete a tree; to be called by derived class after, presumably, some sort
        /// of checking code.
        /// </summary>
        protected void DoDeleteTree(SyncDirectoryInfo dir)
        {
            var toDelete = new Stack<string>();
            toDelete.Push(dir.FullName);
            var emptyDirs = new List<string>();
            while ( toDelete.Count() > 0 )
            {
                string path = toDelete.Pop();
                foreach (var fileName in Directory.EnumerateFiles(path))
                {
                    Actioner.DeleteFile(fileName);
                }
                foreach (var dirName in Directory.EnumerateDirectories(path))
                {
                    toDelete.Push(Path.Combine(path,dirName));
                }
                emptyDirs.Add(path);
            }
            foreach (var dirName in emptyDirs)
            {
                Actioner.DeleteDirectory(dirName);
            }
        }
        /// <summary>
        /// Actually copy a tree; to be called by derived class
        /// </summary>
        protected void DoCopyTree(SyncDirectoryInfo from, SyncFileDirName toFullPath)
        {
            Actioner.CreateDirectory(toFullPath.FullName);
            var toCopy = new Stack<string>();
            toCopy.Push("");
            while (toCopy.Count() > 0)
            {
                string path = toCopy.Pop();
                string fromPath = Path.Combine(from.FullName, path);
                string toPath = Path.Combine(toFullPath.FullName, path);
                foreach (var fileName in Directory.EnumerateFiles(fromPath))
                {
                    // fileName contains full path, so need to extract filename!
                    string name = Path.GetFileName(fileName);
                    Actioner.Copy(Path.Combine(fromPath, name), Path.Combine(toPath, name));
                }
                foreach (var dirName in Directory.EnumerateDirectories(fromPath))
                {
                    var fullDirName = Path.Combine(path, Path.GetFileName(dirName));
                    Actioner.CreateDirectory(Path.Combine(toPath, fullDirName));
                    toCopy.Push(fullDirName);
                }
            }
        }
    }

    /// <summary>
    /// Simulate: Doesn't actually perform actions, but prompts etc. as if for real
    /// Policies: Warns about deleting from external, but not overwriting
    /// </summary>
    class LocalToExternalSimulateMover : Move
    {
        public Log Logger { get; private set; }

        public LocalToExternalSimulateMover(ActionWithLog actioner, GetResponse responder, Log logger)
            : base(actioner, responder) { Logger = logger; }

        public override void Replace(SyncFileInfo from, SyncFileInfo to)
        {
            Logger.WriteLine("Replace '{0}' -> '{1}'", from, to);
        }
        public override void Copy(SyncFileInfo from, SyncFileDirName toFullName)
        {
 	        Logger.WriteLine("Copy '{0}' -> '{1}'", from, toFullName);
        }
        public override void Remove(SyncFileInfo file)
        {
            if ( Responder.SeekYes(String.Format("Delete {0}", file.FullName)) )
            {
                Logger.WriteLine("Remove '{0}'", file);
            }
        }
        public override void DeleteTree(SyncDirectoryInfo dir)
        {
            if (Responder.SeekYes(String.Format("Delete all of tree {0}", dir.FullName)))
            {
                Logger.WriteLine("DeleteTree '{0}'", dir);
            }
        }
        public override void CopyTree(SyncDirectoryInfo from, SyncFileDirName toFullPath)
        {
            Logger.WriteLine("CopyTree '{0}' -> '{1}'", from, toFullPath);
        }
    }

    /// <summary>
    /// Policies: Warns about deleting from external, but not overwriting
    /// </summary>
    class LocalToExternalMover : Move
    {
        public LocalToExternalMover(ActionWithLog actioner, GetResponse responder)
            : base(actioner, responder) { }

        public override void Replace(SyncFileInfo from, SyncFileInfo to)
        {
            Actioner.DeleteFile(to.FullName);
            Actioner.Copy(from.FullName, to.FullName);
        }
        public override void Copy(SyncFileInfo from, SyncFileDirName toFullName)
        {
            Actioner.Copy(from.FullName, toFullName.FullName);
        }
        public override void Remove(SyncFileInfo file)
        {
            if (Responder.SeekYes(String.Format("Delete {0}", file.FullName)))
            {
                Actioner.DeleteFile(file.FullName);
            }
        }
        public override void DeleteTree(SyncDirectoryInfo dir)
        {
            if (Responder.SeekYes(String.Format("Delete all of tree {0}", dir.FullName)))
            {
                DoDeleteTree(dir);
            }
        }
        public override void CopyTree(SyncDirectoryInfo from, SyncFileDirName toFullPath)
        {
            DoCopyTree(from, toFullPath);
        }
    }

    /// <summary>
    /// Simulate: Doesn't actually perform actions, but prompts etc. as if for real
    /// Policies: Prompts user about any changes to the target filesystem
    /// </summary>
    class ExternalToLocalSimulateMover : Move
    {
        public Log Logger { get; private set; }

        public ExternalToLocalSimulateMover(ActionWithLog actioner, GetResponse responder, Log logger)
            : base(actioner, responder) { Logger = logger; }

        public override void Replace(SyncFileInfo from, SyncFileInfo to)
        {
            if (Responder.SeekYes(String.Format("Replace {0}", to.FullName)))
            {
                Logger.WriteLine("Replace '{0}' -> '{1}'", from, to);
            }
        }
        public override void Copy(SyncFileInfo from, SyncFileDirName toFullName)
        {
            Logger.WriteLine("Copy '{0}' -> '{1}'", from, toFullName);
        }
        public override void Remove(SyncFileInfo file)
        {
            if (Responder.SeekYes(String.Format("Delete {0}", file.FullName)))
            {
                Logger.WriteLine("Remove '{0}'", file);
            }
        }
        public override void DeleteTree(SyncDirectoryInfo dir)
        {
            if (Responder.SeekYes(String.Format("Delete all of tree {0}", dir.FullName)))
            {
                Logger.WriteLine("DeleteTree '{0}'", dir);
            }
        }
        public override void CopyTree(SyncDirectoryInfo from, SyncFileDirName toFullPath)
        {
            Logger.WriteLine("CopyTree '{0}' -> '{1}'", from, toFullPath);
        }
    }

    /// <summary>
    /// Simulate: Doesn't actually perform actions, but prompts etc. as if for real
    /// Policies: Prompts user about any changes to the target filesystem
    /// </summary>
    class ExternalToLocalMover : Move
    {
        public ExternalToLocalMover(ActionWithLog actioner, GetResponse responder)
            : base(actioner, responder) { }

        public override void Replace(SyncFileInfo from, SyncFileInfo to)
        {
            if (Responder.SeekYes(String.Format("Replace {0}", to.FullName)))
            {
                Actioner.DeleteFile(to.FullName);
                Actioner.Copy(from.FullName, to.FullName);
            }
        }
        public override void Copy(SyncFileInfo from, SyncFileDirName toFullName)
        {
            Actioner.Copy(from.FullName, toFullName.FullName);
        }
        public override void Remove(SyncFileInfo file)
        {
            if (Responder.SeekYes(String.Format("Delete {0}", file.FullName)))
            {
                Actioner.DeleteFile(file.FullName);
            }
        }
        public override void DeleteTree(SyncDirectoryInfo dir)
        {
            if (Responder.SeekYes(String.Format("Delete all of tree {0}", dir.FullName)))
            {
                DoDeleteTree(dir);
            }
        }
        public override void CopyTree(SyncDirectoryInfo from, SyncFileDirName toFullPath)
        {
            DoCopyTree(from, toFullPath);
        }
    }

    /// <summary>
    /// Simulate: Doesn't actually perform actions, but prompts etc. as if for real
    /// Policies: Prompts user for any deletions, and any overwriting of the _local_ filesystem
    /// </summary>
    class MatchSimulateMover : Move
    {
        public Log Logger { get; private set; }

        public MatchSimulateMover(ActionWithLog actioner, GetResponse responder, Log logger)
            : base(actioner, responder) { Logger = logger; }
        
        public override void Replace(SyncFileInfo from, SyncFileInfo to)
        {
            if ((to.Origin != OriginEnum.Local) ||
                (to.Origin == OriginEnum.Local && Responder.SeekYes(String.Format("Replace {0}", to.FullName))))
            {
                Logger.WriteLine("Replace '{0}' -> '{1}'", from, to);
            }
        }
        public override void Copy(SyncFileInfo from, SyncFileDirName toFullName)
        {
            Logger.WriteLine("Copy '{0}' -> '{1}'", from, toFullName);
        }
        public override void Remove(SyncFileInfo file)
        {
            if (Responder.SeekYes(String.Format("Delete {0}", file.FullName)))
            {
                Logger.WriteLine("Remove '{0}'", file);
            }
        }
        public override void DeleteTree(SyncDirectoryInfo dir)
        {
            if (Responder.SeekYes(String.Format("Delete all of tree {0}", dir.FullName)))
            {
                Logger.WriteLine("DeleteTree '{0}'", dir);
            }
        }
        public override void CopyTree(SyncDirectoryInfo from, SyncFileDirName toFullPath)
        {
            Logger.WriteLine("CopyTree '{0}' -> '{1}'", from, toFullPath);
        }
    }

    /// <summary>
    /// Policies: Prompts user for any deletions, and any overwriting of the _local_ filesystem
    /// </summary>
    class MatchMover : Move
    {
        public MatchMover(ActionWithLog actioner, GetResponse responder)
            : base(actioner, responder) { }
        
        public override void Replace(SyncFileInfo from, SyncFileInfo to)
        {
            if ( (to.Origin != OriginEnum.Local) ||
                ( to.Origin == OriginEnum.Local && Responder.SeekYes(String.Format("Replace {0}", to.FullName)) ) )
            {
                Actioner.DeleteFile(to.FullName);
                Actioner.Copy(from.FullName, to.FullName);
            }
        }
        public override void Copy(SyncFileInfo from, SyncFileDirName toFullName)
        {
            Actioner.Copy(from.FullName, toFullName.FullName);
        }
        public override void Remove(SyncFileInfo file)
        {
            if (Responder.SeekYes(String.Format("Delete {0}", file.FullName)))
            {
                Actioner.DeleteFile(file.FullName);
            }
        }
        public override void DeleteTree(SyncDirectoryInfo dir)
        {
            if (Responder.SeekYes(String.Format("Delete all of tree {0}", dir.FullName)))
            {
                DoDeleteTree(dir);
            }
        }
        public override void CopyTree(SyncDirectoryInfo from, SyncFileDirName toFullPath)
        {
            DoCopyTree(from, toFullPath);
        }
    }
}
