using Rwall.Controls;
using Rwall.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            foreach(WallpaperControl childWallpaperControl in WallPaperWrapPanel.Children)
            {
                childWallpaperControl.Width = this.Width / Consts.WallpaperColumnSize;
                childWallpaperControl.Height = this.Height / Consts.WallpaperColumnSize;

                if(this.WindowState == WindowState.Maximized)
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

            return wallpaperUris;
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

            await Task.Run(() => pictureUris = GetWallpaperUris(subReddit));

            //get the screen object so we can size pictures properly if the window is maximized
            var screen = GetScreen(this);

            foreach (var uri in pictureUris.Take(20)) //we only want to use x pictures at a time.
            {
                var wallpaperControl = new WallpaperControl();
                BitmapImage src = new BitmapImage();
                src.BeginInit();
                src.UriSource = uri;
                src.CacheOption = BitmapCacheOption.OnLoad;
                src.EndInit();

                wallpaperControl.Margin = new Thickness(10);
                wallpaperControl.Image.Source = src;
                wallpaperControl.SnapsToDevicePixels = true;
                wallpaperControl.UseLayoutRounding = true;

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
        }
    }
}
