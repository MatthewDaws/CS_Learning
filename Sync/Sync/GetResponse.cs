using System;
//using System.Collections.Generic;
//using System.Linq;

namespace Sync
{
    abstract class GetResponse
    {
        /// <summary>
        /// Should print or display a meesage such as "[message] [y/N]"
        /// </summary>
        /// <param name="message">Message to display.  Yes/No prompts etc. will be added by method.</param>
        /// <returns>true if user selects "yes", false otherwise (and defaults to false)</returns>
        public abstract bool SeekYes(string message);
        /// <summary>
        /// Obtains a free-text response from the user.
        /// </summary>
        /// <param name="message">Message to display.</param>
        public abstract string SeekString(string message);
    }

    class ConsoleResponse : GetResponse
    {
        public override bool SeekYes(string message)
        {
            Console.Write("{0} [y/N]? ", message);
            string input = Console.ReadLine();
            if ( input == "Y" || input == "y" )
            {
                return true;
            }
            return false;
        }
        public override string SeekString(string message)
        {
            Console.Write(message);
            return Console.ReadLine();
        }
    }
}
