using Rwall.Shared;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Rwall.Controls
{
    /// <summary>
    /// Interaction logic for Wallpaper.xaml
    /// </summary>
    public partial class WallpaperControl : UserControl
    {
        public String ImageSourceURI { get; set; }

        DropShadowEffect m_dropShadowEffect = new DropShadowEffect();

        public WallpaperControl()
        {
            InitializeComponent();

            m_dropShadowEffect.ShadowDepth = 0;
            m_dropShadowEffect.Color = Color.FromRgb(255, 255, 255); //white
            m_dropShadowEffect.Opacity = 1;
            m_dropShadowEffect.BlurRadius = 20;
        }

        private void Image_IsMouseDirectlyOverChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(Image.IsMouseOver)
            {
                BackingRectangle.Effect = m_dropShadowEffect;
 
            }
            else
            {
                BackingRectangle.Effect = null;
            }
        }

        private async void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            await Task.Factory.StartNew((Object imageSource) =>
                 {
                     Uri uri = new Uri(imageSource as String);
                     Wallpaper.Set(uri, (Wallpaper.Style.Stretched));
                 },  ImageSourceURI
             );
        }
    }
}
