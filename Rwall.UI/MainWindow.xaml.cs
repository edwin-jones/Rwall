using Rwall.Controls;
using Rwall.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Rwall
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// CTOR
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            //set wallpaperstyle combobox items and select the first/default value. We reverse the order so the 'stretched' value is first in the list.
            WallpaperStyleComboBox.ItemsSource = Enum.GetValues(typeof(Wallpaper.Style)).Cast<Wallpaper.Style>().OrderByDescending(e => e);

            WallpaperStyleComboBox.SelectedIndex = 0;

            GetWallpapersAsync(Consts.DefaultSubreddit);

        }

        /// <summary>
        /// This method is triggered whenever the size of the main window changes, and resizes wallpaper images.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_SizeChanged(object sender, EventArgs e)
        {

            //get the screen object so we can size pictures properly if the window is maximized
            var screen = GetScreen(this);

            //resize wallpapers for the correct window size
            foreach (WallpaperControl childWallpaperControl in WallPaperWrapPanel.Children)
            {
                childWallpaperControl.Width = this.Width / Consts.WallpaperColumnSize;
                childWallpaperControl.Height = this.Height / Consts.WallpaperColumnSize;

                if (this.WindowState == WindowState.Maximized)
                {
                    childWallpaperControl.Width = screen.WorkingArea.Width / Consts.WallpaperColumnSize;
                    childWallpaperControl.Height = screen.WorkingArea.Height / Consts.WallpaperColumnSize;
                }
            }
        }

        /// <summary>
        /// This helper function uses winforms methods to get an object that fully details the screen the current window is active on.
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        public static System.Windows.Forms.Screen GetScreen(Window window)
        {
            return System.Windows.Forms.Screen.FromHandle(new WindowInteropHelper(window).Handle);
        }

        /// <summary>
        /// This method runs when the go button is clicked, and triggers a load of wallpapers from the given subreddit.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGoButtonClick(object sender, RoutedEventArgs e)
        {
            GetWallpapersAsync(SubredditNameTextBox.Text);
        }

        /// <summary>
        /// This function get's a list of wallpaper URIs from the given subreddit.
        /// </summary>
        /// <param name="subReddit"></param>
        /// <returns></returns>
        private List<Uri> GetWallpaperUris(String subReddit)
        {
            var wallpaperUris = Wallpaper.GetLatestWallpaperURLs(subReddit);

            if (wallpaperUris.Count < 1)
            {
                MessageBox.Show(Consts.NoWallpapersFoundErrorMessage, Consts.AppErrorMessageTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return wallpaperUris.Take(20).ToList();
        }

        private List<BitmapImage> GetImages(List<Uri> pictureUris, System.Windows.Forms.Screen screen)
        {

            List<BitmapImage> results = new List<BitmapImage>();

            foreach (var uri in pictureUris) //we only want to use x pictures at a time.
            {

                BitmapImage src = new BitmapImage(uri);
                results.Add(src);
            }

            return results;
        }

        private async Task<ImageSource> LoadImageSourceAsync(string address)
        {
            ImageSource imgSource = null;

            using (WebClient webClient = new WebClient())
            {
                //no need to dispose memory stream, it is basically a byte array with no unmanaged resources.
                //if we dispose of it too early, images using it for data will look corrupted.
                MemoryStream ms = new MemoryStream(await new WebClient().DownloadDataTaskAsync(new Uri(address)));
                ImageSourceConverter imageSourceConverter = new ImageSourceConverter();
                imgSource = (ImageSource)imageSourceConverter.ConvertFrom(ms);
                
            }
            
            return imgSource;
        }

        /// <summary>
        /// Async wrapper around GetWallpapers.
        /// </summary>
        /// <param name="subReddit"></param>
        private async void GetWallpapersAsync(String subReddit)
        {
            //make sure the subreddit name is shown if we get from something other than user input.
            SubredditNameTextBox.Text = subReddit;

            //flush out all old wallpapers.
            WallPaperWrapPanel.Children.Clear();

            List<Uri> pictureUris = new List<Uri>();
            List<BitmapImage> pictureBitmaps = new List<BitmapImage>();

            //get the screen object so we can size pictures properly if the window is maximized
            var screen = GetScreen(this);

            await Task.Run(() => pictureUris = GetWallpaperUris(subReddit));
            //await Task.Run(() => pictureBitmaps = GetImages(pictureUris, screen));

            foreach (var pictureSource in pictureUris) //we only want to use x pictures at a time.
            {
                var wallpaperControl = new WallpaperControl();

                wallpaperControl.Margin = new Thickness(10);
                wallpaperControl.ImageSourceURI = pictureSource.ToString();

                //resize wallpapers for the correct window size
                wallpaperControl.Width = this.Width / Consts.WallpaperColumnSize;
                wallpaperControl.Height = this.Height / Consts.WallpaperColumnSize;

                if (this.WindowState == WindowState.Maximized)
                {
                    wallpaperControl.Width = screen.WorkingArea.Width / Consts.WallpaperColumnSize;
                    wallpaperControl.Height = screen.WorkingArea.Height / Consts.WallpaperColumnSize;
                }

                WallPaperWrapPanel.Children.Add(wallpaperControl);
            }

            for (int i = 0; i < pictureUris.Count(); i++)
            {
                var wallpaperControl = WallPaperWrapPanel.Children[i] as WallpaperControl;
                wallpaperControl.Image.Source = await LoadImageSourceAsync(pictureUris[i].ToString());
            }
        }
    }
}
