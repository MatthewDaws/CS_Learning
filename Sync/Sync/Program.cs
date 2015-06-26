using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace Sync
{
    // Main program
    //    - No attempt to catch exceptions: these should all be I/O related, and little we can do to recover.
    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            var responder = new ConsoleResponse();
            var message = new StringBuilder();
            message.Append("Syncronise files between local and external filesystems.\n");
            message.AppendFormat("Local = {0}\nExternal = {1}\n", options.LocalPath, options.ExternalPath);
            message.AppendFormat("Directories to sync: {0}\n", options.Directories);
            message.Append("\nOptions:\n");
            message.Append("[1] Syncronise both ways.\n");
            message.Append("[2] Copy from local to external.\n");
            message.Append("[3] Copy from external to local.\n");
            message.Append("[4] Syncronise both ways.  Simulate.\n");
            message.Append("[5] Copy from local to external.  Simulate.\n");
            message.Append("[6] Copy from external to local.  Simulate.\n");
            message.Append("Choice (other to quit):");
            string userChoice = responder.SeekString(message.ToString());
            int choice;
            if ( !Int32.TryParse(userChoice, out choice) )
            {
                return;
            }

            var logger = new ConsoleLog();
            FileLog mainLogger = null;
            // Want mainLogger to write even if exception
            try
            {

                ActionWithLog actioner;
                switch (choice)
                {
                    default:
                        return;
                    case 1:
                    case 2:
                    case 3:
                        mainLogger = new FileLog("synclog.txt");
                        mainLogger.WriteLine("Ran at {0}", DateTime.Now);
                        actioner = new PerformActions(mainLogger);
                        break;
                    case 4:
                    case 5:
                    case 6:
                        actioner = new SimulateActions(logger);
                        break;
                }
                Decision decider;
                Move mover;
                switch (choice)
                {
                    default:
                        return;
                    case 1:
                        mover = new MatchMover(actioner, responder);
                        decider = new MatchDecider(mover);
                        break;
                    case 2:
                        mover = new LocalToExternalMover(actioner, responder);
                        decider = new LocalToExternalDecider(mover);
                        break;
                    case 3:
                        mover = new ExternalToLocalMover(actioner, responder);
                        decider = new ExternalToLocalDecider(mover);
                        break;
                    case 4:
                        mover = new MatchSimulateMover(actioner, responder, logger);
                        decider = new MatchDecider(mover);
                        break;
                    case 5:
                        mover = new LocalToExternalSimulateMover(actioner, responder, logger);
                        decider = new LocalToExternalDecider(mover);
                        break;
                    case 6:
                        mover = new ExternalToLocalSimulateMover(actioner, responder, logger);
                        decider = new ExternalToLocalDecider(mover);
                        break;
                }

                var treeWalker = new WalkTree(options: options, decider: decider, mover: mover);
                foreach (var root in options.Directories)
                {
                    treeWalker.ProcessFromRoot(root);
                }

            }
            finally
            {
                if (mainLogger != null)
                {
                    mainLogger.WriteLine("Finished successfully at {0}", DateTime.Now);
                    mainLogger.Dispose();
                }
            }
        }
    }
}
