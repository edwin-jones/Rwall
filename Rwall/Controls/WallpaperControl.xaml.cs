using Rwall.Shared;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace Rwall.Controls
{
    /// <summary>
    /// Interaction logic for Wallpaper.xaml
    /// </summary>
    public partial class WallpaperControl : UserControl
    {
        /// <summary>
        /// We use this member to control what sort of drop shadow (if any) the control should have.
        /// </summary>
        DropShadowEffect m_dropShadowEffect = new DropShadowEffect();

        /// <summary>
        /// We use this to store the faddress of the full size wallpaper (not previews etc.)
        /// </summary>
        public String FullSizeImageUrl { get; set; }

        /// <summary>
        /// CTOR
        /// </summary>
        public WallpaperControl()
        {
            InitializeComponent();

            //Set up drop shadow
            m_dropShadowEffect.ShadowDepth = 0;
            m_dropShadowEffect.Color = Color.FromRgb(255, 255, 255); //white
            m_dropShadowEffect.Opacity = 1;
            m_dropShadowEffect.BlurRadius = 20;
        }

        /// <summary>
        /// This method fires when the user's cursor rolls over a wallpaper and gives it a glow to hightligh it.
        /// </summary>
        private void ForeGround_IsMouseDirectlyOverChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (ForegroundRectangle.IsMouseOver)
            {
                BackgroundRectangle.Effect = m_dropShadowEffect;

            }
            else
            {
                BackgroundRectangle.Effect = null;
            }
        }


        /// <summary>
        /// This method runs when an image is clicked and changes the wallpaper of the current user.
        /// </summary>
        private async void ForeGround_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //extract URI from the clicked image's source so we can pass into another thread that sets the desktop background (without blocking UI).
            //We have to store arguments in local variables and pass them as arguments so we don't get cross thread access errors.
            var wallpaperUri = new Uri(FullSizeImageUrl);
            var style = MainWindow.SelectedWallpaperStyle;

            try
            {
                //Change the wallpaper with an async call
                await Task.Run(() => ChangeWallpaper(wallpaperUri, style));
            }
            catch (WebException wex) //catch http errors and send them to the main window to display.
            {
                MainWindow.HandleWebException(wex);
            }
            catch (Exception ex)//Show serious exceptions to user and then throw the error (which will probably crash the application)
            {   
                System.Windows.MessageBox.Show("FATAL ERROR: " + ex.ToString(), Consts.AppErrorMessageTitle, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                throw ex;
            }
        }


        /// <summary>
        /// Private implementation of Wallpaper.Set. This method exists so we can call it in an async manner with Task.Run() etc.
        /// </summary>
        private void ChangeWallpaper(Uri wallpaperUri, Wallpaper.Style style)
        {
            Wallpaper.Set(wallpaperUri, style);
        }
    }
}
