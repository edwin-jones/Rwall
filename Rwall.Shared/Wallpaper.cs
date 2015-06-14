using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace Rwall.Shared
{
    /// <summary>
    /// This static class exposes methods for changing the desktop wallpaper, as well as getting details of available images to use as wallpaper.
    /// </summary>
    public static class Wallpaper
    {
        /// <summary>
        /// This enum describes the style of wallpaper.
        /// </summary>
        public enum Style
        {
            Tiled,
            Centered,
            Stretched
        }

        /// <summary>
        /// Consts to define old windows api flags for interop call(s).
        /// </summary>
        private const int SPI_SETDESKWALLPAPER = 20;
        private const int SPIF_UPDATEINIFILE = 1;
        private const int SPIF_SENDWININICHANGE = 2;

        /// <summary>
        /// a static object to use to stop multithreaded calls using managed resources at the same time.
        /// </summary>
        private readonly static Object s_lockObject = new Object();

        /// <summary>
        /// Windows native code import so we can change the wallpaper using the native windows API.
        /// </summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);


        /// <summary>
        /// This function sets the wallpaper with the given web image and the givens style by passing in that image's URL.
        /// </summary>
        public static void Set(Uri uri, Wallpaper.Style style)
        {
            using (WebClient webclient = new WebClient())
            {
                Stream stream = webclient.OpenRead(uri.ToString());
                Set(stream, style);
            }
        }


        /// <summary>
        /// Internal implementation of Wallpaper.Set.
        /// </summary>
        private static void Set(Stream imageStream, Wallpaper.Style style)
        {
            lock (s_lockObject)
            {
                //save the image in the temp folder from the given image data stream.
                Image image = Image.FromStream(imageStream);
                string newWallpaperFileName = Path.Combine(Path.GetTempPath(), "wallpaper.bmp");
                image.Save(newWallpaperFileName, ImageFormat.Bmp);

                //Open the users desktop registry key so we can go in and change wallpaper settings.
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", true);
                switch (style)
                {
                    case Wallpaper.Style.Tiled:
                        registryKey.SetValue(Consts.WallpaperStyleRegKey, Consts.TiledStyleWallpaperStyleRegValue);
                        registryKey.SetValue(Consts.TileWallpaperRegKey, Consts.TiledStyleTileWallpaperRegValue);
                        break;
                    case Wallpaper.Style.Centered:
                        registryKey.SetValue(Consts.WallpaperStyleRegKey, Consts.CenteredStyleWallpaperStyleRegValue);
                        registryKey.SetValue(Consts.TileWallpaperRegKey, Consts.CenteredStyleTileWallpaperRegValue);
                        break;
                    default: //the default is stretched!
                        registryKey.SetValue(Consts.WallpaperStyleRegKey, Consts.StretchedStyleWallpaperStyleRegValue);
                        registryKey.SetValue(Consts.TileWallpaperRegKey, Consts.StretchedStyleTileWallpaperRegValue);
                        break;
                }

                //Use an interop method to change the wallpaper.
                Wallpaper.SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, newWallpaperFileName, SPIF_SENDWININICHANGE);          
            }
        }


        /// <summary>
        /// This function will get a random wallpaper URL from a given subreddit.
        /// </summary>
        public static Uri GetLatestWallpaperUrl(String subreddit)
        {
            Random random = new Random();

            var candidates = GetLatestWallpaperURLs(subreddit);
            var candidateIndex = random.Next(candidates.Count());

            return candidates[candidateIndex];
        }


        /// <summary>
        /// This method gets a the first X wallpapers from the given subreddit. Limited to 50 records by the Reddit API.
        /// </summary>
        public static List<Uri> GetLatestWallpaperURLs(String subreddit)
        {
            String address = String.Format("http://www.reddit.com/r/{0}.xml?limit={1}", subreddit, Consts.MaxWallpapersPerRequest);
            List<String> pictureUrlList = new List<string>();
 
                String redditXmlApiResponse = new WebClient().DownloadString(address);
                XDocument xDocument = XDocument.Parse(redditXmlApiResponse);
                IEnumerable<XElement> enumerable = xDocument.Descendants(XName.Get("description"));
                foreach (XElement current in enumerable)
                {
                    String[] redditXmlApiResponseElements = current.Value.ToString().Split(new Char[]
					{
						'"'
					});
                    for (int i = 0; i < redditXmlApiResponseElements.Length; i++)
                    {
                        if (redditXmlApiResponseElements[i].Contains("[link]"))
                        {
                            string redditImageUrl = redditXmlApiResponseElements[i - 1];
                            string redditImageUrlLowered = redditImageUrl.ToLower();

                            if (redditImageUrl.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) || redditImageUrl.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase))
                            {
                                pictureUrlList.Add(redditImageUrl);
                            }
                            //if we have a non direct imgur link THAT IS NOT AN ALBUM/GALLERY try to guess the correct URL.
                            else if (redditImageUrlLowered.Contains(Consts.ImgurDotCom)
                                && !redditImageUrlLowered.Contains(Consts.ImgurGalleryString)
                                && !redditImageUrlLowered.Contains(Consts.ImgurAlbumString))
                            {
                                var imgurId = redditImageUrl.Split('/').Last();
                                var realImgurUrl = String.Format("https://i.imgur.com/{0}.png", imgurId);

                                pictureUrlList.Add(realImgurUrl);
                            }
                        }
                    }
                }

            //Return the strings as URL/URIs.
            return pictureUrlList.Select(pic => new Uri(pic)).ToList();
        }
    }
}

