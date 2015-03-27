using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rwall.Shared
{
    public static class Consts
    {
        public const String AppName = "Rwall";

        public const String DefaultSubreddit = "Wallpapers";

        public const String AppErrorMessageTitle = "Rwall Error";

        public const String NoWallpapersFoundErrorMessage = "No images found for that subreddit. Are you sure it exists?";

        public const Int32 DefaultPictureWidth = 256;

        public const Int32 DefaultPictureHeight = 256;

        /// <summary>
        /// The number to divide a window width/height by to make sure we always have 20 wallpapers on screen on any resize. (5 columns x 4 rows)
        /// </summary>
        public const Int32 WallpaperColumnSize = 6;
    }
}
