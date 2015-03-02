using Rwall.Shared;
using System;
using System.Diagnostics;
using System.Linq;
using System.Security;
//using Rwall.Shared;

namespace Rwall
{
    internal class Program
    {
        //program variables
        public const string AppName = "Rwall";
        public static bool AuthorisedToWriteToEventLog = true;

        /// <summary>
        /// errorcodes for the program.
        /// </summary>
        enum ErrorCode : int
        {
            SUCCESS = 0,
            FAILURE = 1
        }

        /// <summary>
        /// Main function of Rwall. Will run, parse arguments and set the wallpaper.
        /// </summary>
        /// <param name="args"></param>
        private static Int32 Main(string[] args)
        {
            try
            {
                EventLog.WriteEntry(AppName, "Attempting to get wallpaper...", EventLogEntryType.Information);
            }
            catch (SecurityException)
            {
                Program.AuthorisedToWriteToEventLog = false;
            }
            string text = args.ToList<string>().FirstOrDefault<string>();
            if (string.IsNullOrEmpty(text))
            {
                text = "wallpapers";
            }
            byte style = 2;
            if (args.Length > 1)
            {
                byte.TryParse(args[1], out style);
            }

            Uri latestWallpaperURL = Wallpaper.GetLatestWallpaperURL(text);
            if (latestWallpaperURL != null)
            {
                Wallpaper.Set(latestWallpaperURL, (Wallpaper.Style)style);

                if (Program.AuthorisedToWriteToEventLog)
                {
                    EventLog.WriteEntry(AppName, "Finished getting and setting wallpaper.", EventLogEntryType.Information);
                }

                return Convert.ToInt32(ErrorCode.SUCCESS);
            }
            else
            {
                return Convert.ToInt32(ErrorCode.FAILURE);
            }
        }
    }
}
