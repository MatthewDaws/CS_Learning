// Walktree class, which walks a directory tree and decides what to do

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
//using System.Text;
//using System.Threading.Tasks;

namespace Sync
{
    /// <summary>
    /// Enumerate for "origin" (currently local or external) which may have an effect on policy (e.g.
    ///    how much to prompt user etc.)
    /// </summary>
    public enum OriginEnum
    {
        Local,
        External
    };
    
    /// <summary>
    /// Cut-down version of FileInfo from BCL, with info about our notion of "origin"
    /// </summary>
    class SyncFileInfo
    {
        public string Name { get; private set; }
        public string FullName { get; private set; }
        public DateTime LastWriteTime { get; private set; }
        public OriginEnum Origin { get; private set; }

        public SyncFileInfo(string name, string fullName, DateTime lastWriteTime, OriginEnum origin)
        {
            Name = name; FullName = fullName; LastWriteTime = lastWriteTime; Origin = origin;
        }

        public override string ToString()
        {
 	         return String.Format("SyncFileInfo(Name: {0}, FullName: {1}, LastWriteTime: {2}, Origin: {3})",
                 Name, FullName, LastWriteTime, Origin);
        }
    }

    /// <summary>
    /// Stores a file or directory name, and Origin
    /// </summary>
    class SyncFileDirName
    {
        public string FullName { get; private set; }
        public OriginEnum Origin { get; private set; }

        public SyncFileDirName(string fullName, OriginEnum origin)
        {
            FullName = fullName; Origin = origin;
        }

        public override string ToString()
        {
            return String.Format("SyncFileDirName(FullName: {0}, Origin: {1})", FullName, Origin);
        }
    }

    /// <summary>
    /// Cut-down version of DirectoryInfo from BCL, with info about our notion of "origin"
    /// </summary>
    class SyncDirectoryInfo
    {
        public string Name { get; private set; }
        public string FullName { get; private set; }
        public OriginEnum Origin { get; private set; }

        public SyncDirectoryInfo(string name, string fullName, OriginEnum origin)
        {
            Name = name; FullName = fullName; Origin = origin;
        }

        public override string ToString()
        {
 	         return String.Format("SyncDirectoryInfo(Name: {0}, FullName: {1}, Origin: {2})",
                 Name, FullName, Origin);
        }
    }

    /// <summary>
    /// Walks down a directory tree, making decisions on what to do to each file/directory
    /// </summary>
    class WalkTree
    {
        private Options options { get; set; }
        private Decision decider { get; set; }
        private Move mover { get; set;}
        
        public WalkTree(Options options, Decision decider, Move mover)
        {
            this.options = options;
            this.decider = decider;
            this.mover = mover;
        }

        public void ProcessFromRoot(string root)
        {
            var toVisit = new Stack<string>();
            toVisit.Push(root);
            while ( toVisit.Count > 0)
            {
                string path = toVisit.Pop();
                string localPath = Path.Combine(options.LocalPath, path);
                string externalPath = Path.Combine(options.ExternalPath, path);
                var localDirInfo = new DirectoryInfo(localPath); // Exposes more information in Enumerate?? methods
                var externalDirInfo = new DirectoryInfo(externalPath);

                // Look at files
                Dictionary<string, SyncFileInfo> localFiles = (from file in localDirInfo.EnumerateFiles()
                                                           select new SyncFileInfo(name: file.Name, fullName: file.FullName,
                                                              lastWriteTime: file.LastWriteTimeUtc, origin: OriginEnum.Local)
                                                          ).ToDictionary(file => file.Name);
                Dictionary<string, SyncFileInfo> externalFiles = (from file in externalDirInfo.EnumerateFiles()
                                                               select new SyncFileInfo(name: file.Name, fullName: file.FullName,
                                                                  lastWriteTime: file.LastWriteTimeUtc, origin: OriginEnum.External)
                                                          ).ToDictionary(file => file.Name);
                var fileInBoth = from fileName in localFiles.Keys
                                 where externalFiles.ContainsKey(fileName)
                                 select new { Local = localFiles[fileName], External = externalFiles[fileName] };
                foreach (var pair in fileInBoth)
                {
                    decider.ProcessFile(pair.Local, pair.External);
                }
                var fileInLocalOnly = from fileName in localFiles.Keys
                                      where !externalFiles.ContainsKey(fileName)
                                      select new { Local = localFiles[fileName], External = new SyncFileDirName(fullName: Path.Combine(externalPath, fileName), origin: OriginEnum.External) };
                foreach (var pair in fileInLocalOnly)
                {
                    decider.ProcessFile(pair.Local, pair.External);
                }
                var fileInExternalOnly = from fileName in externalFiles.Keys
                                         where !localFiles.ContainsKey(fileName)
                                         select new { Local = new SyncFileDirName(fullName: Path.Combine(localPath, fileName), origin: OriginEnum.Local), External = externalFiles[fileName] };
                foreach (var pair in fileInExternalOnly)
                {
                    decider.ProcessFile(pair.Local, pair.External);
                }

                // Now look at directories, and decide which to recursively follow
                Dictionary<string, SyncDirectoryInfo> localDir = (from dir in localDirInfo.EnumerateDirectories()
                                                               select new SyncDirectoryInfo(name: dir.Name, fullName: dir.FullName,
                                                                  origin: OriginEnum.Local)
                                                          ).ToDictionary(dir => dir.Name);
                Dictionary<string, SyncDirectoryInfo> externalDir = (from dir in externalDirInfo.EnumerateDirectories()
                                                                  select new SyncDirectoryInfo(name: dir.Name, fullName: dir.FullName,
                                                                     origin: OriginEnum.External)
                                                          ).ToDictionary(dir => dir.Name); 
                // Different pattern, not using LINQ.  Not sure which I prefer...
                foreach (var dirName in localDir.Keys)
                {
                    if (externalDir.ContainsKey(dirName))
                    {
                        // Both exist so recurse
                        toVisit.Push(Path.Combine(path, dirName));
                    }
                    else
                    {
                        decider.ProcessDir(localDir[dirName], new SyncFileDirName(fullName: Path.Combine(externalPath, dirName), origin: OriginEnum.External));
                    }
                }
                foreach (var dirName in externalDir.Keys)
                {
                    if ( !localDir.ContainsKey(dirName))
                    {
                        decider.ProcessDir(new SyncFileDirName(fullName: Path.Combine(localPath, dirName), origin: OriginEnum.Local), externalDir[dirName]);
                    }
                }
            }
        }
    }
}
