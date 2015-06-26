using System;
using System.Collections.Generic;
using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

namespace Sync
{
    /// <summary>
    /// Eventually, load options from a local .ini file or similar.
    /// </summary>
    class Options
    {
        public List<string> Directories { get; private set; }
        public string LocalPath { get; private set; }
        public string ExternalPath { get; private set; }

        public Options()
        {
            Directories = new List<string> {"test_dir"};
            LocalPath = @"c:\users\matthew\documents";
            ExternalPath = @"E:\";
        }
    }
}
