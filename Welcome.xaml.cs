using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using AriPlayer;

namespace Ari_Player
{
    public partial class Welcome : Window
    {
        public Welcome()
        {
            InitializeComponent();
            
        }

        private void Settings_click(object sender, RoutedEventArgs e)
        {
            Player playerWindow = new Player();
            playerWindow.Show();
            this.Close();
        }


    }
}
