using System;
using System.IO;
//using System.Collections.Generic;
//using System.Linq;

namespace Sync
{
    /// <summary>
    /// Logging class
    /// </summary>
    abstract class Log
    {
        public abstract void WriteLine(string message);
        public void WriteLine(string message, object one)
        {
            WriteLine(String.Format(message, one));
        }
        public void WriteLine(string message, object one, object two)
        {
            WriteLine(String.Format(message, one, two));
        }
        public void WriteLine(string message, object one, object two, object three)
        {
            WriteLine(String.Format(message, one, two, three));
        }
    }

    class ConsoleLog : Log
    {
        public override void WriteLine(string message)
        {
            Console.WriteLine(message);
        }
    }

    /// <summary>
    /// Logs to a file.  Supports IDisposable as need to close the file (buffer doesn't get flushed otherwise).
    /// </summary>
    sealed class FileLog : Log, IDisposable
    {
        private StreamWriter ourFile { get; set; }
        
        public FileLog(string logFileName)
        {
            ourFile = new StreamWriter(logFileName, false);
        }

        /// <summary>
        /// http://stackoverflow.com/a/1136252/3403507
        /// http://blog.nuclex-games.com/tutorials/idisposable-pattern/
        /// </summary>
        public void Dispose()
        {
            if (ourFile != null) { ourFile.Dispose(); }
        }

        public override void WriteLine(string message)
        {
            ourFile.WriteLine(message);
        }
    }
}
