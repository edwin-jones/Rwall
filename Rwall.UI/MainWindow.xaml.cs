using Rwall.Controls;
using Rwall.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Forms = System.Windows.Forms;

namespace Rwall
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// We use this static reference so we can expose instance members globally, primarily for the SelectedWallpaperStyle property.
        /// </summary>
        private static ComboBox s_currentWallpaperStyleComboBox;

        /// <summary>
        /// We use this static reference so we can allow other types send errors for the main window to display.
        /// </summary>
        private static TextBlock s_currentUserPromptTextBlock;

        /// <summary>
        /// We use this static reference so we can allow other types send errors for the main window to display.
        /// </summary>
        private static WrapPanel s_currentWallpaperWrapPanel;


        /// <summary>
        /// This property exposes the user's current selection for the style of wallpaper they want (centered, tiled etc.)
        /// </summary>
        public static Wallpaper.Style SelectedWallpaperStyle
        {
            get
            {
                var style = (Wallpaper.Style)s_currentWallpaperStyleComboBox.SelectedItem;
                return style;
            }
        }

      
        /// <summary>
        /// CTOR
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            //Make sure we set the static references to the currently in use WallpaperStyleComboBox, UserPromptTextBlock and WallpaperWrapPanel. THIS IS IMPORTANT.
            s_currentWallpaperStyleComboBox = WallpaperStyleComboBox;
            s_currentUserPromptTextBlock = UserPromptTextBlock;
            s_currentWallpaperWrapPanel = WallpaperWrapPanel;

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
            foreach (WallpaperControl childWallpaperControl in WallpaperWrapPanel.Children)
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
        public static Forms.Screen GetScreen(Window window)
        {
            return Forms.Screen.FromHandle(new WindowInteropHelper(window).Handle);
        }


        /// <summary>
        /// This method runs when the user presses a button, checks to see if that button was 'enter' and triggers a load of wallpapers from the given subreddit if so.
        /// </summary>
        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                GetWallpapersAsync(SubredditNameTextBox.Text);
            }
        }


        /// <summary>
        /// This method runs when the go button is clicked, and triggers a load of wallpapers from the given subreddit.
        /// </summary>
        private void OnGoButtonClick(object sender, RoutedEventArgs e)
        {
            GetWallpapersAsync(SubredditNameTextBox.Text);
        }


        /// <summary>
        /// This function get's a list of wallpaper URIs from the given subreddit.
        /// </summary>
        private List<Uri> GetWallpaperUris(String subReddit)
        {
            //only every get the top 20 wallpapers from reddit, or less.
            var wallpaperUris = Wallpaper.GetLatestWallpaperURLs(subReddit, Consts.DefaultWallpaperUrlLimit);
            return wallpaperUris;
        }


        /// <summary>
        /// This async method gets a list of all the Urls we want to use and creates wallpaper controls for each one on the main window.
        /// The images for each are loaded in seperate threads so they shouldn't block UI.
        /// </summary>
        private async void GetWallpapersAsync(String subReddit)
        {
            //make sure the subreddit name is shown if we get from something other than user input.
            SubredditNameTextBox.Text = subReddit;

            //flush out all old wallpapers.
            WallpaperWrapPanel.Children.Clear();

            //show the loading prompt
            UserPromptTextBlock.Visibility = Visibility.Visible;
            UserPromptTextBlock.Text = "Getting a list of images, this may take some time...";

            List<Uri> pictureUris = new List<Uri>();
            List<BitmapImage> pictureBitmaps = new List<BitmapImage>();

            //get the screen object so we can size pictures properly if the window is maximized
            var screen = GetScreen(this);

            try
            {
                //Get a list of the available wallpaper pictures async.
                await Task.Run(() => pictureUris = GetWallpaperUris(subReddit));

                if (pictureUris.Count < 1)
                {
                    UserPromptTextBlock.Text = Consts.NoWallpapersFoundErrorMessage;
                }
            }
            catch (System.Net.WebException webException) //catch web errors.
            {
                HandleWebException(webException);
            }

            foreach (var pictureUri in pictureUris) //create a new wallpaper control for each picture URL and show it on screen.
            {
                var wallpaperControl = new WallpaperControl();

               //Make sure the wallpaper control ALWAYS knows where the full size image lives.
                wallpaperControl.FullSizeImageUrl = pictureUri.OriginalString;

                var urlWithoutExtenstion = Path.GetFileNameWithoutExtension(pictureUri.OriginalString);

                //Try to filter out imgur URLs that aren't previews and MAKE them previews to speed loading.
                if (pictureUri.OriginalString.ToLower().Contains(Consts.ImgurDotCom) && urlWithoutExtenstion.Count() < 8)
                {
                   //get the index of the last '.' in the string, that is the start of the file extension
                   var previewCharInsertionIndex = pictureUri.OriginalString.LastIndexOf('.');

                   var newString = pictureUri.OriginalString.Insert(previewCharInsertionIndex, Consts.ImgurLargePreviewApiChar.ToString());
                   wallpaperControl.Image.Source = new BitmapImage(new Uri(newString));
                }
                else
                {
                    wallpaperControl.Image.Source = new BitmapImage(pictureUri);
                }

                //resize wallpapers for the correct window size
                wallpaperControl.Width = this.Width / Consts.WallpaperColumnSize;
                wallpaperControl.Height = this.Height / Consts.WallpaperColumnSize;

                if (this.WindowState == WindowState.Maximized)
                {
                    wallpaperControl.Width = screen.WorkingArea.Width / Consts.WallpaperColumnSize;
                    wallpaperControl.Height = screen.WorkingArea.Height / Consts.WallpaperColumnSize;
                }

                //Make sure we add this new wallpaper control to the container item.
                WallpaperWrapPanel.Children.Add(wallpaperControl);

                //hide the loading prompt
                UserPromptTextBlock.Visibility = Visibility.Collapsed;
            }
        }


        /// <summary>
        /// This method allows other types to let the main window display web errors.
        /// </summary>
        public static void HandleWebException(WebException webException)
        {
            //flush out all old wallpapers.
            s_currentWallpaperWrapPanel.Children.Clear();

            String errorMessage = Consts.CannotConnectErrorMessage;

            //catch http errors.
            if (webException.Status == WebExceptionStatus.ProtocolError)
            {
                var httpResponse = (HttpWebResponse)webException.Response;
                errorMessage = String.Format("Web Request Returned Error Code : {0}, {1}", (int)httpResponse.StatusCode, httpResponse.StatusCode);
            }

            //this isn't a server http error, can the user connect?
            s_currentUserPromptTextBlock.Text = errorMessage;
            s_currentUserPromptTextBlock.Visibility = Visibility.Visible;
        }
    }
}
