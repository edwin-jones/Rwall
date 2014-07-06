using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace Rwall
{
    public static class Wallpaper
    {
        public enum Style : byte
        {
            Tiled,
            Centered,
            Stretched
        }

        private const int SPI_SETDESKWALLPAPER = 20;
        private const int SPIF_UPDATEINIFILE = 1;
        private const int SPIF_SENDWININICHANGE = 2;

        //cpp code import so we can change the wallpaper using the native windows API.
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        /// <summary>
        /// This function sets the wallpaper with the given web image and the givens style
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="style"></param>
        public static void Set(Uri uri, Wallpaper.Style style)
        {
            try
            {
                Stream stream = new WebClient().OpenRead(uri.ToString());
                Image image = Image.FromStream(stream);
                string text = Path.Combine(Path.GetTempPath(), "wallpaper.bmp");
                image.Save(text, ImageFormat.Bmp);
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", true);
                switch (style)
                {
                    case Wallpaper.Style.Tiled:
                        registryKey.SetValue("WallpaperStyle", 1.ToString());
                        registryKey.SetValue("TileWallpaper", 1.ToString());
                        break;
                    case Wallpaper.Style.Centered:
                        registryKey.SetValue("WallpaperStyle", 1.ToString());
                        registryKey.SetValue("TileWallpaper", 0.ToString());
                        break;
                    default:
                        registryKey.SetValue("WallpaperStyle", 2.ToString());
                        registryKey.SetValue("TileWallpaper", 0.ToString());
                        break;
                }
                Wallpaper.SystemParametersInfo(20, 0, text, 3);
            }
            catch (Exception ex)
            {
                if (!Program.AuthorisedToWriteToEventLog)
                {
                    throw ex;
                }
                EventLog.WriteEntry("Rwall", string.Format("{0}{1}{1}{2}{3}", new object[]
				{
					"Error Setting Wallpaper",
					Environment.NewLine,
					"Error: ",
					ex.Message
				}), EventLogEntryType.Error);
            }
        }

        /// <summary>
        /// This function will get a random wallpaper URL from a given subreddit.
        /// </summary>
        public static Uri GetLatestWallpaperURL(string subreddit)
        {
            string address = string.Format("http://www.reddit.com/r/{0}.xml?limit=50", subreddit);
            List<string> list = new List<string>();
            Uri result;
            try
            {
                string text = new WebClient().DownloadString(address);
                XDocument xDocument = XDocument.Parse(text);
                IEnumerable<XElement> enumerable = xDocument.Descendants(XName.Get("description"));
                foreach (XElement current in enumerable)
                {
                    string[] array = current.Value.ToString().Split(new char[]
					{
						'"'
					});
                    for (int i = 0; i < array.Length; i++)
                    {
                        if (array[i].Contains("[link]"))
                        {
                            string text2 = array[i - 1];
                            if (text2.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) || text2.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase))
                            {
                                list.Add(text2);
                            }
                        }
                    }
                }
                if (list.Count == 0)
                {
                    throw new Exception("No wallpapers found at source.");
                }
                Random random = new Random();
                result = new Uri(list[random.Next(list.Count - 1)]);
            }
            catch (Exception ex)
            {
                if (Program.AuthorisedToWriteToEventLog)
                {
                    EventLog.WriteEntry("Rwall", string.Format("{0}{1}{1}{2}{3}", new object[]
					{
						"Error Getting Wallpaper",
						Environment.NewLine,
						"Error: ",
						ex.Message
					}), EventLogEntryType.Error);
                }
                result = null;
            }
            return result;
        }
    }
}
