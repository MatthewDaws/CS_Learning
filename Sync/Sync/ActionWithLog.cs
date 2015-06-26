using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Sync
{
    /// <summary>
    /// Encapsulate all of the actual file-system actions, with a view to (possibly) logging them.
    /// </summary>
    abstract class ActionWithLog
    {
        public Log Logger { get; private set; }
        public ActionWithLog(Log logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Copy a file, with possible logging.
        /// </summary>
        public abstract void Copy(string from, string to);
        /// <summary>
        /// Delete a file, with possible logging.
        /// </summary>
        public abstract void DeleteFile(string fileName);
        /// <summary>
        /// Delete a directory, with possible logging.
        /// </summary>
        public abstract void DeleteDirectory(string dirName);
        /// <summary>
        /// Create a directory.
        /// </summary>
        public abstract void CreateDirectory(string dirName);
    }

    class SimulateActions : ActionWithLog
    {
        public SimulateActions(Log logger) : base(logger) { }
        public override void Copy(string from, string to)
        {
            Logger.WriteLine("Copied '{0}' -> '{1}'", from, to);
        }
        public override void DeleteFile(string fileName)
        {
            Logger.WriteLine("Deleted file '{0}'", fileName);
        }
        public override void DeleteDirectory(string dirName)
        {
            Logger.WriteLine("Deleted dir '{0}'", dirName);
        }
        public override void CreateDirectory(string dirName)
        {
            Logger.WriteLine("Created dir '{0}'", dirName);
        }
    }

    class PerformActions : ActionWithLog
    {
        public PerformActions(Log logger) : base(logger) { }
        public override void Copy(string from, string to)
        {
            Logger.WriteLine("Copied '{0}' -> '{1}'", from, to);
            File.Copy(from, to);
        }
        public override void DeleteFile(string fileName)
        {
            Logger.WriteLine("Deleted file '{0}'", fileName);
            File.Delete(fileName);
        }
        public override void DeleteDirectory(string dirName)
        {
            Logger.WriteLine("Deleted dir '{0}'", dirName);
            Directory.Delete(dirName);
        }
        public override void CreateDirectory(string dirName)
        {
            Logger.WriteLine("Created dir '{0}'", dirName);
            Directory.CreateDirectory(dirName);
        }
    }
}
