using Rwall.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rwall.CLI
{
    /// <summary>
    /// This program allows us to use the functionality of rwall from the command line.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var subreddit = args[0] ?? Consts.DefaultSubreddit;

            Console.WriteLine("Setting wallpaper from subreddit {0}", subreddit);
                
            var wallpaperUrl = Wallpaper.GetLatestWallpaperUrl(subreddit);

            Wallpaper.Set(wallpaperUrl, Wallpaper.Style.Stretched);

            Console.WriteLine("Finished Setting wallpaper");
        }
    }
}
