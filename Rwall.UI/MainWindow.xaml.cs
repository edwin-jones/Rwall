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

            foreach (var uri in pictureUris.Take(20)) //we only want to use x pictures at a time.
            {
                var image = new Image();
                BitmapImage src = new BitmapImage();
                src.BeginInit();
                src.UriSource = uri;
                src.CacheOption = BitmapCacheOption.OnLoad;
                src.EndInit();

                image.Margin = new Thickness(10);
                image.Source = src;
                image.Width = this.Width /  6;
                image.Height = this.Height / 6;
                image.ContextMenu = contextMenu;
                image.MouseLeftButtonDown += (obj, args) => { Wallpaper.Set((BitmapImage)image.Source, Wallpaper.Style.Stretched); };

                //image.ToolTip = String.Format("Width: {0}px Height: {1}px", image.Source.Width, image.Source.Height);

                WallPaperWrapPanel.Children.Add(image);

                //WallpaperStackPanel.Children.Add(image);
            }
        }

        private void Window_SizeChanged(object sender, EventArgs e)
        {
            var screen = GetScreen(this);


            foreach(Image childImg in WallPaperWrapPanel.Children)
            {
                childImg.Width = this.Width / 6;
                childImg.Height = this.Height / 6;

                if(this.WindowState == WindowState.Maximized)
                {
                    childImg.Width = screen.WorkingArea.Width / 6;
                    childImg.Height = screen.WorkingArea.Height / 6;
                }
            }
        }

        public static System.Windows.Forms.Screen GetScreen(Window window)
        {
            return System.Windows.Forms.Screen.FromHandle(new WindowInteropHelper(window).Handle);
        }


    }
}
