// Search class:
//   - Recursively walk a directory tree

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Sync
{
    /// <summary>
    /// Not sure we'll use this right now.
    /// </summary>
    class Search
    {
        static IEnumerable<string> WalkTree(string root)
        {
            var toVisit = new Stack<string>();
            toVisit.Push(root);
            while ( toVisit.Count > 0)
            {
                string path = toVisit.Pop();
                foreach (var dir in Directory.EnumerateDirectories(path))
                {
                    yield return dir;
                    toVisit.Push( Path.Combine(path,dir) );
                }
            }
        }
    }
}
