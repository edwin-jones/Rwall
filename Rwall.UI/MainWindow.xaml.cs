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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Rwall.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var pictureUris = Wallpaper.GetLatestWallpaperURLs(Consts.DefaultSubreddit);

            MenuItem wallpaperSetMenuItem = new MenuItem();
            wallpaperSetMenuItem.Header = "Set This As Wallpaper";
            wallpaperSetMenuItem.Click += (obj, args) =>
                {
                    var menuItem = obj as MenuItem;
                    var cMenu = menuItem.Parent as ContextMenu;
                    var img = cMenu.PlacementTarget as Image;
                    var source = img.Source as BitmapImage;
                    Wallpaper.Set(source, Wallpaper.Style.Stretched);
                    //var uri = ((Image)obj.ContextMenu.Parent);
                };


            var contextMenu = new ContextMenu();
            contextMenu.Items.Add(wallpaperSetMenuItem);

            foreach (var uri in pictureUris.Take(20)) //we only want to use 20 pictures at a time.
            {
                var image = new Image();
                BitmapImage src = new BitmapImage();
                src.BeginInit();
                src.UriSource = uri;
                src.CacheOption = BitmapCacheOption.OnLoad;
                src.EndInit();

                image.Source = src;
                image.Width = Consts.DefaultPictureWidth;
                image.Height = Consts.DefaultPictureHeight;
                image.ContextMenu = contextMenu;

                WallPaperPanel.Children.Add(image);

                //WallpaperStackPanel.Children.Add(image);
            }
        }


    }
}
